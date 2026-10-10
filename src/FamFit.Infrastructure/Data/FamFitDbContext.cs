using FamFit.Application.Interfaces;
using FamFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamFit.Infrastructure.Data;

/// <summary>
/// EF Core DbContext для FamFit.
/// Реализует IApplicationDbContext и содержит Fluent API конфигурацию.
/// </summary>
public sealed class FamFitDbContext : DbContext, IApplicationDbContext
{
    public FamFitDbContext(DbContextOptions<FamFitDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ActivityType> ActivityTypes => Set<ActivityType>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Применяем конфигурации из Assembly-атрибутов (если будут)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FamFitDbContext).Assembly);

        // --- User ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("email");

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("uq_users_email");

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(512)
                .HasColumnName("password_hash");

            entity.Property(e => e.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnName("role");

            entity.Property(e => e.StreakCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("streak_count");
        });

        // --- ActivityType ---
        modelBuilder.Entity<ActivityType>(entity =>
        {
            entity.ToTable("activity_types");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("uq_activity_types_name");

            entity.Property(e => e.UnitType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasColumnName("unit_type");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("is_active");
        });

        // --- ActivityLog ---
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("activity_logs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.UserId)
                .HasColumnType("uuid")
                .HasColumnName("user_id");

            entity.Property(e => e.ActivityTypeId)
                .HasColumnType("uuid")
                .HasColumnName("activity_type_id");

            entity.Property(e => e.Value)
                .IsRequired()
                .HasColumnType("numeric(18,2)")
                .HasColumnName("value");

            entity.Property(e => e.Timestamp)
                .IsRequired()
                .HasColumnType("timestamptz")
                .HasColumnName("timestamp");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_activity_logs_users");

            entity.HasOne(e => e.ActivityType)
                .WithMany()
                .HasForeignKey(e => e.ActivityTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_activity_logs_activity_types");

            entity.HasIndex(e => new { e.UserId, e.Timestamp })
                .HasDatabaseName("ix_activity_logs_user_timestamp");
        });

        // --- PushSubscription ---
        modelBuilder.Entity<PushSubscription>(entity =>
        {
            entity.ToTable("push_subscriptions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.UserId)
                .HasColumnType("uuid")
                .HasColumnName("user_id");

            entity.Property(e => e.Endpoint)
                .IsRequired()
                .HasMaxLength(1024)
                .HasColumnName("endpoint");

            entity.Property(e => e.P256dh)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("p256dh");

            entity.Property(e => e.Auth)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("auth");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamptz")
                .HasColumnName("created_at");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_push_subscriptions_users");

            entity.HasIndex(e => new { e.UserId, e.Endpoint })
                .IsUnique()
                .HasDatabaseName("uq_push_subscriptions_user_endpoint");
        });
    }
}
