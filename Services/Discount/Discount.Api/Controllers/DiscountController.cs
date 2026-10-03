using System.Net;
using Discount.Api.Entities;
using Discount.Api.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Discount.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class DiscountController(IDiscountRepository repository) : ControllerBase
{
    [HttpGet("{productName}", Name = "GetCoupon")]
    [ProducesResponseType(typeof(Coupon), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Coupon>> GetCoupon(string productName)
    {
        var result = await repository.GetCoupon(productName);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Coupon), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Coupon>> CreateCoupon(string productName, string description, int amount)
    {
        var coupon = new Coupon
        {
            ProductName = productName,
            Description = description,
            Amount = amount
        };
        await repository.Create(coupon);
        return CreatedAtRoute("GetCoupon", new { productName }, coupon);
    }

    [HttpDelete("{productName}")]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<bool>> DeleteCoupon(string productName)
    {
        return Ok(await repository.Delete(productName));
    }

    [HttpPut]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<bool>> UpdateCoupon([FromBody] Coupon coupon)
    {
        return Ok(await repository.Update(coupon));
    }
}