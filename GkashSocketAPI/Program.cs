using GkashSocketAPI.Core;
using GkashSocketAPI.Repository;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IGkashRepository, GkashRepository>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string url = builder.Configuration.GetValue<string>("Urls");

builder.WebHost.UseUrls(url);

builder.Host.UseSerilog((ctx, loggerConfig) =>
{
    loggerConfig.MinimumLevel.Information()
           .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogEventLevel.Warning)
           .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
           .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .WriteTo.File(new RenderedCompactJsonFormatter(),
               path: $@"C:\SeriLog\GkashSocketAPI\GkashSocketAPI_.txt",
               rollingInterval: RollingInterval.Month);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
