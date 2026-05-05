using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EventManager.DAL.Models;

public partial class EventManagerDbContext : DbContext
{
    public EventManagerDbContext(DbContextOptions<EventManagerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventPerformer> EventPerformers { get; set; }

    public virtual DbSet<EventType> EventTypes { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Performer> Performers { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Event");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.Events)
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Event_User");

            entity.HasOne(d => d.EventType).WithMany(p => p.Events)
                .HasForeignKey(d => d.EventTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Event_EventType");

            entity.HasOne(d => d.Image).WithMany(p => p.Events)
                .HasForeignKey(d => d.ImageId)
                .HasConstraintName("FK_Event_Image");
        });

        modelBuilder.Entity<EventPerformer>(entity =>
        {
            entity.ToTable("EventPerformer");

            entity.HasIndex(e => new { e.EventId, e.PerformerId }, "UQ_EventPerformer_Event_Performer").IsUnique();

            entity.HasOne(d => d.Event).WithMany(p => p.EventPerformers)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventPerformer_Event");

            entity.HasOne(d => d.Performer).WithMany(p => p.EventPerformers)
                .HasForeignKey(d => d.PerformerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventPerformer_Performer");
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.ToTable("EventType");

            entity.HasIndex(e => e.Name, "UQ_EventType_Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.ToTable("Image");

            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.ToTable("Log");

            entity.Property(e => e.Message).HasMaxLength(1024);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Performer>(entity =>
        {
            entity.ToTable("Performer");

            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.ToTable("Registration");

            entity.HasIndex(e => new { e.UserId, e.EventId }, "UQ_Registration_User_Event").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.RegisteredAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Event).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Registration_Event");

            entity.HasOne(d => d.User).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Registration_User");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.HasIndex(e => e.Name, "UQ_Role_Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ_User_Email").IsUnique();

            entity.HasIndex(e => e.Username, "UQ_User_Username").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(256);
            entity.Property(e => e.LastName).HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(256);
            entity.Property(e => e.PwdHash).HasMaxLength(256);
            entity.Property(e => e.PwdSalt).HasMaxLength(256);
            entity.Property(e => e.RoleId).HasDefaultValue(2);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
