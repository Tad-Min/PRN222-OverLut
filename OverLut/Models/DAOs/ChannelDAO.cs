using OverLut.Models.BusinessObjects;
using System;
using System.Collections.Generic;

namespace OverLut.Models.DAOs;

public class ChannelDAO
{
    private readonly OverLutContext _context;
    public ChannelDAO(OverLutContext context)
    {
        _context = context;
    }

    // Add channel
    public async Task AddChannelAsync(Channel channel)
    {
        await _context.Channels.AddAsync(channel);
        await _context.SaveChangesAsync();
    }

    // delete channel
    public async Task DeleteChannelAsync(Channel channel)
    {
        _context.Channels.Remove(channel);
        await _context.SaveChangesAsync();
    }
}
