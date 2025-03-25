using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignaliteWebAPI.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class PhotoIdInGroupEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Groups");

            migrationBuilder.AddColumn<int>(
                name: "PhotoId",
                table: "Groups",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_PhotoId",
                table: "Groups",
                column: "PhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Photos_PhotoId",
                table: "Groups",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Photos_PhotoId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_PhotoId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "PhotoId",
                table: "Groups");

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Groups",
                type: "TEXT",
                nullable: true);
        }
    }
}
