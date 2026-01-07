using OverLut.Models.DTOs;

namespace OverLut.Models.Repositories
{
    public class UserRepository : IUserRepository
    {
        public Task<bool> CreateUserAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteUserAsync(Guid UserID)
        {
            throw new NotImplementedException();
        }

        public Task<UserDTO> GetUserByIdAsync(Guid UserID)
        {
            throw new NotImplementedException();
        }

        public Task<UserDTO> UpdateUserAsync()
        {
            throw new NotImplementedException();
        }
    }
}
