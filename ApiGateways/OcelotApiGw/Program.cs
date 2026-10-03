using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    $"ocelot.{builder.Environment.EnvironmentName}.json",
    optional: false,
    reloadOnChange: true);

builder.Logging
    .AddConfiguration(builder.Configuration.GetSection("Logging"));

var services = builder.Services;

//Cache Manager in ocelot
services.AddOcelot(builder.Configuration)
    .AddCacheManager(x => x.WithDictionaryHandle());

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
}


var app = builder.Build();
app.MapGet("/", () => "Hello World!");

await app.UseOcelot();
app.Run();
