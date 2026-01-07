using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OverLut.Models.BusinessObjects;

public partial class OverLutStorageContext : DbContext
{
    public OverLutStorageContext()
    {
    }

    public OverLutStorageContext(DbContextOptions<OverLutStorageContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FileBlob> FileBlobs { get; set; }

    public virtual DbSet<FileChunk> FileChunks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=OverLut_Storage;User Id=sa;Password=12345;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileBlob>(entity =>
        {
            entity.HasKey(e => e.FileBlobId).HasName("PK__FileBlob__8E6FD50C142EFBB1");

            entity.Property(e => e.FileBlobId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("FileBlobID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsComplete).HasDefaultValue(false);
        });

        modelBuilder.Entity<FileChunk>(entity =>
        {
            entity.HasKey(e => e.ChunkId).HasName("PK__FileChun__FBFF9D20DF01B329");

            entity.HasIndex(e => new { e.FileBlobId, e.SequenceNumber }, "IX_Chunks_Blob_Order");

            entity.Property(e => e.ChunkId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("ChunkID");
            entity.Property(e => e.FileBlobId).HasColumnName("FileBlobID");

            entity.HasOne(d => d.FileBlob).WithMany(p => p.FileChunks)
                .HasForeignKey(d => d.FileBlobId)
                .HasConstraintName("FK_Chunks_Blobs");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
