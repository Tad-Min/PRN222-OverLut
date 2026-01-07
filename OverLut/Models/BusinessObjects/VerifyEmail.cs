using System;
using System.Collections.Generic;

namespace OverLut.Models.BusinessObjects;

public partial class VerifyEmail
{
    public Guid Key { get; set; }

    public string Email { get; set; } = null!;

    public long Time { get; set; }
}
