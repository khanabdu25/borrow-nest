using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace borrownest.Migrations
{
    /// <inheritdoc />
    public partial class secondmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientCheckIns_AspNetUsers_BNUserId",
                table: "ClientCheckIns");

            migrationBuilder.DropIndex(
                name: "IX_ClientCheckIns_BNUserId",
                table: "ClientCheckIns");

            migrationBuilder.DropColumn(
                name: "BNUserId",
                table: "ClientCheckIns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BNUserId",
                table: "ClientCheckIns",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientCheckIns_BNUserId",
                table: "ClientCheckIns",
                column: "BNUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientCheckIns_AspNetUsers_BNUserId",
                table: "ClientCheckIns",
                column: "BNUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
