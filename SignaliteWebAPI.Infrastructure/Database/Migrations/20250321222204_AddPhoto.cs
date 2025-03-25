using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignaliteWebAPI.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "BackgroundPhotoId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfilePhotoId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    PublicId = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_BackgroundPhotoId",
                table: "Users",
                column: "BackgroundPhotoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProfilePhotoId",
                table: "Users",
                column: "ProfilePhotoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_UserId",
                table: "Photos",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Photos_BackgroundPhotoId",
                table: "Users",
                column: "BackgroundPhotoId",
                principalTable: "Photos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Photos_ProfilePhotoId",
                table: "Users",
                column: "ProfilePhotoId",
                principalTable: "Photos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Photos_BackgroundPhotoId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Photos_ProfilePhotoId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Users_BackgroundPhotoId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ProfilePhotoId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BackgroundPhotoId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundUrl",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }
    }
}
