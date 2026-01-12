using Newtonsoft.Json;
using OverLut.Models.BusinessObjects;
using OverLut.Models.DAOs;
using OverLut.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Channel = OverLut.Models.BusinessObjects.Channel;

namespace OverLut.Models.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly AttachmentDAO _attachmentDAO;
        private readonly ChannelDAO _channelDAO;
        private readonly ChannelMemberDAO _ChannelMemberDAO;
        private readonly MessageDAO _messageDAO;
        private readonly ReadReceiptDAO _readReceiptDAO;
        private readonly UserDAO _userDAO;
        
        public ChatRepository(
            AttachmentDAO attachmentDAO,
            ChannelDAO channelDAO,
            ChannelMemberDAO channelMemberDAO,
            MessageDAO messageDAO,
            ReadReceiptDAO readReceiptDAO,
            UserDAO userDAO)
        {
            _attachmentDAO = attachmentDAO;
            _channelDAO = channelDAO;
            _ChannelMemberDAO = channelMemberDAO;
            _messageDAO = messageDAO;
            _readReceiptDAO = readReceiptDAO;
            _userDAO = userDAO;
        }
        #region Channel methods
        public async Task<bool> CreateChannelAsync()
        {
            try
            {
                var channel = new Channel
                {
                    ChannelType = 0,
                    ChannelName = "New Channel",
                    DefaultPermissions = 0,
                    CreateAt = DateTime.UtcNow,
                };

                await _channelDAO.AddChannelAsync(channel);
                return true;
            }
            catch {
                return false;
            }
        }
        public async Task<IEnumerable<ChannelDTO>?> GetAllChannelByUserIDAsync(Guid userID)
        {
            return await _channelDAO.GetAllChannelsByUserIDAsync(userID);
        }
        public async Task<IEnumerable<ChannelDTO>> GetAllChannelByChannelName(string channelName)
        {
            var channels = await _channelDAO.GetChannelsByNameAsync(channelName);

            var ChannelByName = channels.Select(c => new ChannelDTO
            {
                ChannelId = c.ChannelId,
                ChannelName = c.ChannelName,
                ChannelType = c.ChannelType,
                DefaultPermissions = c.DefaultPermissions,
                CreateAt = c.CreateAt
            }).ToList();

            return ChannelByName;
        }
        public async Task<ChannelDTO> EditChannelAsync(Guid channelId)
        {
            var channel = await _channelDAO.GetChannelByIdAsync(channelId);
            if (channel == null) return null;

            return new ChannelDTO
            {
                ChannelId = channel.ChannelId,
                ChannelName = channel.ChannelName,
                ChannelType = channel.ChannelType,
                DefaultPermissions = channel.DefaultPermissions,
                CreateAt = channel.CreateAt
            };
        }
        public async Task<bool> UpdateChannelAsync(ChannelDTO channelDto)
        {
            if (channelDto == null) return false;

            var channel = await _channelDAO.GetChannelByIdAsync(channelDto.ChannelId);
            if (channel == null) return false;

            
            channel.ChannelName = channelDto.ChannelName;
            channel.ChannelType = channelDto.ChannelType;
            channel.DefaultPermissions = channelDto.DefaultPermissions;

            try
            {
                await _channelDAO.UpdateChannelAsync(channel);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        // Create message (used when uploading attachments that should be associated with a message)
        public async Task<MessageDTO?> CreateMessageAsync(MessageDTO message)
        {
            if (message == null) return null;

            var messageEntity = new Message
            {
                UserId = message.UserId,
                ChannelId = message.ChannelId,
                MessageType = message.MessageType,
                Content = message.Content ?? string.Empty,
                CreateAt = DateTime.UtcNow
            };

            await _messageDAO.CreateMessageAsync(messageEntity);

            message.MessageId = messageEntity.MessageId;
            message.CreateAt = messageEntity.CreateAt;

            return message;
        }

        public async Task<AttachmentDTO?> UploadAttachmentAsync(Microsoft.AspNetCore.Http.IFormFile file, Guid channelId, Guid userId)
        {
            if (file == null) return null;

            // Create an empty message to attach the file to
            var msg = new MessageDTO
            {
                ChannelId = channelId,
                UserId = userId,
                MessageType = 2, // file
                Content = string.Empty
            };

            var created = await CreateMessageAsync(msg);
            if (created == null) return null;

            // delegate to DAO to save blob/chunk and attachment record
            var attachment = await _attachmentDAO.SaveAttachmentAsync(file, channelId, created.MessageId, userId);

            // broadcast message DTO to members (filename as content)
            try
            {
                var broadcast = new
                {
                    MessageId = created.MessageId,
                    ChannelId = created.ChannelId,
                    UserId = created.UserId,
                    MessageType = created.MessageType,
                    Content = attachment != null ? attachment.FileName : string.Empty,
                    AttachmentId = attachment?.AttachmentId,
                    FileBlobId = attachment?.FileBlobId,
                    FileName = attachment?.FileName,
                    ContentType = attachment?.ContentType,
                    CreateAt = created.CreateAt
                };

                var jsonResponse = JsonConvert.SerializeObject(broadcast);

                var memberIds = await _ChannelMemberDAO.GetUserIdsByChannelIdAsync(channelId);
                var sendTasks = memberIds
                    .Where(m => m != null)
                    .Select(m => ChatWebSocketHandler.SendToUserAsync(m.UserId.ToString(), jsonResponse))
                    .ToArray();

                if (sendTasks.Length > 0)
                {
                    await Task.WhenAll(sendTasks);
                }
            }
            catch
            {
                // ignore broadcast errors
            }

            return attachment;
        }

        // Chunked upload flow
        public async Task<Guid?> StartUploadAsync(string fileName, string contentType)
        {
            return await _attachmentDAO.StartUploadAsync(fileName, contentType);
        }

        public async Task<bool> UploadChunkAsync(Guid fileBlobId, int sequenceNumber, byte[] data)
        {
            return await _attachmentDAO.SaveChunkAsync(fileBlobId, sequenceNumber, data);
        }

        public async Task<AttachmentDTO?> FinishUploadAsync(Guid fileBlobId, Guid channelId, long fileSize, Guid userId, string fileName, string contentType)
        {
            // create message first
            var msg = new MessageDTO
            {
                ChannelId = channelId,
                UserId = userId,
                MessageType = 2,
                Content = string.Empty
            };

            var created = await CreateMessageAsync(msg);
            if (created == null) return null;

            // mark blob complete
            await _attachmentDAO.CompleteBlobAsync(fileBlobId);

            var attachment = await _attachmentDAO.CreateAttachmentFromBlobAsync(fileBlobId, channelId, created.MessageId, userId, fileName, contentType, fileSize);

            // broadcast similar to single-file flow
            try
            {
                var broadcast = new
                {
                    MessageId = created.MessageId,
                    ChannelId = created.ChannelId,
                    UserId = created.UserId,
                    MessageType = created.MessageType,
                    Content = attachment != null ? attachment.FileName : string.Empty,
                    AttachmentId = attachment?.AttachmentId,
                    FileBlobId = attachment?.FileBlobId,
                    FileName = attachment?.FileName,
                    ContentType = attachment?.ContentType,
                    CreateAt = created.CreateAt
                };

                var jsonResponse = JsonConvert.SerializeObject(broadcast);

                var memberIds = await _ChannelMemberDAO.GetUserIdsByChannelIdAsync(channelId);
                var sendTasks = memberIds
                    .Where(m => m != null)
                    .Select(m => ChatWebSocketHandler.SendToUserAsync(m.UserId.ToString(), jsonResponse))
                    .ToArray();

                if (sendTasks.Length > 0)
                {
                    await Task.WhenAll(sendTasks);
                }
            }
            catch
            {
                // ignore
            }

            return attachment;
        }
        
        public async Task<AttachmentDTO?> GetAttachmentAsync(Guid attachmentId)
        {
            return await _attachmentDAO.GetAttachmentByIdAsync(attachmentId);
        }

        public async Task<(byte[] data, string contentType, string fileName)?> DownloadAttachmentAsync(Guid attachmentId)
        {
            var att = await _attachmentDAO.GetAttachmentByIdAsync(attachmentId);
            if (att == null) return null;

            var blobBytes = await _attachmentDAO.ReadBlobAsync(att.FileBlobId);
            if (blobBytes == null) return null;

            return (blobBytes, att.ContentType, att.FileName);
        }
        public async Task<bool> DeleteChannelAsync(Guid channelId )
        {
            var channel = await _channelDAO.GetChannelByIdAsync(channelId);
            if (channel == null) return false;

            try
            {
                await _ChannelMemberDAO.RemoveMembersByChannelIdAsync(channelId);

                await _channelDAO.DeleteChannelAsync(channel);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region member methods
        public async Task<bool> AddMemberToChannelAsync(ChannelMemberDTO memberDto)
        {
            if (memberDto == null) return false;

            // Validate channel exists
            var channel = await _channelDAO.GetChannelByIdAsync(memberDto.ChannelId);
            if (channel == null) return false;

            // Prevent duplicate membership
            var existing = await _ChannelMemberDAO.GetMemberAsync(memberDto.ChannelId, memberDto.UserId);
            if (existing != null) return false;

            var entity = new ChannelMember
            {
                ChannelId = memberDto.ChannelId,
                UserId = memberDto.UserId,
                Nickname = memberDto.Nickname,
                MemberRole = memberDto.MemberRole,
                Permissions = memberDto.Permissions
            };

            try
            {
                await _ChannelMemberDAO.AddMemberToChannelAsync(entity);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> RemoveMemberFromChannelAsync(Guid channelId, Guid userId)
        {
            // ensure member exists
            var existing = await _ChannelMemberDAO.GetMemberAsync(channelId, userId);
            if (existing == null) return false;

            try
            {
                await _ChannelMemberDAO.RemoveMemberAsync(channelId, userId);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> UdateMemberNickName(Guid channelId, Guid userId, string? nickname)
        {
            // ensure member exists
            var existing = await _ChannelMemberDAO.GetMemberAsync(channelId, userId);
            if (existing == null) return false;

            try
            {
                await _ChannelMemberDAO.UpdateMemberNicknameAsync(channelId, userId, nickname);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Message methods
        public async Task<bool> SendMessageAsync(MessageDTO message)
        {
            try
            {
                // Optional validation: ensure user is a member of the channel
                var memberIds = await _ChannelMemberDAO.GetUserIdsByChannelIdAsync(message.ChannelId);
                if (memberIds == null || !memberIds.Any(m => m.UserId == message.UserId))
                {
                    // sender is not a channel member - reject
                    return false;
                }

                var messageEntity = new Message
                {
                    UserId = message.UserId,
                    ChannelId = message.ChannelId,
                    MessageType = message.MessageType,
                    Content = message.Content,
                    CreateAt = DateTime.UtcNow
                };
                await _messageDAO.CreateMessageAsync(messageEntity);

                message.MessageId = messageEntity.MessageId;
                message.CreateAt = messageEntity.CreateAt;

                var jsonResponse = JsonConvert.SerializeObject(message);

                // Broadcast concurrently to all members
                // FIX: use member.UserId.ToString() instead of member.ToString()
                var sendTasks = memberIds
                    .Where(m => m != null)
                    .Select(m => ChatWebSocketHandler.SendToUserAsync(m.UserId.ToString(), jsonResponse))
                    .ToArray();

                if (sendTasks.Length > 0)
                {
                    await Task.WhenAll(sendTasks);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<IEnumerable<MessageDTO>> GetMessagesByChannelIDAsync(Guid channelId, int page = 1, int page_size = 20)
        {
            try
            {
                return await _messageDAO.GetMessagesByChannelIDAsync(channelId, page, page_size);
            }
            catch
            {
                throw;
            }
        }
        public Task<bool> DeleteMessageAsync()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region Attachment methods
        #endregion
        #region ReadReceipt methods
        #endregion
    }
}
