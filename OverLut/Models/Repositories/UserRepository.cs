using OverLut.Models.BusinessObjects;
using OverLut.Models.DAOs;
using OverLut.Models.DTOs;

namespace OverLut.Models.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDAO _userDAO;
        public UserRepository(UserDAO userDAO)
        {
            _userDAO = userDAO;
        }
        public async Task<bool> CreateUserAsync(UserDTO user)
        {
            try
            {
                var newUser = new User
                {
                    UserName = user.UserName,
                    Password = user.Password,
                    Name = user.Name,
                    Email = user.Email,
                    RoleId = user.RoleId
                };

                await _userDAO.CreateUserAsync(newUser);

                return true;
            }
            catch
            {
                return false;
            }

        }
        public async Task<UserDTO> GetUserByIdAsync(Guid userID)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> LoginUser(string username, string password)
        {
            try
            {
                return await _userDAO.LoginUserAsync(username, password);
            }
            catch
            {
                return false;
            }
        }
        public async Task<UserDTO> UpdateUserAsync()
        {
            throw new NotImplementedException();
        }
        public async Task<bool> DeleteUserAsync(Guid userID)
        {
            throw new NotImplementedException();
        }
    }
}
