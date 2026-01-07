using OverLut.Models.DTOs;

namespace OverLut.Models.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly IChatRepository _chatRepository;
        public ChatRepository(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
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
        public Task<bool> SendMessageAsync()
        {
            throw new NotImplementedException();
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
