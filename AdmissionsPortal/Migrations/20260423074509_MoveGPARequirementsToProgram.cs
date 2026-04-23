using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdmissionsPortal.Migrations
{
    /// <inheritdoc />
    public partial class MoveGPARequirementsToProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinGPA",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "MinYears",
                table: "Universities");

            migrationBuilder.AddColumn<decimal>(
                name: "MinGPA",
                table: "MasterPrograms",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MinYears",
                table: "MasterPrograms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MinGPA", "MinYears" },
                values: new object[] { 3.0m, 3 });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MinGPA", "MinYears" },
                values: new object[] { 2.5m, 3 });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MinGPA", "MinYears" },
                values: new object[] { 2.8m, 4 });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "MinGPA", "MinYears" },
                values: new object[] { 3.2m, 3 });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "MinGPA", "MinYears" },
                values: new object[] { 3.0m, 3 });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "MinGPA", "MinYears" },
                values: new object[] { 3.0m, 3 });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "MinGPA", "MinYears" },
                values: new object[] { 2.8m, 3 });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "MinGPA", "MinYears" },
                values: new object[] { 2.7m, 3 });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "MinGPA", "MinYears" },
                values: new object[] { 2.5m, 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinGPA",
                table: "MasterPrograms");

            migrationBuilder.DropColumn(
                name: "MinYears",
                table: "MasterPrograms");

            migrationBuilder.AddColumn<decimal>(
                name: "MinGPA",
                table: "Universities",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MinYears",
                table: "Universities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MinGPA", "MinYears" },
                values: new object[] { 1.66m, 3 });

            migrationBuilder.UpdateData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MinGPA", "MinYears" },
                values: new object[] { 2.0m, 3 });

            migrationBuilder.UpdateData(
                table: "Universities",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MinGPA", "MinYears" },
                values: new object[] { 2.33m, 3 });
        }
    }
}
