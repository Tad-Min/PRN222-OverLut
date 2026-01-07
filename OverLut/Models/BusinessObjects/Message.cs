using System;
using System.Collections.Generic;

namespace OverLut.Models.BusinessObjects;

public partial class Message
{
    public Guid MessageId { get; set; }

    public Guid ChannelId { get; set; }

    public Guid UserId { get; set; }

    public int? MessageType { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreateAt { get; set; }

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ChannelMember ChannelMember { get; set; } = null!;

    public virtual ICollection<ReadReceipt> ReadReceipts { get; set; } = new List<ReadReceipt>();
}
