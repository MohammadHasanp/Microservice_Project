using Basket.Api.Entities;
using Basket.Api.GrpcServices;
using Basket.Api.Repository;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using AutoMapper;
using Basket.Api.Models;
using EventBus.Messages.Events;

namespace Basket.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class BasketController(IBasketRepository repository, IBasketRepository basketRepository,
    IMapper mapper, IPublishEndpoint publishEndpoint, DiscountGrpcService discountGrpcService) : ControllerBase
{
    #region get basket

    [HttpGet("{userName}", Name = "GetBasket")]
    [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
    {
        var basket = await basketRepository.GetUserBasket(userName);
        return Ok(basket ?? new ShoppingCart(userName));
    }

    #endregion

    #region update basket

    [HttpPost]
    [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
    {
        // todo: get data from discount.grpc and calculate final price of product

        foreach (var item in basket.BasketItems)
        {
            var copun = await discountGrpcService.GetDiscountAsync(item.ProductName);
            item.Price -= copun.Amount;
        }

        return Ok(await basketRepository.UpdateBasket(basket));
    }

    #endregion

    #region remove basket

    [HttpDelete("{userName}", Name = "DeleteBasket")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> DeleteBasket(string userName)
    {
        await basketRepository.DeleteBasket(userName);
        return Ok();
    }

    #endregion

    #region checkout

    [HttpPost("[action]")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
    {
        // get existing basket with total price
        var basket = await basketRepository.GetUserBasket(basketCheckout.UserName);
        if (basket == null)
        {
            return BadRequest();
        }


        // create BasketCheckoutEvent -- set total price on basketCheckout event message
        var eventMessage = mapper.Map<BasketCheckoutEvent>(basketCheckout);
        eventMessage.TotalPrice = basket.TotalPrice;

        // send checkout event to rabbitmq
        await publishEndpoint.Publish(eventMessage);

        // remove basket
        await basketRepository.DeleteBasket(basketCheckout.UserName);


        return Accepted();
    }

    #endregion
}
