using Discount.Grpc.Context;
using Discount.Grpc.Extensions;
using Discount.Grpc.Repository;
using Discount.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddTransient<IDiscountRepository, DiscountRepository>();
builder.Services.AddTransient(_ =>
{
    var connection = builder.Configuration.GetValue<string>("ConnectionSetting:ConnectionString");
    return new DiscountModuleContext(connection!);
});

var app = builder.Build();
app.MigrateDatabase<Program>();

// Configure the HTTP request pipeline.
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapGrpcService<DiscountGrpcService>();

app.Run();
