using Shopping.Aggregator.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
// Add services to the container.

services.AddSwaggerGen();
services.AddHttpClient<ICatalogService, CatalogService>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:CatalogUrl"]!);
});

services.AddHttpClient<IBasketService, BasketService>(b =>
{
    b.BaseAddress = new Uri(builder.Configuration["ApiSettings:BasketUrl"]!);
});

services.AddHttpClient<IOrderService, OrderService>(o =>
{
    o.BaseAddress = new Uri(builder.Configuration["ApiSettings:OrderingUrl"]!);
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();
