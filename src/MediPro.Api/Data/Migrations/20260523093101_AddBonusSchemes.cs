using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediPro.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBonusSchemes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BonusSchemes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Manufacturer = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: true),
                    BuyQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    BonusQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    BannerText = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ValidFromUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValidToUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusSchemes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusSchemes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BonusSchemes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BonusSchemes_ProductId",
                table: "BonusSchemes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusSchemes_TenantId_IsActive",
                table: "BonusSchemes",
                columns: new[] { "TenantId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BonusSchemes");
        }
    }
}
