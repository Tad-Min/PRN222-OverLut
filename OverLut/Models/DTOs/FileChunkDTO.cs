using System;
using System.Collections.Generic;

namespace OverLut.Models.DTOs;

public partial class FileChunkDTO
{
    public Guid ChunkId { get; set; }

    public Guid FileBlobId { get; set; }

    public int SequenceNumber { get; set; }

    public byte[] Data { get; set; } = null!;

}
