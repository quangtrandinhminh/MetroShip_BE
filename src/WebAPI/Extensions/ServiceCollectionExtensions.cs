﻿using Microsoft.EntityFrameworkCore;
using AspNetCoreRateLimit;
using Microsoft.Extensions.Options;
using MetroShip.Repository.Infrastructure;
using MetroShip.Repository.Models.Identity;
using MetroShip.Repository.Interfaces;
using MetroShip.Repository.Repositories;
using MetroShip.Repository.Base;
using MetroShip.Service.Interfaces;
using MetroShip.Service.Mapper;
using MetroShip.Service.Services;
using MetroShip.Utility.Config;
using AppDbContext = MetroShip.Repository.Infrastructure.AppDbContext;
using MapperlyMapper = MetroShip.Service.Mapper.MapperlyMapper;

namespace MetroShip.WebAPI.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register third-party services
        services.RegisterThirdPartyServices();

        // Register infrastructure services
        services.RegisterInfrastructureServices(configuration);

        // Register application-specific services
        RegisterApplicationServices(services);

        return services;
    }

    private static void RegisterThirdPartyServices(this IServiceCollection services)
    {
        // Configure settings
        services.Configure<SystemSettingModel>(options =>
        {
            options.ApplicationName = GetEnvironmentVariableOrThrow("APP_NAME");
            options.Domain = Environment.GetEnvironmentVariable("SYSTEM_DOMAIN");
            options.SecretKey = GetEnvironmentVariableOrThrow("SYSTEM_SECRET_KEY");
            options.SecretCode = GetEnvironmentVariableOrThrow("SYSTEM_SECRET_CODE");
        });
        SystemSettingModel.Instance = services.BuildServiceProvider().GetService<IOptions<SystemSettingModel>>().Value;

        services.Configure<VnPaySetting>(options =>
        {
            options.TmnCode = GetEnvironmentVariableOrThrow("VNPAY_TMN_CODE");
            options.HashSecret = GetEnvironmentVariableOrThrow("VNPAY_HASH_SECRET");
            options.BaseUrl = GetEnvironmentVariableOrThrow("VNPAY_BASE_URL");
            options.Version = GetEnvironmentVariableOrThrow("VNPAY_VERSION");
            options.CurrCode = GetEnvironmentVariableOrThrow("VNPAY_CURR_CODE");
            options.Locale = GetEnvironmentVariableOrThrow("VNPAY_LOCALE");
        });
        VnPaySetting.Instance = services.BuildServiceProvider().GetService<IOptions<VnPaySetting>>().Value;

        services.Configure<SmtpSetting>(options =>
        {
            options.Host = GetEnvironmentVariableOrThrow("SMTP_HOST");
            options.Port = int.Parse(GetEnvironmentVariableOrThrow("SMTP_PORT"));
            options.EnableSsl = bool.Parse(GetEnvironmentVariableOrThrow("SMTP_ENABLE_SSL"));
            options.UsingCredential = bool.Parse(GetEnvironmentVariableOrThrow("SMTP_USING_CREDENTIAL"));
            options.Username = GetEnvironmentVariableOrThrow("SMTP_USERNAME");
            options.Password = GetEnvironmentVariableOrThrow("SMTP_PASSWORD");
        });

        services.Configure<MailSettingModel>(options =>
        {
            options.Smtp = services.BuildServiceProvider().GetService<IOptions<SmtpSetting>>().Value;
            options.FromAddress = GetEnvironmentVariableOrThrow("SMTP_FROM_ADDRESS");
            options.FromDisplayName = GetEnvironmentVariableOrThrow("SMTP_FROM_DISPLAY_NAME");
        });
        MailSettingModel.Instance = services.BuildServiceProvider().GetService<IOptions<MailSettingModel>>().Value;

        services.Configure<VietQRSetting>(options =>
        {
            options.APIKey = GetEnvironmentVariableOrThrow("VIETQR_API_KEY");
            options.ClientID = GetEnvironmentVariableOrThrow("VIETQR_CLIENT_ID");
        });
        VietQRSetting.Instance = services.BuildServiceProvider().GetService<IOptions<VietQRSetting>>().Value;

        services.Configure<GoogleSetting>(options =>
        {
            options.ClientID = GetEnvironmentVariableOrThrow("GOOGLE_CLIENT_ID");
            options.ClientSecret = GetEnvironmentVariableOrThrow("GOOGLE_CLIENT_SECRET");
        });
        GoogleSetting.Instance = services.BuildServiceProvider().GetService<IOptions<GoogleSetting>>().Value;

        services.Configure<PayOSSetting>(options =>
        {
            options.ApiKey = GetEnvironmentVariableOrThrow("PAYOS_API_KEY");
            options.ChecksumKey = GetEnvironmentVariableOrThrow("PAYOS_CHECKSUM_KEY");
            options.ClientID = GetEnvironmentVariableOrThrow("PAYOS_CLIENT_ID");
        });
        PayOSSetting.Instance = services.BuildServiceProvider().GetService<IOptions<PayOSSetting>>().Value;

        services.Configure<CloudinarySetting>(options =>
        {
            options.CloudinaryUrl = GetEnvironmentVariableOrThrow("CLOUDINARY_URL");
        });
        CloudinarySetting.Instance = services.BuildServiceProvider().GetService<IOptions<CloudinarySetting>>().Value;

        services.Configure<TwilioSetting>(options =>
        {
            options.AccountSID = GetEnvironmentVariableOrThrow("TWILIO_ACCOUNT_SID");
            options.AuthToken = GetEnvironmentVariableOrThrow("TWILIO_AUTH_TOKEN");
            options.PhoneNumber = GetEnvironmentVariableOrThrow("TWILIO_PHONE_NUMBER");
        });
    }

    private static void RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Cors
        const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";
        services.AddCors(options =>
        {
            options.AddPolicy(myAllowSpecificOrigins, policy =>
            {
                policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
            });
        });

        // Add DbContext
        services.AddDbContext<AppDbContext>(options =>
          options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")
                      ?? GetEnvironmentVariableOrThrow("POSTGRES_CONNECTION")));
        // Đảm bảo đặt trước dòng AddSignalR
        services.AddSignalR();

        // Add Cache
        services.AddMemoryCache();
        // Add Identity
        services.AddIdentityCore<UserEntity>()
            .AddRoles<RoleEntity>()
            .AddEntityFrameworkStores<AppDbContext>();

        // Memory cache & rate limiting
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
    }

    private static void RegisterApplicationServices(IServiceCollection services)
    {
        // Register services
        services.AddScoped<IMapperlyMapper, MapperlyMapper>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IParcelCategoryService, ParcelCategoryService>();
        services.AddScoped<IParcelService, ParcelService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IStationService, StationService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IVnPayService, VnPayService>();
        services.AddScoped<IShipmentService, ShipmentService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IMetroLineService, MetroLineService>();
        services.AddScoped<IMetroTimeSlotService, MetroTimeSlotService>();
        services.AddScoped<ITrainService, TrainService>();
        services.AddScoped<IStaffAssignmentService, StaffAssignmentService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IRegionService, RegionService>();

        // Register repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IParcelCategoryRepository, ParcelCategoryRepository>();
        services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IStationRepository, StationRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IShipmentItineraryRepository, ShipmentItineraryRepository>();
        services.AddScoped<IMetroLineRepository, MetroLineRepository>();
        services.AddScoped<IParcelRepository, ParcelRepository>();
        services.AddScoped<ITrainRepository, TrainRepository>();
        services.AddScoped<IMetroTimeSlotRepository, MetroTimeSlotRepository>();
        services.AddScoped<IRegionRepository, RegionRepository>();
        services.AddScoped<IStaffAssignmentRepository, StaffAssignmentRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPricingRepository, PricingRepository>();
    }

    private static string GetEnvironmentVariableOrThrow(string key)
    {
        return Environment.GetEnvironmentVariable(key) ?? throw new ArgumentNullException(key, $"Environment variable '{key}' is not set.");
    }
}
