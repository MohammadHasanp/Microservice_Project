using Discount.Api.Context;
using Discount.Api.Extensions;
using Discount.Api.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddTransient<IDiscountRepository, DiscountRepository>();
builder.Services.AddTransient(_ =>
{
    var connection = builder.Configuration.GetValue<string>("ConnectionSetting:ConnectionString");
    return new DiscountModuleContext(connection!);
});

var app = builder.Build();
app.MigrateDatabase<Program>();

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
