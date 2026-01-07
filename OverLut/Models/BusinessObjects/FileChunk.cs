using System;
using System.Collections.Generic;

namespace OverLut.Models.BusinessObjects;

public partial class FileChunk
{
    public Guid ChunkId { get; set; }

    public Guid FileBlobId { get; set; }

    public int SequenceNumber { get; set; }

    public byte[] Data { get; set; } = null!;

    public virtual FileBlob FileBlob { get; set; } = null!;
}
