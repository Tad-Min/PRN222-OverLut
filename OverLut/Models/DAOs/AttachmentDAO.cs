using OverLut.Models.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OverLut.Models.DTOs;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace OverLut.Models.DAOs;

public class AttachmentDAO
{
    private readonly OverLutStorageContext _storageContext;
    private readonly OverLutContext _chatContext;

    public AttachmentDAO(OverLutStorageContext storageContext, OverLutContext chatContext)
    {
        _storageContext = storageContext;
        _chatContext = chatContext;
    }

    public async Task<AttachmentDTO?> GetAttachmentByIdAsync(Guid attachmentId)
    {
        var att = await _chatContext.Attachments.FindAsync(attachmentId);
        if (att == null) return null;

        return new AttachmentDTO
        {
            AttachmentId = att.AttachmentId,
            MessageId = att.MessageId,
            ChannelId = att.ChannelId,
            UserId = att.UserId,
            FileName = att.FileName,
            ContentType = att.ContentType,
            Width = att.Width,
            Height = att.Height,
            Duration = att.Duration,
            FileSize = att.FileSize,
            FileBlobId = att.FileBlobId
        };
    }

    // Start a chunked upload. Creates a FileBlob record and returns its id.
    public async Task<Guid?> StartUploadAsync(string fileName, string contentType)
    {
        var blob = new FileBlob
        {
            FileBlobId = Guid.NewGuid(),
            IsComplete = false,
            CreatedAt = DateTime.UtcNow
        };

        await _storageContext.FileBlobs.AddAsync(blob);
        await _storageContext.SaveChangesAsync();

        return blob.FileBlobId;
    }

    // Save a single chunk for an existing FileBlob
    public async Task<bool> SaveChunkAsync(Guid fileBlobId, int sequenceNumber, byte[] data)
    {
        // ensure blob exists
        var blob = await _storageContext.FileBlobs.FindAsync(fileBlobId);
        if (blob == null) return false;

        var chunk = new FileChunk
        {
            ChunkId = Guid.NewGuid(),
            FileBlobId = fileBlobId,
            SequenceNumber = sequenceNumber,
            Data = data
        };

        await _storageContext.FileChunks.AddAsync(chunk);
        await _storageContext.SaveChangesAsync();

        return true;
    }

    // Mark blob as complete
    public async Task<bool> CompleteBlobAsync(Guid fileBlobId)
    {
        var blob = await _storageContext.FileBlobs.FindAsync(fileBlobId);
        if (blob == null) return false;
        blob.IsComplete = true;
        _storageContext.FileBlobs.Update(blob);
        await _storageContext.SaveChangesAsync();
        return true;
    }

    // Create an Attachment record that references an existing FileBlob
    public async Task<AttachmentDTO?> CreateAttachmentFromBlobAsync(Guid fileBlobId, Guid channelId, Guid messageId, Guid userId, string fileName, string contentType, long fileSize)
    {
        var blob = await _storageContext.FileBlobs.FindAsync(fileBlobId);
        if (blob == null) return null;

        var attachment = new Attachment
        {
            AttachmentId = Guid.NewGuid(),
            MessageId = messageId,
            ChannelId = channelId,
            UserId = userId,
            FileName = fileName,
            ContentType = string.IsNullOrEmpty(contentType) ? "application/octet-stream" : contentType,
            Width = null,
            Height = null,
            Duration = null,
            FileSize = fileSize,
            FileBlobId = fileBlobId
        };

        await _chatContext.Attachments.AddAsync(attachment);
        await _chatContext.SaveChangesAsync();

        return new AttachmentDTO
        {
            AttachmentId = attachment.AttachmentId,
            MessageId = attachment.MessageId,
            ChannelId = attachment.ChannelId,
            UserId = attachment.UserId,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            Width = attachment.Width,
            Height = attachment.Height,
            Duration = attachment.Duration,
            FileSize = attachment.FileSize,
            FileBlobId = attachment.FileBlobId
        };
    }

    // Save a single uploaded file as a FileBlob + FileChunk and create Attachment record
    public async Task<AttachmentDTO?> SaveAttachmentAsync(IFormFile file, Guid channelId, Guid messageId, Guid userId)
    {
        if (file == null) return null;

        // Create FileBlob
        var blob = new FileBlob
        {
            FileBlobId = Guid.NewGuid(),
            IsComplete = true,
            CreatedAt = DateTime.UtcNow
        };

        await _storageContext.FileBlobs.AddAsync(blob);

        // Read file into memory (single chunk)
        byte[] data;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            data = ms.ToArray();
        }

        var chunk = new FileChunk
        {
            ChunkId = Guid.NewGuid(),
            FileBlobId = blob.FileBlobId,
            SequenceNumber = 1,
            Data = data
        };

        await _storageContext.FileChunks.AddAsync(chunk);

        var attachment = new Attachment
        {
            AttachmentId = Guid.NewGuid(),
            MessageId = messageId,
            ChannelId = channelId,
            UserId = userId,
            FileName = file.FileName,
            ContentType = string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType,
            Width = null,
            Height = null,
            Duration = null,
            FileSize = file.Length,
            FileBlobId = blob.FileBlobId
        };

        await _chatContext.Attachments.AddAsync(attachment);

        // Save storage first then chat DB so blob exists before attachment references it
        await _storageContext.SaveChangesAsync();
        await _chatContext.SaveChangesAsync();

        return new AttachmentDTO
        {
            AttachmentId = attachment.AttachmentId,
            MessageId = attachment.MessageId,
            ChannelId = attachment.ChannelId,
            UserId = attachment.UserId,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            Width = attachment.Width,
            Height = attachment.Height,
            Duration = attachment.Duration,
            FileSize = attachment.FileSize,
            FileBlobId = attachment.FileBlobId
        };
    }

    // Read all chunks for a blob and concatenate into single byte[]
    public async Task<byte[]?> ReadBlobAsync(Guid fileBlobId)
    {
        var chunks = await _storageContext.FileChunks
            .Where(c => c.FileBlobId == fileBlobId)
            .OrderBy(c => c.SequenceNumber)
            .ToListAsync();

        if (chunks == null || chunks.Count == 0) return null;

        using var ms = new MemoryStream();
        foreach (var ch in chunks)
        {
            await ms.WriteAsync(ch.Data, 0, ch.Data.Length);
        }

        return ms.ToArray();
    }

}
