using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignaliteWebAPI.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentPublicIdFileSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "FileSize",
                table: "Attachments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "PublicId",
                table: "Attachments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Attachments");
        }
    }
}
