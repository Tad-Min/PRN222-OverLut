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
                
                return true;
            }
            catch {
                return false;
            }
        }
        public Task<IEnumerable<ChannelDTO>> GetAllChannelByUserIDAsync(Guid userID)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<ChannelDTO>> GetAllChannelByChannelName(string ChannelName)
        {
            throw new NotImplementedException();
        }
        public Task<ChannelDTO> EditChannelAsync()
        {
            throw new NotImplementedException(); 
        }
        public Task<bool> UpdateChannelAsync()
        {
            throw new NotImplementedException();
        }
        public Task<bool> DeleteChannelAsync()
        {
            throw new NotImplementedException();
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
                await _messageDAO.CreateMessageAsync(new Message
                {
                    UserId = message.UserId,
                    ChannelId = message.ChannelId,
                    MessageType = message.MessageType,
                    Content = message.Content,
                });
                return true;
            }
            catch
            {
                return false;
            }
        }
        public Task<IEnumerable<MessageDTO>> GetMessagesAsync()
        {
            throw new NotImplementedException();
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
