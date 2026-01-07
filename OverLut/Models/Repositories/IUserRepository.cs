using OverLut.Models.DTOs;

namespace OverLut.Models.Repositories
{
    public interface IUserRepository
    {
        
        Task<bool> CreateUserAsync();
        Task<UserDTO> GetUserByIdAsync(Guid UserID);
        Task<UserDTO> UpdateUserAsync();
        Task<bool> DeleteUserAsync(Guid UserID);
    }
}
