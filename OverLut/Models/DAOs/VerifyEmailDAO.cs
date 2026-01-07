using OverLut.Models.BusinessObjects;
using System;
using System.Collections.Generic;

namespace OverLut.Models.DAOs;

public class VerifyEmailDAO
{
    private readonly OverLutContext _context;
    public VerifyEmailDAO(OverLutContext context)
    {
        _context = context;
    }
}
