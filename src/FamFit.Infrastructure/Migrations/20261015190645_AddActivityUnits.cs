using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamFit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "unit_type",
                table: "activity_types");

            migrationBuilder.AddColumn<Guid>(
                name: "activity_unit_id",
                table: "activity_types",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "activity_units",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_units", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_types_activity_unit_id",
                table: "activity_types",
                column: "activity_unit_id");

            migrationBuilder.CreateIndex(
                name: "uq_activity_units_name",
                table: "activity_units",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_activity_types_activity_units",
                table: "activity_types",
                column: "activity_unit_id",
                principalTable: "activity_units",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_activity_types_activity_units",
                table: "activity_types");

            migrationBuilder.DropTable(
                name: "activity_units");

            migrationBuilder.DropIndex(
                name: "ix_activity_types_activity_unit_id",
                table: "activity_types");

            migrationBuilder.DropColumn(
                name: "activity_unit_id",
                table: "activity_types");

            migrationBuilder.AddColumn<string>(
                name: "unit_type",
                table: "activity_types",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
