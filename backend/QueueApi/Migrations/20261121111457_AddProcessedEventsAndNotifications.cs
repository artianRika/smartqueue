using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QueueApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedEventsAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "queue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "processed_events",
                schema: "queue",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_events", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_EventId",
                schema: "queue",
                table: "notifications",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_QueueId_CreatedAt",
                schema: "queue",
                table: "notifications",
                columns: new[] { "QueueId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_TicketId",
                schema: "queue",
                table: "notifications",
                column: "TicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications",
                schema: "queue");

            migrationBuilder.DropTable(
                name: "processed_events",
                schema: "queue");
        }
    }
}
