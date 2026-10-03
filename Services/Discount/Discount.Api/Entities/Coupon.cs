namespace Discount.Api.Entities;

public class Coupon
{
    public int Id { get; set; }
    public string ProductName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Amount { get; set; }
}