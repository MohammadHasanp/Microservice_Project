using Discount.Grpc.Context;
using Npgsql;

namespace Discount.Grpc.Extensions;

public static class HostExtensions
{
    public static IHost MigrateDatabase<TLogger>(this IHost host, int? retry = 0)
    {
        var retryForAvailability = retry;
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<DiscountModuleContext>();
        var logger = services.GetRequiredService<ILogger<TLogger>>();

        //migrate database 
        try
        {
            logger.LogInformation("migrating postgresql database");
            using var connection = context.CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.Connection = connection;

            command.CommandText = "DROP TABLE IF EXISTS Coupon";
            command.ExecuteNonQuery();

            command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY,
                                                            ProductName VARCHAR(100) NOT NULL ,
                                                            Description TEXT ,
                                                            Amount INT)";
            command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO Coupon(ProductName,Description,Amount) VALUES ('IPhone x','Iphone discount',15000)";
            command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO Coupon(ProductName,Description,Amount) VALUES ('IPhone y','Iphone discount',180000)";
            command.ExecuteNonQuery();
            logger.LogInformation("migration has been completed !!!!");
        }
        catch (NpgsqlException)
        {
            logger.LogError("an error has been occured");
            if (retryForAvailability < 50)
            {
                retryForAvailability++;
                Thread.Sleep(200);
                host.MigrateDatabase<TLogger>(retryForAvailability);
            }
        }
        return host;
    }
}