using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly HerreraSystemContext _context;

    public UserRepository(HerreraSystemContext context)
        => _context = context;

    public async Task<User?> GetByUserNameAsync(string userName)
        => await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

    public async Task<User?> GetByIdWithRoleAsync(int id)
        => await _context.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<List<string>> GetUserRolesAsync(int userId)
        => await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.RoleName)
            .ToListAsync();

    public async Task<Role?> GetRoleByNameAsync(string roleName)
        => await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);

    public async Task<User?> GetByResetTokenAsync(string token)
        => await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token);

    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<User>> GetAllWithRolesAsync()
    => await _context.Users
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
        .ToListAsync();
}