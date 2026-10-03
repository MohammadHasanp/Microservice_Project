using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Basket.Api.Repository;

public interface IBasketRepository
{
    public Task<Entities.ShoppingCart?> GetUserBasket(string userName);
    public Task<Entities.ShoppingCart?> UpdateBasket(Entities.ShoppingCart shoppingCart);
    public Task DeleteBasket(string userName);
}

public class BasketRepository(IDistributedCache cache) : IBasketRepository
{
    public async Task<Entities.ShoppingCart?> GetUserBasket(string userName)
    {
        var basket = await cache.GetStringAsync(userName);
        return basket == null ? null : JsonConvert.DeserializeObject<Entities.ShoppingCart>(basket);
    }

    public async Task<Entities.ShoppingCart?> UpdateBasket(Entities.ShoppingCart shoppingCart)
    {
        await cache.SetStringAsync(shoppingCart.UserName, JsonConvert.SerializeObject(shoppingCart));
        return await GetUserBasket(shoppingCart.UserName);
    }

    public async Task DeleteBasket(string userName)
    {
        await cache.RemoveAsync(userName);
    }
}