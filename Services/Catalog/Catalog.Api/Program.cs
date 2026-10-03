using Catalog.Api.Common.Models;
using Catalog.Api.Context;
using Catalog.Api.Repository;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.

services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
services.AddOpenApi();
services.AddSwaggerGen();

services.AddScoped<CatalogContext>();
services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));
services.AddSingleton<IMongoClient, MongoClient>(provider =>
{
    var settings = provider.GetRequiredService<MongoSettings>();
    return new MongoClient(settings.ConnectionString);
});
services.AddSingleton<MongoSettings>(sp => sp.GetRequiredService<IOptions<MongoSettings>>().Value);
services.AddTransient<IProductRepository, ProductRepository>();

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
