using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QueueApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "queue",
                table: "tickets",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "queue",
                table: "tickets");
        }
    }
}
