using Microsoft.EntityFrameworkCore;
using OverLut.Models.BusinessObjects;
using OverLut.Models.DTOs;
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

    // Get channels by partial or full name (case-insensitive)
    public async Task<List<Channel>> GetChannelsByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return await _context.Channels.ToListAsync();
        }

        var pattern = name.Trim();
        return await _context.Channels
            .Where(c => c.ChannelName != null && EF.Functions.Like(c.ChannelName, $"%{pattern}%"))
            .ToListAsync();
    }
    // Get channel by id
    public async Task<Channel?> GetChannelByIdAsync(Guid channelId)
    {
        return await _context.Channels.FindAsync(channelId);
    }

    // Update channel
    public async Task UpdateChannelAsync(Channel channel)
    {
        _context.Channels.Update(channel);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ChannelDTO>?> GetAllChannelsByUserIDAsync(Guid userid)
    {
        return await _context.ChannelMembers
            .Include(cm => cm.Channel)
            .Where(cm => cm.UserId == userid)
            .Select(cm => new ChannelDTO
            {
                ChannelId = cm.ChannelId,
                ChannelType = cm.Channel.ChannelType,
                ChannelName = cm.Channel.ChannelName,
                DefaultPermissions = cm.Channel.DefaultPermissions,
                CreateAt = cm.Channel.CreateAt,
            }).ToListAsync();
    }
}
