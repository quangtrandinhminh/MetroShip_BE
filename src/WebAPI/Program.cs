using Serilog;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using MetroShip.WebAPI.Extensions;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from root folder
Env.Load("../be.env");
string appName = Environment.GetEnvironmentVariable("APP_NAME");
Console.WriteLine($"Environment: {appName}");

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Services.AddSingleton(Log.Logger);
builder.Host.UseSerilog();
builder.Logging.AddSerilog(Log.Logger);

// Configure services
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);
builder.Services.AddSwaggerDocumentation(appName, "v1");
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure middleware
app.UseSwaggerDocumentation(appName, "v1");
app.UseApplicationMiddleware();

app.MapControllers();
app.Run();