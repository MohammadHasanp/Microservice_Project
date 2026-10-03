namespace Basket.Api.Entities;

public class ShoppingCart(string userName)
{
    public string UserName { get; set; } = userName;
    public List<ShoppingCartItem> BasketItems { get; set; } = [];

    public decimal TotalPrice
    {
        get
        {
            return BasketItems.Sum(d => d.Price * d.Quantity);
        }
    }
}