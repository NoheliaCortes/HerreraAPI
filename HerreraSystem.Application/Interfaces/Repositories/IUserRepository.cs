using HerreraSystem.Domain.Entities;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameAsync(string userName);
        Task<User?> GetByIdWithRoleAsync(int id);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task<User?> GetByResetTokenAsync(string token);
        Task<IEnumerable<User>> GetAllWithRolesAsync();
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdNumberAsync(string idNumber);
        Task<int> CountActiveAdminsAsync();
        Task<bool> HasInactiveRoleAsync(int userId);
        Task AddAsync(User user);
        Task UpdateAsync(User user);

    }
}