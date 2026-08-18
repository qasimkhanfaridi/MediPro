using System.Globalization;
using ClosedXML.Excel;
using MediPro.Api.Contracts;
using MediPro.Api.Data;
using MediPro.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediPro.Api.Services;

public sealed class ProductExcelImporter(MediProDbContext db)
{
    private static readonly string[] RequiredHeaders =
    [
        "SkuCode", "Name", "Pack", "Manufacturer", "SaltComposition", "TradePrice",
    ];

    private static readonly string[] OptionalHeaders =
    [
        "Mrp", "StockQuantity", "IsActive", "Category",
    ];

    public async Task<ImportProductsResultDto> ImportAsync(Guid tenantId, Stream stream, CancellationToken ct)
    {
        var result = new ImportProductsResultDto();
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.FirstOrDefault();
        if (ws is null)
        {
            result.Errors.Add(new ImportRowErrorDto { RowNumber = 0, Message = "Workbook has no worksheets." });
            return result;
        }

        var headerRow = ws.FirstRowUsed()?.RowNumber() ?? 0;
        if (headerRow == 0)
        {
            result.Errors.Add(new ImportRowErrorDto { RowNumber = 0, Message = "Worksheet is empty." });
            return result;
        }

        var colMap = BuildHeaderMap(ws, headerRow);
        foreach (var h in RequiredHeaders)
        {
            if (!colMap.ContainsKey(NormalizeHeader(h)))
            {
                result.Errors.Add(new ImportRowErrorDto
                {
                    RowNumber = headerRow,
                    Message = $"Missing required column '{h}'. Expected headers: {string.Join(", ", RequiredHeaders.Concat(OptionalHeaders))}.",
                });
                return result;
            }
        }

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
        var existingSkus = await db.Products.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .Select(p => p.SkuCode)
            .ToListAsync(ct);
        var existingSet = new HashSet<string>(existingSkus, StringComparer.OrdinalIgnoreCase);
        var seenInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var toInsert = new List<Product>();
        var now = DateTime.UtcNow;

        for (var r = headerRow + 1; r <= lastRow; r++)
        {
            ct.ThrowIfCancellationRequested();
            if (IsRowEmpty(ws, r, colMap))
                continue;

            result.TotalRowsAttempted++;

            var sku = GetCellString(ws, r, colMap, "SkuCode");
            if (string.IsNullOrWhiteSpace(sku))
            {
                result.Errors.Add(new ImportRowErrorDto { RowNumber = r, Message = "SkuCode is empty." });
                continue;
            }

            sku = sku.Trim();
            if (seenInFile.Contains(sku))
            {
                result.Errors.Add(new ImportRowErrorDto { RowNumber = r, Message = $"Duplicate SkuCode in file: {sku}." });
                continue;
            }

            if (existingSet.Contains(sku))
            {
                result.Errors.Add(new ImportRowErrorDto { RowNumber = r, Message = $"SkuCode already exists in catalog: {sku}." });
                continue;
            }

            var name = GetCellString(ws, r, colMap, "Name");
            var pack = GetCellString(ws, r, colMap, "Pack");
            var manufacturer = GetCellString(ws, r, colMap, "Manufacturer");
            var salt = GetCellString(ws, r, colMap, "SaltComposition");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(pack)
                || string.IsNullOrWhiteSpace(manufacturer) || string.IsNullOrWhiteSpace(salt))
            {
                result.Errors.Add(new ImportRowErrorDto { RowNumber = r, Message = "Name, Pack, Manufacturer, and SaltComposition are required." });
                continue;
            }

            if (!TryGetDecimal(ws, r, colMap, "TradePrice", out var trade, out var priceErr))
            {
                result.Errors.Add(new ImportRowErrorDto { RowNumber = r, Message = priceErr ?? "Invalid TradePrice." });
                continue;
            }

            decimal? mrp = null;
            if (colMap.ContainsKey(NormalizeHeader("Mrp")))
            {
                if (!TryGetOptionalDecimal(ws, r, colMap, "Mrp", out mrp, out var mrpErr))
                {
                    result.Errors.Add(new ImportRowErrorDto { RowNumber = r, Message = mrpErr ?? "Invalid Mrp." });
                    continue;
                }
            }

            int? stockQty = null;
            if (colMap.ContainsKey(NormalizeHeader("StockQuantity")))
            {
                if (!TryGetOptionalInt(ws, r, colMap, "StockQuantity", out stockQty, out var sqErr))
                {
                    result.Errors.Add(new ImportRowErrorDto { RowNumber = r, Message = sqErr ?? "Invalid StockQuantity." });
                    continue;
                }
            }

            var isActive = true;
            if (colMap.ContainsKey(NormalizeHeader("IsActive")))
            {
                var ia = GetCellString(ws, r, colMap, "IsActive");
                if (!string.IsNullOrWhiteSpace(ia))
                    isActive = ParseBool(ia);
            }

            var category = colMap.ContainsKey(NormalizeHeader("Category"))
                ? NullIfEmpty(GetCellString(ws, r, colMap, "Category"))
                : null;

            seenInFile.Add(sku);
            existingSet.Add(sku);

            toInsert.Add(new Product
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SkuCode = sku,
                Name = name.Trim(),
                Pack = pack.Trim(),
                Manufacturer = manufacturer.Trim(),
                SaltComposition = salt.Trim(),
                Category = category,
                TradePrice = trade,
                Mrp = mrp,
                IsActive = isActive,
                StockQuantity = stockQty,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            });
        }

        if (toInsert.Count > 0)
        {
            db.Products.AddRange(toInsert);
            await db.SaveChangesAsync(ct);
        }

        result.InsertedCount = toInsert.Count;
        result.SkippedOrFailedCount = result.Errors.Count;
        return result;
    }

    private static Dictionary<string, int> BuildHeaderMap(IXLWorksheet ws, int headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var row = ws.Row(headerRow);
        foreach (var cell in row.CellsUsed())
        {
            var raw = cell.GetString().Trim();
            if (string.IsNullOrEmpty(raw))
                continue;
            map[NormalizeHeader(raw)] = cell.Address.ColumnNumber;
        }

        return map;
    }

    private static string NormalizeHeader(string h) =>
        h.Trim().Replace(" ", "", StringComparison.Ordinal).ToLowerInvariant();

    private static bool IsRowEmpty(IXLWorksheet ws, int row, IReadOnlyDictionary<string, int> colMap)
    {
        foreach (var col in colMap.Values)
        {
            var v = ws.Cell(row, col).GetString().Trim();
            if (!string.IsNullOrEmpty(v))
                return false;
        }

        return true;
    }

    private static string GetCellString(IXLWorksheet ws, int row, IReadOnlyDictionary<string, int> colMap, string header)
    {
        var key = NormalizeHeader(header);
        if (!colMap.TryGetValue(key, out var col))
            return "";
        return ws.Cell(row, col).GetString().Trim();
    }

    private static bool TryGetDecimal(IXLWorksheet ws, int row, IReadOnlyDictionary<string, int> colMap, string header, out decimal value, out string? error)
    {
        error = null;
        value = 0;
        var key = NormalizeHeader(header);
        if (!colMap.TryGetValue(key, out var col))
        {
            error = $"Column {header} not mapped.";
            return false;
        }

        var cell = ws.Cell(row, col);
        if (cell.IsEmpty())
        {
            error = $"{header} is required.";
            return false;
        }

        if (cell.DataType == XLDataType.Number)
        {
            value = (decimal)cell.GetDouble();
            return true;
        }

        var s = cell.GetString().Trim();
        if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
        {
            value = dec;
            return true;
        }

        error = $"{header} must be a number.";
        return false;
    }

    private static bool TryGetOptionalDecimal(IXLWorksheet ws, int row, IReadOnlyDictionary<string, int> colMap, string header, out decimal? value, out string? error)
    {
        error = null;
        value = null;
        var s = GetCellString(ws, row, colMap, header);
        if (string.IsNullOrWhiteSpace(s))
            return true;

        if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
        {
            value = dec;
            return true;
        }

        var key = NormalizeHeader(header);
        if (!colMap.TryGetValue(key, out var col))
            return true;

        var cell = ws.Cell(row, col);
        if (cell.DataType == XLDataType.Number)
        {
            value = (decimal)cell.GetDouble();
            return true;
        }

        error = $"{header} must be a number or blank.";
        return false;
    }

    private static bool TryGetOptionalInt(IXLWorksheet ws, int row, IReadOnlyDictionary<string, int> colMap, string header, out int? value, out string? error)
    {
        error = null;
        value = null;
        var s = GetCellString(ws, row, colMap, header);
        if (string.IsNullOrWhiteSpace(s))
            return true;

        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
        {
            value = i;
            return true;
        }

        error = $"{header} must be an integer or blank.";
        return false;
    }

    private static bool ParseBool(string s)
    {
        s = s.Trim();
        if (bool.TryParse(s, out var b))
            return b;
        if (int.TryParse(s, out var i))
            return i != 0;
        return s.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || s.Equals("y", StringComparison.OrdinalIgnoreCase);
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
