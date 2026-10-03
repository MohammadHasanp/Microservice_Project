using System.Data;
using Npgsql;

namespace Discount.Api.Context;

public class DiscountModuleContext(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(connectionString: _connectionString);
    }
}