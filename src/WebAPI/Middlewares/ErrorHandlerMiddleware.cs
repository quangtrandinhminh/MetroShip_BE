using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ApiModels_BaseResponse = MetroShip.Service.ApiModels.BaseResponse;
using ILogger = Serilog.ILogger;

namespace MetroShip.WebAPI.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        // handle exceptions
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    await HandleUnauthorizedAsync(context);
                }

                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    var roles = GetRequiredRoles(context);
                    await HandleForbiddenAsync(context, roles ?? string.Empty);
                }
            }
            catch (AppException ex)
            {
                await HandleExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        // get required roles from endpoint metadata
        private static string? GetRequiredRoles(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var authorizeData = endpoint?.Metadata?.GetMetadata<IAuthorizeData>();

            return authorizeData?.Roles;
        }

        // handle 401 error
        public static Task HandleUnauthorizedAsync(HttpContext context)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = StatusCodes.Status401Unauthorized;

            var message = "You need AccessToken to access this resource. If token was provided, it may be invalid or expired";
            var result = ParseResponse(response, HttpResponseCodeConstants.UNAUTHORIZED, message);
            return context.Response.WriteAsync(result);
        }

        // handle 403 error
        private static Task HandleForbiddenAsync(HttpContext context, string requiredRoles)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = StatusCodes.Status403Forbidden;

            var message = !string.IsNullOrEmpty(requiredRoles)
                ? ResponseMessageIdentity.USER_NOT_ALLOWED + $" You need '{requiredRoles}' role to access this resource."
                : ResponseMessageIdentity.USER_NOT_ALLOWED;

            var result = ParseResponse(response, HttpResponseCodeConstants.FORBIDDEN, message);
            return context.Response.WriteAsync(result);
        }

        // handle 400 error
        private static async Task HandleExceptionAsync(HttpContext context, AppException ex)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = ex.StatusCode;

            var result = ParseResponse(response, ex.Code, ex.Message);
            await response.WriteAsync(result);
        }

        // handle 500 error
        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = StatusCodes.Status500InternalServerError;

            var result = ParseResponse(response, HttpResponseCodeConstants.INTERNAL_SERVER_ERROR, ex.Message);
            await response.WriteAsync(result);
        }

        // parse response to json
        private static string ParseResponse(HttpResponse response, string code, string message)
        {
            var data = new ApiModels_BaseResponse(response.StatusCode, code, message);
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }
}
