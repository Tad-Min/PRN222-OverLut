using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OverLut.Models.BusinessObjects;

public partial class OverLutContext : DbContext
{
    public OverLutContext()
    {
    }

    public OverLutContext(DbContextOptions<OverLutContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<ChannelMember> ChannelMembers { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<ReadReceipt> ReadReceipts { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VerifyEmail> VerifyEmails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=OverLut;User Id=sa;Password=12345;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId).HasName("PK_AttachmentID");

            entity.HasIndex(e => new { e.UserId, e.ChannelId, e.MessageId }, "IX_Attachments_ChannelID_UserID_MessageID");

            entity.Property(e => e.AttachmentId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("AttachmentID");
            entity.Property(e => e.ChannelId).HasColumnName("ChannelID");
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.FileBlobId).HasColumnName("FileBlobID");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.MessageId).HasColumnName("MessageID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Message).WithMany(p => p.Attachments)
                .HasForeignKey(d => new { d.ChannelId, d.UserId, d.MessageId })
                .HasConstraintName("FK_Attachments_Messages");
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.Property(e => e.ChannelId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("ChannelID");
            entity.Property(e => e.ChannelName).HasMaxLength(255);
            entity.Property(e => e.ChannelType).HasDefaultValue(0);
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DefaultPermissions).HasDefaultValue(3);
        });

        modelBuilder.Entity<ChannelMember>(entity =>
        {
            entity.HasKey(e => new { e.ChannelId, e.UserId });

            entity.HasIndex(e => e.UserId, "IX_ChannelMembers_UserID");

            entity.Property(e => e.ChannelId).HasColumnName("ChannelID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.MemberRole).HasDefaultValue(0);
            entity.Property(e => e.Nickname).HasMaxLength(100);
            entity.Property(e => e.Permissions).HasDefaultValue(3);

            entity.HasOne(d => d.Channel).WithMany(p => p.ChannelMembers)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChannelMembers_Channels");

            entity.HasOne(d => d.User).WithMany(p => p.ChannelMembers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChannelMembers_Users");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => new { e.ChannelId, e.UserId, e.MessageId });

            entity.HasIndex(e => new { e.ChannelId, e.CreateAt }, "IX_Messages_ChannelID_CreateAt").IsDescending(false, true);

            entity.Property(e => e.ChannelId).HasColumnName("ChannelID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.MessageId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("MessageID");
            entity.Property(e => e.CreateAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.MessageType).HasDefaultValue(0);

            entity.HasOne(d => d.ChannelMember).WithMany(p => p.Messages)
                .HasForeignKey(d => new { d.ChannelId, d.UserId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_ChannelMembers");
        });

        modelBuilder.Entity<ReadReceipt>(entity =>
        {
            entity.HasKey(e => new { e.ChannelId, e.UserId });

            entity.HasIndex(e => new { e.UserId, e.ChannelId }, "IX_ReadReceipts_UserID_ChannelID");

            entity.Property(e => e.ChannelId).HasColumnName("ChannelID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.LastReadMessageId).HasColumnName("LastReadMessageID");
            entity.Property(e => e.LastReadTime).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Message).WithMany(p => p.ReadReceipts)
                .HasForeignKey(d => new { d.ChannelId, d.UserId, d.LastReadMessageId })
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ReadReceipts_Messages");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B616099CA1C96").IsUnique();

            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053422C4C3DB").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__Users__C9F2845683ABE19B").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("UserID");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasDefaultValue("User");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UserName)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<VerifyEmail>(entity =>
        {
            entity.HasKey(e => e.Key);

            entity.ToTable("VerifyEmail");

            entity.HasIndex(e => e.Email, "UQ__VerifyEm__A9D1053486E3D644").IsUnique();

            entity.Property(e => e.Key).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
