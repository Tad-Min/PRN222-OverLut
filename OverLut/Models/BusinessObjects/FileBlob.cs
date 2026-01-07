using System;
using System.Collections.Generic;

namespace OverLut.Models.BusinessObjects;

public partial class FileBlob
{
    public Guid FileBlobId { get; set; }

    public bool? IsComplete { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<FileChunk> FileChunks { get; set; } = new List<FileChunk>();
}
