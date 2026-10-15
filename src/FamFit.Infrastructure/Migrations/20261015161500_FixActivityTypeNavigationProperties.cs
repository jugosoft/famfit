using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamFit.Infrastructure.Migrations
{
    /// <summary>
    /// Fixes ActivityType navigation properties in ActivityLog, WorkoutEntry, WorkoutTemplateItem.
    /// Removes shadow FK columns created by EF Core when navigation properties were added.
    /// </summary>
    public partial class FixActivityTypeNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop shadow FK columns and indexes if they exist (they were never applied in production)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Drop shadow FK constraints if they exist
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'fk_activity_logs_activity_types_activity_type_id1') THEN
                        ALTER TABLE activity_logs DROP CONSTRAINT fk_activity_logs_activity_types_activity_type_id1;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'fk_workout_entries_activity_types_activity_type_id1') THEN
                        ALTER TABLE workout_entries DROP CONSTRAINT fk_workout_entries_activity_types_activity_type_id1;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'fk_workout_template_items_activity_types_activity_type_id1') THEN
                        ALTER TABLE workout_template_items DROP CONSTRAINT fk_workout_template_items_activity_types_activity_type_id1;
                    END IF;

                    -- Drop shadow indexes if they exist
                    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_workout_template_items_activity_type_id1') THEN
                        DROP INDEX ix_workout_template_items_activity_type_id1;
                    END IF;
                    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_workout_entries_activity_type_id1') THEN
                        DROP INDEX ix_workout_entries_activity_type_id1;
                    END IF;
                    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'ix_activity_logs_activity_type_id1') THEN
                        DROP INDEX ix_activity_logs_activity_type_id1;
                    END IF;

                    -- Drop shadow columns if they exist
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'workout_template_items' AND column_name = 'activity_type_id1') THEN
                        ALTER TABLE workout_template_items DROP COLUMN activity_type_id1;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'workout_entries' AND column_name = 'activity_type_id1') THEN
                        ALTER TABLE workout_entries DROP COLUMN activity_type_id1;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'activity_logs' AND column_name = 'activity_type_id1') THEN
                        ALTER TABLE activity_logs DROP COLUMN activity_type_id1;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: we don't need to recreate shadow columns
        }
    }
}
