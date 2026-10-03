using Dapper;
using Discount.Grpc.Context;
using Discount.Grpc.Entities;

namespace Discount.Grpc.Repository;

public interface IDiscountRepository
{
    public Task Create(Coupon coupon);
    public Task<bool> Delete(string productName);
    public Task<bool> Update(Coupon coupon);
    public Task<Coupon?> GetCoupon(string productName);
}

public class DiscountRepository(DiscountModuleContext context) : IDiscountRepository
{
    public async Task Create(Coupon coupon)
    {
        using var connection = context.CreateConnection();
        const string sql = @"INSERT INTO Coupon (ProductName,Description,Amount)
                   VALUES (@ProductName,@Description,@Amount)";
        await connection.ExecuteAsync(sql,
             new { coupon.ProductName, coupon.Description, coupon.Amount });
    }

    public async Task<bool> Delete(string productName)
    {
        using var connection = context.CreateConnection();
        const string sql = "DELETE FROM Coupon WHERE productName = @productName";
        var result = await connection.ExecuteAsync(sql, new { productName });
        return result > 0;
    }

    public async Task<bool> Update(Coupon coupon)
    {
        using var connection = context.CreateConnection();
        const string sql = "UPDATE Coupon SET ProductName = @productName , Description=@description,Amount = @amount WHERE Id = @Id";
        var result = await connection.ExecuteAsync(sql,
            new { coupon.Id, coupon.ProductName, coupon.Description, coupon.Amount });
        return result > 0;
    }

    public async Task<Coupon?> GetCoupon(string productName)
    {
        using var connection = context.CreateConnection();
        const string sql = "SELECT * FROM Coupon WHERE productName = @productName";
        return await connection.QueryFirstOrDefaultAsync<Coupon>(sql, new { productName });
    }
}