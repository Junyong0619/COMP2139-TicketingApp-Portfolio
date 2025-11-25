using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBC_Ticketing.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseAttendee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttendeeId",
                table: "Purchases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Purchases",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_AttendeeId",
                table: "Purchases",
                column: "AttendeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_AspNetUsers_AttendeeId",
                table: "Purchases",
                column: "AttendeeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_AspNetUsers_AttendeeId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_AttendeeId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "AttendeeId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Purchases");
        }
    }
}
