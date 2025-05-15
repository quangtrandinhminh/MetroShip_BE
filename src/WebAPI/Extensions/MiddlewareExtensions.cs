using MetroShip.WebAPI.Middlewares;

namespace MetroShip.WebAPI.Extensions;

using AspNetCoreRateLimit;

public static class MiddlewareExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        // Use custom error handling middleware
        app.UseMiddleware<ErrorHandlerMiddleware>();

        // Use rate limiting middleware
        app.UseIpRateLimiting();

        // Use CORS
        app.UseCors("_myAllowSpecificOrigins");

        // HTTPS redirection
        app.UseHttpsRedirection();

        // Authentication and Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}