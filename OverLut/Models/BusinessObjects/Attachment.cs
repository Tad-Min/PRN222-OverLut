using System;
using System.Collections.Generic;

namespace OverLut.Models.BusinessObjects;

public partial class Attachment
{
    public Guid AttachmentId { get; set; }

    public Guid MessageId { get; set; }

    public Guid ChannelId { get; set; }

    public Guid UserId { get; set; }

    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public int? Width { get; set; }

    public int? Height { get; set; }

    public int? Duration { get; set; }

    public long FileSize { get; set; }

    public Guid FileBlobId { get; set; }

    public virtual Message Message { get; set; } = null!;
}
