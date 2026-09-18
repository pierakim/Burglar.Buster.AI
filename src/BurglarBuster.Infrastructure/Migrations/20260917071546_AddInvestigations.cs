using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BurglarBuster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestigations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Investigations",
                columns: table => new
                {
                    InvestigationId = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Outcome = table.Column<int>(type: "INTEGER", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    RequiresHumanReview = table.Column<bool>(type: "INTEGER", nullable: false),
                    ModelDeploymentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ModelTurnCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ToolCallCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TerminationReason = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedInformationJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investigations", x => x.InvestigationId);
                });

            migrationBuilder.CreateTable(
                name: "InvestigationCandidates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InvestigationId = table.Column<string>(type: "TEXT", nullable: false),
                    PersonId = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchedEvidenceJson = table.Column<string>(type: "TEXT", nullable: false),
                    ConflictsJson = table.Column<string>(type: "TEXT", nullable: false),
                    MissingEvidenceJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestigationCandidates_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalTable: "Investigations",
                        principalColumn: "InvestigationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationCandidates_InvestigationId",
                table: "InvestigationCandidates",
                column: "InvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationCandidates_PersonId",
                table: "InvestigationCandidates",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_CreatedAtUtc",
                table: "Investigations",
                column: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestigationCandidates");

            migrationBuilder.DropTable(
                name: "Investigations");
        }
    }
}
