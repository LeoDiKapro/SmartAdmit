using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdmissionsPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddRankingFieldsAndRejectLetter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailableSpots",
                table: "MasterPrograms",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinScore",
                table: "MasterPrograms",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Applications",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepNotes",
                table: "Applications",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AvailableSpots", "MinScore" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AvailableSpots", "MinScore" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AvailableSpots", "MinScore" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AvailableSpots", "MinScore" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AvailableSpots", "MinScore" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AvailableSpots", "MinScore" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AvailableSpots", "MinScore" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AvailableSpots", "MinScore" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MasterPrograms",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AvailableSpots", "MinScore" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableSpots",
                table: "MasterPrograms");

            migrationBuilder.DropColumn(
                name: "MinScore",
                table: "MasterPrograms");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "RepNotes",
                table: "Applications");
        }
    }
}
