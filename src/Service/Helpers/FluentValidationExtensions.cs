using FluentValidation;
using MetroShip.Utility.Constants;
using Microsoft.AspNetCore.Http;
using MetroShip.Utility.Exceptions;

namespace MetroShip.Service.Helpers;

public static class FluentValidationExtensions
{
    // This extension method is used to validate an API model using FluentValidation.
    public static void ValidateApiModel<T>(this IValidator<T> v, T instance)
    {
        var result = v.Validate(instance);

        if (!result.IsValid)
        {
            var details = result.Errors
                .First();

            throw new AppException(
                HttpResponseCodeConstants.BAD_REQUEST,
                details.ErrorMessage,
                    StatusCodes.Status400BadRequest);
        }
    }

    // This extension method is used to validate a business entity using FluentValidation,
    // Can be used for more complex validation scenarios with custom error codes.
    public static void ValidateBusiness<T>(this IValidator<T> v, T instance, int statusCode)
    {
        var result = v.Validate(instance);

        if (!result.IsValid)
        {
            var details = result.Errors
                .First();

            throw new AppException(
                 details.ErrorCode,
                 details.ErrorMessage,
                 statusCode);
        }
    }
}
