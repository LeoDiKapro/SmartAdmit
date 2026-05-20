using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdmissionsPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddEducationFieldMatching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FieldMatchBonus",
                table: "ScoringWeights",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Field",
                table: "MasterPrograms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UndergraduateField",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 1,
                column: "Field",
                value: 0);

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 2,
                column: "Field",
                value: 2);

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 3,
                column: "Field",
                value: 3);

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 4,
                column: "Field",
                value: 0);

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 5,
                column: "Field",
                value: 1);

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 6,
                column: "Field",
                value: 1);

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 7,
                column: "Field",
                value: 6);

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 8,
                column: "Field",
                value: 2);

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 9,
                column: "Field",
                value: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FieldMatchBonus",
                table: "ScoringWeights");

            migrationBuilder.DropColumn(
                name: "Field",
                table: "MasterPrograms");

            migrationBuilder.DropColumn(
                name: "UndergraduateField",
                table: "AspNetUsers");
        }
    }
}
