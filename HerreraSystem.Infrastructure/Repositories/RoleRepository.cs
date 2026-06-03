using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly HerreraSystemContext _context;

    public RoleRepository(HerreraSystemContext context)
        => _context = context;

    public async Task<IEnumerable<Role>> GetAllAsync()
        => await _context.Roles.ToListAsync();

    public async Task<Role?> GetByIdAsync(int id)
        => await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Role?> GetByNameAsync(string roleName)
        => await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);

    public async Task AddAsync(Role role)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Role role)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
    }
}