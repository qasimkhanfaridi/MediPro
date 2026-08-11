namespace MediPro.Api.Domain;

/// <summary>Allowed distributor transitions (admin). Same status is always allowed (idempotent).</summary>
public static class OrderStatusRules
{
    private static readonly IReadOnlyDictionary<OrderStatus, OrderStatus[]> Allowed = new Dictionary<OrderStatus, OrderStatus[]>
    {
        [OrderStatus.Submitted] = [OrderStatus.Confirmed, OrderStatus.OnHold, OrderStatus.Rejected, OrderStatus.Cancelled],
        [OrderStatus.OnHold] = [OrderStatus.Confirmed, OrderStatus.Rejected, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Processing, OrderStatus.OnHold, OrderStatus.Cancelled],
        [OrderStatus.Processing] = [OrderStatus.Dispatched, OrderStatus.OnHold, OrderStatus.Cancelled],
        [OrderStatus.Dispatched] = [OrderStatus.Delivered, OrderStatus.Cancelled],
        [OrderStatus.Rejected] = [],
        [OrderStatus.Delivered] = [],
        [OrderStatus.Cancelled] = [],
    };

    public static bool CanTransition(OrderStatus from, OrderStatus to)
    {
        if (from == to)
            return true;
        return Allowed.TryGetValue(from, out var next) && next.Contains(to);
    }

    public static string AllowedTargetsSummary(OrderStatus from)
    {
        if (!Allowed.TryGetValue(from, out var next) || next.Length == 0)
            return "none (terminal)";
        return string.Join(", ", next.Select(s => s.ToString()));
    }
}
