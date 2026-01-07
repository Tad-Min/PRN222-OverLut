using System;
using System.Collections.Generic;

namespace OverLut.Models.DTOs;

public partial class ChannelMemberDTO
{
    public Guid ChannelId { get; set; }

    public Guid UserId { get; set; }

    public string? Nickname { get; set; }

    public int? MemberRole { get; set; }

    public int? Permissions { get; set; }

}
