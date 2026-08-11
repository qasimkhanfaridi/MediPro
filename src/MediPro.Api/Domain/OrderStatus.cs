namespace MediPro.Api.Domain;

public enum OrderStatus
{
    Submitted = 0,
    Confirmed = 1,
    OnHold = 2,
    Rejected = 3,
    Processing = 4,
    Dispatched = 5,
    Delivered = 6,
    Cancelled = 7,
}
