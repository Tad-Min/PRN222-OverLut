using Microsoft.EntityFrameworkCore;
using OverLut.Models.BusinessObjects;
using System;
using System.Collections.Generic;

namespace OverLut.Models.DAOs;

public class MessageDAO
{
    private readonly OverLutContext _context;
    public MessageDAO(OverLutContext context)
    {
        _context = context;
    }
    public async Task<List<Message>> GetMessagesAsync(Guid channelId, int skip, int take)
    {
        return await _context.Messages
            .Include(m => m.Attachments)
            .Include(m => m.ChannelMember)
                .ThenInclude(cm => cm.User)
            .Where(m => m.ChannelId == channelId)
            .OrderByDescending(m => m.CreateAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task CreateMessageAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateMessageContentAsync(Guid channelId, Guid userId, Guid messageId, string newContent)
    {
        var msg = await _context.Messages.FindAsync(channelId, userId, messageId);
        if (msg != null)
        {
            msg.Content = newContent;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteMessageAsync(Guid channelId, Guid userId, Guid messageId)
    {
        var msg = await _context.Messages.FindAsync(channelId, userId, messageId);
        if (msg != null)
        {
            _context.Messages.Remove(msg);
            await _context.SaveChangesAsync();
        }
    }
}
