using Microsoft.EntityFrameworkCore;
using OverLut.Models.BusinessObjects;
using OverLut.Models.DTOs;
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
    public async Task<IEnumerable<MessageDTO>> GetMessagesByChannelIDAsync(Guid channelId, int page = 0, int page_size = 20)
    {
        int skipMessages = (page - 1) * page_size;

        var allMessages = await _context.Messages
            .Where(m => m.ChannelId == channelId)
            .OrderByDescending(m => m.CreateAt)
            .Skip(skipMessages < 0 ? 0 : skipMessages)
            .Take(page_size)
            .AsNoTracking()
            .ToListAsync();

        return allMessages.Select(m => new MessageDTO
        {
            MessageId = m.MessageId,
            ChannelId = m.ChannelId,
            UserId = m.UserId,
            MessageType = m.MessageType,
            Content = m.Content,
            CreateAt = m.CreateAt
        });

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
