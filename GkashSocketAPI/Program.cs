using GkashSocketAPI.Core;
using GkashSocketAPI.Dto.Settings;
using GkashSocketAPI.Service.Impl;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IGkashService, GkashService>();
builder.Services.Configure<SettingsDto>(builder.Configuration.GetSection("Settings"));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddHttpClient();

string url = builder.Configuration.GetValue<string>("Settings:Urls");
string loggingPath = builder.Configuration.GetValue<string>("Settings:LoggingPath");

builder.WebHost.UseUrls(url);

builder.Host.UseSerilog((ctx, loggerConfig) =>
{
    loggerConfig.MinimumLevel.Information()
           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .WriteTo.File(
               path: loggingPath,
               rollingInterval: RollingInterval.Month);


});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
