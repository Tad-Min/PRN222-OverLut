using System;
using System.Collections.Generic;

namespace OverLut.Models.BusinessObjects;

public partial class ReadReceipt
{
    public Guid ChannelId { get; set; }

    public Guid UserId { get; set; }

    public Guid? LastReadMessageId { get; set; }

    public DateTime? LastReadTime { get; set; }

    public virtual Message? Message { get; set; }
}
