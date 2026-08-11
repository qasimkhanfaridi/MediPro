using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediPro.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "Stores",
                type: "TEXT",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Stores");
        }
    }
}
