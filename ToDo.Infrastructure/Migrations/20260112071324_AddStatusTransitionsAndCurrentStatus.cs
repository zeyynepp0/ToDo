using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusTransitionsAndCurrentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChangedBy",
                table: "ProjectStatusHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentProjectStatusId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CurrentProjectStatusId",
                table: "Projects",
                column: "CurrentProjectStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ProjectStatuses_CurrentProjectStatusId",
                table: "Projects",
                column: "CurrentProjectStatusId",
                principalTable: "ProjectStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ProjectStatuses_CurrentProjectStatusId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CurrentProjectStatusId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ChangedBy",
                table: "ProjectStatusHistories");

            migrationBuilder.DropColumn(
                name: "CurrentProjectStatusId",
                table: "Projects");
        }
    }
}
