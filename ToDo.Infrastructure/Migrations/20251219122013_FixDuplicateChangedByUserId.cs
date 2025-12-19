using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDuplicateChangedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangedByUserId",
                table: "ProjectStatusHistories");

            migrationBuilder.RenameColumn(
                name: "ChangedByuserId",
                table: "ProjectStatusHistories",
                newName: "ChangedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChangedByUserId",
                table: "ProjectStatusHistories",
                newName: "ChangedByuserId");

            migrationBuilder.AddColumn<string>(
                name: "ChangedByUserId",
                table: "ProjectStatusHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
