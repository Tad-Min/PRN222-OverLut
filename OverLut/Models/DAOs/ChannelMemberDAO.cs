using OverLut.Models.BusinessObjects;
using System;
using System.Collections.Generic;

namespace OverLut.Models.DAOs;

public partial class ChannelMemberDAO
{
    private readonly OverLutContext _context;
    public ChannelMemberDAO(OverLutContext context)
    {
        _context = context;
    }

    // add member to channel
    public async Task AddMemberToChannelAsync(ChannelMember channelMember)
    {
        await _context.ChannelMembers.AddAsync(channelMember);
        await _context.SaveChangesAsync();
    }
    // get channel from user id
    public async Task<List<ChannelMember>> GetChannelsFromUserIdAsync(Guid userId)
    {
        return await Task.FromResult(_context.ChannelMembers.Where(cm => cm.UserId == userId).ToList());
    }

    // get channel from channel name
    public async Task<List<ChannelMember>> GetMembersFromChannelIdAsync(Guid channelId)
    {
        return await Task.FromResult(_context.ChannelMembers.Where(cm => cm.ChannelId == channelId).ToList());
    }

}
