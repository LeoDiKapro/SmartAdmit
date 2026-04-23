using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdmissionsPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_MasterPrograms_MasterProgramId",
                table: "Applications");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_MasterPrograms_MasterProgramId",
                table: "Applications",
                column: "MasterProgramId",
                principalTable: "MasterPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_MasterPrograms_MasterProgramId",
                table: "Applications");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_MasterPrograms_MasterProgramId",
                table: "Applications",
                column: "MasterProgramId",
                principalTable: "MasterPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
