using OverLut.Models.BusinessObjects;
using System;
using System.Collections.Generic;

namespace OverLut.Models.DAOs;

public class AttachmentDAO
{
    private readonly OverLutContext _context;
    public AttachmentDAO(OverLutContext context)
    {
        _context = context; 
    }

}
