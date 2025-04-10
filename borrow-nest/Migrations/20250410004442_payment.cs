using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace borrownest.Migrations
{
    /// <inheritdoc />
    public partial class payment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Listings_ListingID",
                table: "Payments");

            migrationBuilder.AlterColumn<long>(
                name: "ListingID",
                table: "Payments",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SenderId",
                table: "Payments",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_SenderId",
                table: "Payments",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Listings_ListingID",
                table: "Payments",
                column: "ListingID",
                principalTable: "Listings",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_SenderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Listings_ListingID",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BookingId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SenderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SenderId",
                table: "Payments");

            migrationBuilder.AlterColumn<long>(
                name: "ListingID",
                table: "Payments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Listings_ListingID",
                table: "Payments",
                column: "ListingID",
                principalTable: "Listings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
