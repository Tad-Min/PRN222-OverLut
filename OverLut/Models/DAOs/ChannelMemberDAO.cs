using Microsoft.EntityFrameworkCore;
using OverLut.Models.BusinessObjects;
using OverLut.Models.DTOs;
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

    // get user from channel id
    public async Task<IEnumerable<ChannelMemberDTO>> GetUserIdsByChannelIdAsync(Guid channelId)
    {
        return await _context.ChannelMembers.Select(cm => new ChannelMemberDTO
        {
            ChannelId = channelId,
            UserId = cm.UserId,
            Nickname = cm.Nickname,
            MemberRole = cm.MemberRole,
        }).Where(cm => cm.ChannelId == channelId).ToListAsync();
    }

    // get channel from channel name
    public async Task<List<ChannelMember>> GetMembersFromChannelIdAsync(Guid channelId)
    {
        return await Task.FromResult(_context.ChannelMembers.Where(cm => cm.ChannelId == channelId).ToList());
    }

    // get single member
    public async Task<ChannelMember?> GetMemberAsync(Guid channelId, Guid userId)
    {
        return await _context.ChannelMembers.FindAsync(channelId, userId);
    }

    // remove single member
    public async Task RemoveMemberAsync(Guid channelId, Guid userId)
    {
        var member = await _context.ChannelMembers.FindAsync(channelId, userId);
        if (member == null) return;
        _context.ChannelMembers.Remove(member);
        await _context.SaveChangesAsync();
    }

    // Remove all members for a channel (used when deleting a channel)
    public async Task RemoveMembersByChannelIdAsync(Guid channelId)
    {
        var members = _context.ChannelMembers.Where(cm => cm.ChannelId == channelId).ToList();
        if (members.Count == 0) return;
        _context.ChannelMembers.RemoveRange(members);
        await _context.SaveChangesAsync();
    }

    // update nickname for a member
    public async Task UpdateMemberNicknameAsync(Guid channelId, Guid userId, string? nickname)
    {
        var member = await _context.ChannelMembers.FindAsync(channelId, userId);
        if (member == null) return;
        member.Nickname = nickname;
        await _context.SaveChangesAsync();
    }
}
