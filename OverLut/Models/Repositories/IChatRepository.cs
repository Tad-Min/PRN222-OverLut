using OverLut.Models.BusinessObjects;
using OverLut.Models.DTOs;

namespace OverLut.Models.Repositories
{
    public interface IChatRepository
    {
        #region Channel methods
        Task<bool> CreateChannelAsync();
        Task<IEnumerable<ChannelDTO>> GetAllChannelByUserIDAsync(Guid userID);
        Task<IEnumerable<ChannelDTO>> GetAllChannelByChannelName(string ChannelName);
        Task<ChannelDTO> EditChannelAsync();
        Task<bool> UpdateChannelAsync();
        Task<bool> DeleteChannelAsync();
        #endregion

        #region member methods
        Task<bool> AddMemberToChannelAsync();
        Task<bool> RemoveMemberFromChannelAsync();
        Task<bool> UdateMemberNickName();
        #endregion

        #region Message methods
        Task<bool> SendMessageAsync(MessageDTO message);
        Task<IEnumerable<MessageDTO>> GetMessagesAsync();
        Task<bool> DeleteMessageAsync();
        #endregion
        #region Attachment methods
        #endregion
        #region ReadReceipt methods
        #endregion

    }
}
