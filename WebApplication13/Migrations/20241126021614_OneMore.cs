using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication13.Migrations
{
    /// <inheritdoc />
    public partial class OneMore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LogEntry",
                table: "LogEntry");

            migrationBuilder.RenameTable(
                name: "LogEntry",
                newName: "LogEntries");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogEntries",
                table: "LogEntries",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LogEntries",
                table: "LogEntries");

            migrationBuilder.RenameTable(
                name: "LogEntries",
                newName: "LogEntry");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogEntry",
                table: "LogEntry",
                column: "Id");
        }
    }
}
