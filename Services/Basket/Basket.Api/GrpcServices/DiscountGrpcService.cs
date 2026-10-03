using Discount.Grpc.Protos;

namespace Basket.Api.GrpcServices;

public class DiscountGrpcService(DiscountProto.DiscountProtoClient client)
{
    public async Task<DiscountModel> GetDiscountAsync(string productName)
    {
       return await client.GetDiscountAsync(new DiscountRequest { ProductName = productName });
    }
}