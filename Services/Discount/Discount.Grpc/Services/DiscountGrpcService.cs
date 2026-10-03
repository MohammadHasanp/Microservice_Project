using Discount.Grpc.Entities;
using Discount.Grpc.Protos;
using Discount.Grpc.Repository;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Discount.Grpc.Services;

public class DiscountGrpcService(IDiscountRepository repository) : DiscountProto.DiscountProtoBase
{
    public override async Task<DiscountModel> GetDiscount(DiscountRequest request, ServerCallContext context)
    {
        var result = await repository.GetCoupon(request.ProductName);
        if (result is null)
            return new DiscountModel();

        return new DiscountModel
        {
            Id = result.Id,
            ProductName = result.ProductName,
            Description = result.Description,
            Amount = result.Amount
        };
    }

    public override async Task<Empty> CreateDiscount(DiscountModel request, ServerCallContext context)
    {
        var coupon = new Coupon
        {
            Id = request.Id,
            ProductName = request.ProductName,
            Description = request.Description,
            Amount = request.Amount
        };
        await repository.Create(coupon);
        return new Empty();
    }

    public override async Task<DiscountResponse> DeleteDiscount(DiscountRequest request, ServerCallContext context)
    {
        var result = await repository.Delete(request.ProductName);
        return new DiscountResponse
        {
            Result = result
        };
    }

    public override async Task<DiscountResponse> UpdateDiscount(DiscountModel request, ServerCallContext context)
    {
        var coupon = new Coupon
        {
            Id = request.Id,
            ProductName = request.ProductName,
            Description = request.Description,
            Amount = request.Amount
        };
        var result = await repository.Update(coupon);
        return new DiscountResponse
        {
            Result = result
        };
    }
}