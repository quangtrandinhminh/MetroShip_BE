namespace MetroShip.WebAPI.Extensions;

using MetroShip.Utility.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MetroShip.Service.Utils;
using MetroShip.Utility.Enums;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        // Add authentication
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = JwtUtils.GetTokenValidationParameters();
            })
            .AddGoogle(options =>
            {
                var googleSetting = GoogleSetting.Instance;
                options.ClientId = googleSetting.ClientID;
                options.ClientSecret = googleSetting.ClientSecret;
                options.Scope.Add("email");
                options.Scope.Add("profile");
                options.SaveTokens = true;
            });

        // Add authorization
        services.AddAuthorization();

        services.AddHttpContextAccessor();

        return services;
    }
}