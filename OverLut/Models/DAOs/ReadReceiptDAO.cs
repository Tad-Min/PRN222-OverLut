using Microsoft.EntityFrameworkCore;
using OverLut.Models.BusinessObjects;
using System;
using System.Collections.Generic;

namespace OverLut.Models.DAOs;

public class ReadReceiptDAO
{
    private readonly OverLutContext _context;
    public ReadReceiptDAO(OverLutContext context)
    {
        _context = context;
    }
}
