using Serilog;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using MetroShip.WebAPI.Extensions;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

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

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
});

// Configure form options for larger file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 500_000_000; // 500MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Configure IIS Server options (if using IIS)
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 500_000_000; // 500MB
});

// Configure Kestrel options
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 500_000_000; // 500MB
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