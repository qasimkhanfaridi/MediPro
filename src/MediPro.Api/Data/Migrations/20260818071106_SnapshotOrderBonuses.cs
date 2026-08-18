using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediPro.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SnapshotOrderBonuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BonusLabelSnapshot",
                table: "OrderLines",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BonusQuantitySnapshot",
                table: "OrderLines",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BonusLabelSnapshot",
                table: "OrderLines");

            migrationBuilder.DropColumn(
                name: "BonusQuantitySnapshot",
                table: "OrderLines");
        }
    }
}
