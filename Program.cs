using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using PropertyService.Data;
using Common.Extensions;
using Common.Middleware;
using PropertyService.Repositories;
using PropertyDomainService = PropertyService.Services.PropertyService;
using PropertyService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7026, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });

    options.ListenLocalhost(5203, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
});

builder.Services.AddSnowflakeIdGenerator(builder.Configuration);
builder.Services.AddApiControllers();
builder.Services.AddGrpc();
builder.Services.AddOpenApi();
builder.Services.AddKeycloakJwtAuth(builder.Configuration);
builder.Services.AddScoped<PropertyRepository>();
builder.Services.AddScoped<RoomTypeRepository>();
builder.Services.AddScoped<RoomInventoryRepository>();
builder.Services.AddScoped<PropertyDomainService>();
builder.Services.AddScoped<RoomTypeService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history")));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<HttpExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<GrpcService>();
app.MapControllers();

app.Run();
