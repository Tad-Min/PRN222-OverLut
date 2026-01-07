using OverLut.Models.BusinessObjects;
using System;
using System.Collections.Generic;

namespace OverLut.Models.DAOs;

public class UserDAO
{
    private readonly OverLutContext _context;
    public UserDAO(OverLutContext context)
    {
        _context = context;
    }

    public async Task CreateUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }


}
