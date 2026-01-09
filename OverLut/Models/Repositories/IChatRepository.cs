using OverLut.Models.BusinessObjects;
using OverLut.Models.DTOs;

namespace OverLut.Models.Repositories
{
    public interface IChatRepository
    {
        #region Channel methods
        Task<bool> CreateChannelAsync();
        Task<IEnumerable<ChannelDTO>?> GetAllChannelByUserIDAsync(Guid userID);
        Task<IEnumerable<ChannelDTO>> GetAllChannelByChannelName(string ChannelName);
        Task<ChannelDTO> EditChannelAsync(Guid channelId);
        Task<bool> UpdateChannelAsync(ChannelDTO channel);
        Task<bool> DeleteChannelAsync(Guid channelId);
        #endregion

        #region member methods
        Task<bool> AddMemberToChannelAsync(ChannelMemberDTO member);
        Task<bool> RemoveMemberFromChannelAsync(Guid channelId, Guid userId);
        Task<bool> UdateMemberNickName(Guid channelId, Guid userId, string? nickname);
        #endregion

        #region Message methods
        Task<bool> SendMessageAsync(MessageDTO message);
        Task<IEnumerable<MessageDTO>> GetMessagesByChannelIDAsync(Guid channelId, int page = 1, int page_size = 20);
        Task<bool> DeleteMessageAsync();
        #endregion
        #region Attachment methods
        #endregion
        #region ReadReceipt methods
        #endregion

    }
}
