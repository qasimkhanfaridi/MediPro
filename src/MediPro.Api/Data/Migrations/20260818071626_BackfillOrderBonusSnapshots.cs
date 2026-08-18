using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediPro.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillOrderBonusSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Orders placed before bonus snapshots existed show no free packs. Fill them from the
            // scheme that currently applies to each product so old receipts read correctly.
            migrationBuilder.Sql(
                """
                WITH applicable AS (
                    SELECT
                        l.Id AS LineId,
                        bs.BuyQuantity,
                        bs.BonusQuantity,
                        ROW_NUMBER() OVER (
                            PARTITION BY l.Id
                            ORDER BY bs.SortOrder, bs.Title
                        ) AS Ranking
                    FROM OrderLines l
                    JOIN Orders o ON o.Id = l.OrderId
                    JOIN BonusSchemes bs
                        ON bs.ProductId = l.ProductId
                       AND bs.TenantId = o.TenantId
                       AND bs.IsActive = 1
                       AND bs.BuyQuantity > 0
                       AND bs.BonusQuantity > 0
                )
                UPDATE OrderLines
                SET BonusLabelSnapshot = (
                        SELECT CAST(a.BuyQuantity AS TEXT) || '+' || CAST(a.BonusQuantity AS TEXT)
                        FROM applicable a
                        WHERE a.LineId = OrderLines.Id AND a.Ranking = 1
                    ),
                    BonusQuantitySnapshot = (
                        SELECT (OrderLines.Quantity / a.BuyQuantity) * a.BonusQuantity
                        FROM applicable a
                        WHERE a.LineId = OrderLines.Id AND a.Ranking = 1
                    )
                WHERE BonusLabelSnapshot IS NULL
                  AND EXISTS (
                      SELECT 1 FROM applicable a
                      WHERE a.LineId = OrderLines.Id AND a.Ranking = 1
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE OrderLines
                SET BonusLabelSnapshot = NULL,
                    BonusQuantitySnapshot = 0;
                """);
        }
    }
}
