using OverLut.Models.DTOs;

namespace OverLut.Models.Repositories
{
    public interface IUserRepository
    {

        Task<bool> CreateUserAsync(UserDTO user);
        Task<UserDTO> GetUserByIdAsync(Guid userID);
        Task<Guid?> LoginUser(string username, string password);
        Task<UserDTO> UpdateUserAsync();
        Task<bool> DeleteUserAsync(Guid userID);
    }
}
