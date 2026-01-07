using System;
using System.Collections.Generic;

namespace OverLut.Models.DTOs;

public partial class FileBlobDTO
{
    public Guid FileBlobId { get; set; }

    public bool? IsComplete { get; set; }

    public DateTime? CreatedAt { get; set; }

}
