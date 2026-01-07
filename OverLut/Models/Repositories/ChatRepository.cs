using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OverLut.Models.BusinessObjects;
using OverLut.Models.DAOs;
using Newtonsoft.Json;
using OverLut.Models.BusinessObjects;
using OverLut.Models.DAOs;
using OverLut.Models.DTOs;

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
        public async Task<IEnumerable<ChannelDTO>> GetAllChannelByUserIDAsync(Guid userID)
        {
            var channelsMembers = await _ChannelMemberDAO.GetChannelsFromUserIdAsync(userID);

            var channels = channelsMembers.Select(cm => new ChannelDTO
            {
                ChannelId = cm.Channel.ChannelId,
                ChannelType = cm.Channel.ChannelType,
                ChannelName = cm.Channel.ChannelName,
                DefaultPermissions = cm.Channel.DefaultPermissions,
                CreateAt = cm.Channel.CreateAt,
            }).ToList();
            return channels;
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
        public async Task<bool> DeleteChannelAsync(Guid channelId )
        {
            var channel = await _channelDAO.GetChannelByIdAsync(channelId);
            if (channel == null) return false;

            try
            {
                // Remove members first to avoid FK/PK conflicts
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
        public Task<bool> AddMemberToChannelAsync()
        {
            throw new NotImplementedException();

        }
        public Task<bool> RemoveMemberFromChannelAsync()
        {
            throw new NotImplementedException();
        }
        public Task<bool> UdateMemberNickName()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Message methods
        public async Task<bool> SendMessageAsync(MessageDTO message)
        {
            try
            {
                // Optional validation: ensure user is a member of the channel
                var memberIds = await _ChannelMemberDAO.GetUserIdsByChannelIdAsync(message.ChannelId);
                if (!memberIds.Any(m => m.UserId == message.UserId))
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
                var sendTasks = memberIds.Select(memberId => ChatWebSocketHandler.SendToUserAsync(memberId.ToString(), jsonResponse)).ToArray();
                await Task.WhenAll(sendTasks);

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
            catch (Exception ex)
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
