namespace MediPro.Api.Entities;

public class OrderLine
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string ProductNameSnapshot { get; set; } = "";
    public string PackSnapshot { get; set; } = "";
    public decimal UnitPriceSnapshot { get; set; }
    public int Quantity { get; set; }
    public string? BonusLabelSnapshot { get; set; }
    public int BonusQuantitySnapshot { get; set; }
    public decimal LineTotal { get; set; }
}
