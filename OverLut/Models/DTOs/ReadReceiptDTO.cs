using System;
using System.Collections.Generic;

namespace OverLut.Models.DTOs;

public partial class ReadReceiptDTO
{
    public Guid ChannelId { get; set; }

    public Guid UserId { get; set; }

    public Guid? LastReadMessageId { get; set; }

    public DateTime? LastReadTime { get; set; }

}
