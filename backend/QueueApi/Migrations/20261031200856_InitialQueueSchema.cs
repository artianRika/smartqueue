using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QueueApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialQueueSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "queue");

            migrationBuilder.CreateTable(
                name: "queues",
                schema: "queue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TicketPrefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queues", x => x.Id);
                    table.CheckConstraint("CK_queues_ticket_prefix", "\"TicketPrefix\" ~ '^[A-Z]{1,10}$'");
                });

            migrationBuilder.CreateTable(
                name: "counters",
                schema: "queue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_counters", x => x.Id);
                    table.UniqueConstraint("AK_counters_Id_QueueId", x => new { x.Id, x.QueueId });
                    table.ForeignKey(
                        name: "FK_counters_queues_QueueId",
                        column: x => x.QueueId,
                        principalSchema: "queue",
                        principalTable: "queues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "queue_daily_counters",
                schema: "queue",
                columns: table => new
                {
                    QueueId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_daily_counters", x => new { x.QueueId, x.TicketDate });
                    table.CheckConstraint("CK_queue_daily_counters_last_number", "\"LastNumber\" >= 0");
                    table.ForeignKey(
                        name: "FK_queue_daily_counters_queues_QueueId",
                        column: x => x.QueueId,
                        principalSchema: "queue",
                        principalTable: "queues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "counter_assignments",
                schema: "queue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_counter_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_counter_assignments_counters_CounterId",
                        column: x => x.CounterId,
                        principalSchema: "queue",
                        principalTable: "counters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                schema: "queue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QueueId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    TicketCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TicketDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ServiceStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.Id);
                    table.CheckConstraint("CK_tickets_number", "\"Number\" > 0");
                    table.ForeignKey(
                        name: "FK_tickets_counters_CounterId_QueueId",
                        columns: x => new { x.CounterId, x.QueueId },
                        principalSchema: "queue",
                        principalTable: "counters",
                        principalColumns: new[] { "Id", "QueueId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tickets_queues_QueueId",
                        column: x => x.QueueId,
                        principalSchema: "queue",
                        principalTable: "queues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_counter_assignments_CounterId",
                schema: "queue",
                table: "counter_assignments",
                column: "CounterId",
                unique: true,
                filter: "\"EndedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_counter_assignments_StaffUserId",
                schema: "queue",
                table: "counter_assignments",
                column: "StaffUserId",
                unique: true,
                filter: "\"EndedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_counters_QueueId_Name",
                schema: "queue",
                table: "counters",
                columns: new[] { "QueueId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_queues_OrganizationId",
                schema: "queue",
                table: "queues",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_queues_OrganizationId_Name",
                schema: "queue",
                table: "queues",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_queues_OrganizationId_TicketPrefix",
                schema: "queue",
                table: "queues",
                columns: new[] { "OrganizationId", "TicketPrefix" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tickets_CounterId_QueueId",
                schema: "queue",
                table: "tickets",
                columns: new[] { "CounterId", "QueueId" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_QueueId_Status_CreatedAt",
                schema: "queue",
                table: "tickets",
                columns: new[] { "QueueId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_QueueId_TicketDate_Number",
                schema: "queue",
                table: "tickets",
                columns: new[] { "QueueId", "TicketDate", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tickets_UserId",
                schema: "queue",
                table: "tickets",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "counter_assignments",
                schema: "queue");

            migrationBuilder.DropTable(
                name: "queue_daily_counters",
                schema: "queue");

            migrationBuilder.DropTable(
                name: "tickets",
                schema: "queue");

            migrationBuilder.DropTable(
                name: "counters",
                schema: "queue");

            migrationBuilder.DropTable(
                name: "queues",
                schema: "queue");
        }
    }
}
