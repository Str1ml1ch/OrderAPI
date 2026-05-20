using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderAPI.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueActiveSeatHoldIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_SeatHolds_ActiveSeatId",
                table: "SeatHolds",
                column: "SeatId",
                unique: true,
                filter: "[SeatHoldStatus] = 'Held'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_SeatHolds_ActiveSeatId",
                table: "SeatHolds");
        }
    }
}
