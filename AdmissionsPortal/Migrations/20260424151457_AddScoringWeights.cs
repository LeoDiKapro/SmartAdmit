using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdmissionsPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddScoringWeights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScoringWeights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MasterProgramId = table.Column<int>(type: "INTEGER", nullable: false),
                    GPAWeight = table.Column<int>(type: "INTEGER", nullable: false),
                    YearsWeight = table.Column<int>(type: "INTEGER", nullable: false),
                    LanguageWeight = table.Column<int>(type: "INTEGER", nullable: false),
                    LanguageBonus = table.Column<decimal>(type: "TEXT", nullable: false),
                    DiplomaBonus = table.Column<decimal>(type: "TEXT", nullable: false),
                    RecommendationBonus = table.Column<decimal>(type: "TEXT", nullable: false),
                    DocumentBonus = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringWeights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoringWeights_MasterPrograms_MasterProgramId",
                        column: x => x.MasterProgramId,
                        principalTable: "MasterPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScoringWeights_MasterProgramId",
                table: "ScoringWeights",
                column: "MasterProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScoringWeights");
        }
    }
}
