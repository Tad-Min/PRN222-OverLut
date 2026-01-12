using OverLut.Models.BusinessObjects;
using OverLut.Models.DTOs;
using Microsoft.AspNetCore.Http;

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
        Task<AttachmentDTO?> UploadAttachmentAsync(Microsoft.AspNetCore.Http.IFormFile file, Guid channelId, Guid userId);
        Task<(byte[] data, string contentType, string fileName)?> DownloadAttachmentAsync(Guid attachmentId);
        Task<AttachmentDTO?> GetAttachmentAsync(Guid attachmentId);
        // Chunked upload support
        Task<Guid?> StartUploadAsync(string fileName, string contentType);
        Task<bool> UploadChunkAsync(Guid fileBlobId, int sequenceNumber, byte[] data);
        Task<AttachmentDTO?> FinishUploadAsync(Guid fileBlobId, Guid channelId, long fileSize, Guid userId, string fileName, string contentType);
        #endregion
        #region ReadReceipt methods
        #endregion

    }
}
