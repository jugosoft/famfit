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
    public DbSet<ActivityUnit> ActivityUnits => Set<ActivityUnit>();
    public DbSet<ActivityAttributeDefinition> ActivityAttributeDefinitions => Set<ActivityAttributeDefinition>();
    public DbSet<ActivityAttributeValue> AttributeValues => Set<ActivityAttributeValue>();
    public DbSet<ActivityTypeAttributeValue> ActivityTypeAttributeValues => Set<ActivityTypeAttributeValue>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutEntry> WorkoutEntries => Set<WorkoutEntry>();
    public DbSet<WorkoutEntryAttributeValue> WorkoutEntryAttributeValues => Set<WorkoutEntryAttributeValue>();
    public DbSet<WorkoutTemplate> WorkoutTemplates => Set<WorkoutTemplate>();
    public DbSet<WorkoutTemplateItem> WorkoutTemplateItems => Set<WorkoutTemplateItem>();
    public DbSet<WorkoutTemplateItemAttributeValue> WorkoutTemplateItemAttributeValues => Set<WorkoutTemplateItemAttributeValue>();
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

            entity.Property(e => e.ActivityUnitId)
                .HasColumnType("uuid")
                .HasColumnName("activity_unit_id");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            entity.HasOne(e => e.ActivityUnit)
                .WithMany(e => e.ActivityTypes)
                .HasForeignKey(e => e.ActivityUnitId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_activity_types_activity_units");
        });

        // --- ActivityUnit ---
        modelBuilder.Entity<ActivityUnit>(entity =>
        {
            entity.ToTable("activity_units");

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
                .HasDatabaseName("uq_activity_units_name");

            entity.Property(e => e.Symbol)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("symbol");

            entity.Property(e => e.DataType)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("data_type");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            entity.Property(e => e.DisplayOrder)
                .IsRequired()
                .HasColumnName("display_order");
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
                .WithMany(e => e.ActivityLogs)
                .HasForeignKey(e => e.ActivityTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_activity_logs_activity_types");

            entity.HasIndex(e => new { e.UserId, e.Timestamp })
                .HasDatabaseName("ix_activity_logs_user_timestamp");
        });

        // --- ActivityAttributeDefinition ---
        modelBuilder.Entity<ActivityAttributeDefinition>(entity =>
        {
            entity.ToTable("activity_attribute_definitions");

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
                .HasDatabaseName("uq_activity_attribute_definitions_name");

            entity.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("display_name");

            entity.Property(e => e.DataType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnName("data_type");

            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .HasColumnName("unit");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            entity.Property(e => e.DisplayOrder)
                .IsRequired()
                .HasColumnName("display_order");
        });

        // --- ActivityAttributeValue ---
        modelBuilder.Entity<ActivityAttributeValue>(entity =>
        {
            entity.ToTable("activity_attribute_values");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.DefinitionId)
                .HasColumnType("uuid")
                .HasColumnName("definition_id");

            entity.Property(e => e.IntValue)
                .HasColumnName("int_value");

            entity.Property(e => e.DecimalValue)
                .HasColumnType("numeric(18,4)")
                .HasColumnName("decimal_value");

            entity.Property(e => e.BoolValue)
                .HasColumnName("bool_value");

            entity.Property(e => e.StringValue)
                .HasMaxLength(500)
                .HasColumnName("string_value");

            entity.HasOne(e => e.Definition)
                .WithMany(e => e.AttributeValues)
                .HasForeignKey(e => e.DefinitionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_attribute_values_definitions");
        });

        // --- ActivityTypeAttributeValue ---
        modelBuilder.Entity<ActivityTypeAttributeValue>(entity =>
        {
            entity.ToTable("activity_type_attribute_values");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.ActivityTypeId)
                .HasColumnType("uuid")
                .HasColumnName("activity_type_id");

            entity.Property(e => e.AttributeValueId)
                .HasColumnType("uuid")
                .HasColumnName("attribute_value_id");

            entity.HasOne(e => e.ActivityType)
                .WithMany(e => e.ActivityTypeAttributeValues)
                .HasForeignKey(e => e.ActivityTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_activity_type_attribute_values_activity_types");

            entity.HasOne(e => e.AttributeValue)
                .WithMany(e => e.ActivityTypeAttributeValues)
                .HasForeignKey(e => e.AttributeValueId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_activity_type_attribute_values_values");

            entity.HasIndex(e => new { e.ActivityTypeId, e.AttributeValueId })
                .IsUnique()
                .HasDatabaseName("uq_activity_type_attribute_values_combo");
        });

        // --- WorkoutEntryAttributeValue ---
        modelBuilder.Entity<WorkoutEntryAttributeValue>(entity =>
        {
            entity.ToTable("workout_entry_attribute_values");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.WorkoutEntryId)
                .HasColumnType("uuid")
                .HasColumnName("workout_entry_id");

            entity.Property(e => e.AttributeValueId)
                .HasColumnType("uuid")
                .HasColumnName("attribute_value_id");

            entity.HasOne(e => e.WorkoutEntry)
                .WithMany(e => e.WorkoutEntryAttributeValues)
                .HasForeignKey(e => e.WorkoutEntryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_workout_entry_attribute_values_entries");

            entity.HasOne(e => e.AttributeValue)
                .WithMany(e => e.WorkoutEntryAttributeValues)
                .HasForeignKey(e => e.AttributeValueId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_workout_entry_attribute_values_values");

            entity.HasIndex(e => new { e.WorkoutEntryId, e.AttributeValueId })
                .IsUnique()
                .HasDatabaseName("uq_workout_entry_attribute_values_combo");
        });

        // --- WorkoutTemplateItemAttributeValue ---
        modelBuilder.Entity<WorkoutTemplateItemAttributeValue>(entity =>
        {
            entity.ToTable("workout_template_item_attribute_values");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.WorkoutTemplateItemId)
                .HasColumnType("uuid")
                .HasColumnName("workout_template_item_id");

            entity.Property(e => e.AttributeValueId)
                .HasColumnType("uuid")
                .HasColumnName("attribute_value_id");

            entity.HasOne(e => e.WorkoutTemplateItem)
                .WithMany(e => e.WorkoutTemplateItemAttributeValues)
                .HasForeignKey(e => e.WorkoutTemplateItemId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_workout_template_item_attribute_values_items");

            entity.HasOne(e => e.AttributeValue)
                .WithMany(e => e.WorkoutTemplateItemAttributeValues)
                .HasForeignKey(e => e.AttributeValueId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_workout_template_item_attribute_values_values");

            entity.HasIndex(e => new { e.WorkoutTemplateItemId, e.AttributeValueId })
                .IsUnique()
                .HasDatabaseName("uq_workout_template_item_attribute_values_combo");
        });

        // --- Workout ---
        modelBuilder.Entity<Workout>(entity =>
        {
            entity.ToTable("workouts");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.UserId)
                .HasColumnType("uuid")
                .HasColumnName("user_id");

            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");

            entity.Property(e => e.StartTime)
                .IsRequired()
                .HasColumnType("timestamptz")
                .HasColumnName("start_time");

            entity.Property(e => e.EndTime)
                .HasColumnType("timestamptz")
                .HasColumnName("end_time");

            entity.Property(e => e.IsCompleted)
                .IsRequired()
                .HasColumnName("is_completed");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_workouts_users");

            entity.HasIndex(e => new { e.UserId, e.StartTime })
                .HasDatabaseName("ix_workouts_user_start_time");
        });

        // --- WorkoutEntry ---
        modelBuilder.Entity<WorkoutEntry>(entity =>
        {
            entity.ToTable("workout_entries");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.WorkoutId)
                .HasColumnType("uuid")
                .HasColumnName("workout_id");

            entity.Property(e => e.ActivityTypeId)
                .HasColumnType("uuid")
                .HasColumnName("activity_type_id");

            entity.HasOne(e => e.Workout)
                .WithMany(e => e.Entries)
                .HasForeignKey(e => e.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_workout_entries_workouts");

            entity.HasOne(e => e.ActivityType)
                .WithMany(e => e.WorkoutEntries)
                .HasForeignKey(e => e.ActivityTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_workout_entries_activity_types");
        });

        // --- WorkoutTemplate ---
        modelBuilder.Entity<WorkoutTemplate>(entity =>
        {
            entity.ToTable("workout_templates");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.UserId)
                .HasColumnType("uuid")
                .HasColumnName("user_id");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("name");

            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_workout_templates_users");
        });

        // --- WorkoutTemplateItem ---
        modelBuilder.Entity<WorkoutTemplateItem>(entity =>
        {
            entity.ToTable("workout_template_items");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.TemplateId)
                .HasColumnType("uuid")
                .HasColumnName("template_id");

            entity.Property(e => e.ActivityTypeId)
                .HasColumnType("uuid")
                .HasColumnName("activity_type_id");

            entity.Property(e => e.DisplayOrder)
                .IsRequired()
                .HasColumnName("display_order");

            entity.HasOne(e => e.Template)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_workout_template_items_templates");

            entity.HasOne(e => e.ActivityType)
                .WithMany(e => e.WorkoutTemplateItems)
                .HasForeignKey(e => e.ActivityTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_workout_template_items_activity_types");
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
