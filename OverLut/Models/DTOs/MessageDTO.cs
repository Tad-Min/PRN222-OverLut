using System;
using System.Collections.Generic;

namespace OverLut.Models.DTOs;

public partial class MessageDTO
{
    public Guid MessageId { get; set; }

    public Guid ChannelId { get; set; }

    public Guid UserId { get; set; }

    public int? MessageType { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreateAt { get; set; }

}
