using Microsoft.EntityFrameworkCore;
using OverLut.Models.BusinessObjects;
using System;
using System.Collections.Generic;

namespace OverLut.Models.DAOs;

public class RoleDAO
{
    private readonly OverLutContext _context;
    public RoleDAO(OverLutContext context)
    {
        _context = context;
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<Role?> GetRoleByIdAsync(Guid roleId)
    {
        return await _context.Roles.FindAsync(roleId);
    }

}
