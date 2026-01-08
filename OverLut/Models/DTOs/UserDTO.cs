using System;
using System.Collections.Generic;

namespace OverLut.Models.DTOs;

public partial class UserDTO
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public int? RoleId { get; set; }
}   
