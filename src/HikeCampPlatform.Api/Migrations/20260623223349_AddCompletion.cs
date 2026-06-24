using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HikeCampPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Completions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TourId = table.Column<int>(type: "integer", nullable: false),
                    ConfirmedByOperatorId = table.Column<int>(type: "integer", nullable: true),
                    IsSelfReported = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Completions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Completions_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Completions_Operators_ConfirmedByOperatorId",
                        column: x => x.ConfirmedByOperatorId,
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Completions_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Completions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Completions_BookingId",
                table: "Completions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Completions_ConfirmedByOperatorId",
                table: "Completions",
                column: "ConfirmedByOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Completions_TourId",
                table: "Completions",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_Completions_UserId",
                table: "Completions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Completions");
        }
    }
}
