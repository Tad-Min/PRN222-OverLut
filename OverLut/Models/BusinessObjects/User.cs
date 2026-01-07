using System;
using System.Collections.Generic;

namespace OverLut.Models.BusinessObjects;

public partial class User
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public int? RoleId { get; set; }

    public virtual ICollection<ChannelMember> ChannelMembers { get; set; } = new List<ChannelMember>();

    public virtual Role? Role { get; set; }
}
