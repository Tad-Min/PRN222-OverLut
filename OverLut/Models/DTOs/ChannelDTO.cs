using System;
using System.Collections.Generic;

namespace OverLut.Models.DTOs;

public partial class ChannelDTO
{
    public Guid ChannelId { get; set; }

    public int? ChannelType { get; set; }

    public string? ChannelName { get; set; }

    public int? DefaultPermissions { get; set; }

    public DateTime? CreateAt { get; set; }

}
