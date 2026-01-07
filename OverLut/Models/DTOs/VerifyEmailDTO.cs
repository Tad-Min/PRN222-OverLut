using System;
using System.Collections.Generic;

namespace OverLut.Models.DTOs;

public partial class VerifyEmailDTO
{
    public Guid Key { get; set; }

    public string Email { get; set; } = null!;

    public long Time { get; set; }
}
