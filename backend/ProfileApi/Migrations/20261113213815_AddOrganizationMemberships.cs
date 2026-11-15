using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfileApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "profile",
                table: "user_profiles");

            migrationBuilder.AddColumn<string>(
                name: "ClerkUserId",
                schema: "profile",
                table: "user_profiles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "profile",
                table: "user_profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "profile",
                table: "user_profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "organization_memberships",
                schema: "profile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_organization_memberships_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "profile",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_memberships_user_profiles_UserId",
                        column: x => x.UserId,
                        principalSchema: "profile",
                        principalTable: "user_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_profiles_ClerkUserId",
                schema: "profile",
                table: "user_profiles",
                column: "ClerkUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_memberships_OrganizationId",
                schema: "profile",
                table: "organization_memberships",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_organization_memberships_UserId_OrganizationId",
                schema: "profile",
                table: "organization_memberships",
                columns: new[] { "UserId", "OrganizationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organization_memberships",
                schema: "profile");

            migrationBuilder.DropIndex(
                name: "IX_user_profiles_ClerkUserId",
                schema: "profile",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "ClerkUserId",
                schema: "profile",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "profile",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "profile",
                table: "user_profiles");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "profile",
                table: "user_profiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
