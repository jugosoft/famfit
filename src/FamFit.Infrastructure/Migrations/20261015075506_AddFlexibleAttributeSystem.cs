using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlexibleAttributeSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_type_attributes");

            migrationBuilder.DropColumn(
                name: "reps",
                table: "workout_template_items");

            migrationBuilder.DropColumn(
                name: "sets",
                table: "workout_template_items");

            migrationBuilder.DropColumn(
                name: "weight",
                table: "workout_template_items");

            migrationBuilder.DropColumn(
                name: "reps",
                table: "workout_entries");

            migrationBuilder.DropColumn(
                name: "sets",
                table: "workout_entries");

            migrationBuilder.DropColumn(
                name: "weight",
                table: "workout_entries");

            migrationBuilder.CreateTable(
                name: "activity_attribute_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_attribute_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "activity_attribute_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    int_value = table.Column<int>(type: "integer", nullable: true),
                    decimal_value = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    bool_value = table.Column<bool>(type: "boolean", nullable: false),
                    string_value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_attribute_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_attribute_values_definitions",
                        column: x => x.definition_id,
                        principalTable: "activity_attribute_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "activity_type_attribute_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    activity_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_value_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_type_attribute_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_activity_type_attribute_values_activity_types",
                        column: x => x.activity_type_id,
                        principalTable: "activity_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_activity_type_attribute_values_values",
                        column: x => x.attribute_value_id,
                        principalTable: "activity_attribute_values",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workout_entry_attribute_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    workout_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_value_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workout_entry_attribute_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_workout_entry_attribute_values_entries",
                        column: x => x.workout_entry_id,
                        principalTable: "workout_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_workout_entry_attribute_values_values",
                        column: x => x.attribute_value_id,
                        principalTable: "activity_attribute_values",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workout_template_item_attribute_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    workout_template_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_value_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workout_template_item_attribute_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_workout_template_item_attribute_values_items",
                        column: x => x.workout_template_item_id,
                        principalTable: "workout_template_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_workout_template_item_attribute_values_values",
                        column: x => x.attribute_value_id,
                        principalTable: "activity_attribute_values",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "uq_activity_attribute_definitions_name",
                table: "activity_attribute_definitions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_activity_attribute_values_definition_id",
                table: "activity_attribute_values",
                column: "definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_type_attribute_values_attribute_value_id",
                table: "activity_type_attribute_values",
                column: "attribute_value_id");

            migrationBuilder.CreateIndex(
                name: "uq_activity_type_attribute_values_combo",
                table: "activity_type_attribute_values",
                columns: new[] { "activity_type_id", "attribute_value_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workout_entry_attribute_values_attribute_value_id",
                table: "workout_entry_attribute_values",
                column: "attribute_value_id");

            migrationBuilder.CreateIndex(
                name: "uq_workout_entry_attribute_values_combo",
                table: "workout_entry_attribute_values",
                columns: new[] { "workout_entry_id", "attribute_value_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workout_template_item_attribute_values_attribute_value_id",
                table: "workout_template_item_attribute_values",
                column: "attribute_value_id");

            migrationBuilder.CreateIndex(
                name: "uq_workout_template_item_attribute_values_combo",
                table: "workout_template_item_attribute_values",
                columns: new[] { "workout_template_item_id", "attribute_value_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_type_attribute_values");

            migrationBuilder.DropTable(
                name: "workout_entry_attribute_values");

            migrationBuilder.DropTable(
                name: "workout_template_item_attribute_values");

            migrationBuilder.DropTable(
                name: "activity_attribute_values");

            migrationBuilder.DropTable(
                name: "activity_attribute_definitions");

            migrationBuilder.AddColumn<int>(
                name: "reps",
                table: "workout_template_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sets",
                table: "workout_template_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "weight",
                table: "workout_template_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reps",
                table: "workout_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sets",
                table: "workout_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "weight",
                table: "workout_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "activity_type_attributes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    activity_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reps = table.Column<int>(type: "integer", nullable: true),
                    sets = table.Column<int>(type: "integer", nullable: true),
                    weight = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_type_attributes", x => x.id);
                    table.ForeignKey(
                        name: "fk_activity_type_attributes_activity_types",
                        column: x => x.activity_type_id,
                        principalTable: "activity_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "uq_activity_type_attributes_activity_type",
                table: "activity_type_attributes",
                column: "activity_type_id",
                unique: true);
        }
    }
}
