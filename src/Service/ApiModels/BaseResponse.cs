using MetroShip.Utility.Constants;
using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.ApiModels
{
    public class BaseResponse<T>
    {
        public T? Data { get; set; }
        public object? AdditionalData { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
        public string Code { get; set; }

        public BaseResponse(int statusCode, string code, T? data, object? additionalData = null, string? message = null)
        {
            StatusCode = statusCode;
            Code = code;
            Data = data;
            AdditionalData = additionalData;
            Message = message;
        }

        public BaseResponse(int statusCode, string code, string? message)
        {
            StatusCode = statusCode;
            Code = code;
            Message = message;
        }

        public static BaseResponse<T> OkResponseDto(T data, object? additionalData = null, string code = HttpResponseCodeConstants.SUCCESS)
        {
            return new BaseResponse<T>(StatusCodes.Status200OK, code, data, additionalData);
        }

        public static BaseResponse<T> CreatedResponseDto(T data, object? additionalData = null, string code = HttpResponseCodeConstants.SUCCESS)
        {
            return new BaseResponse<T>(StatusCodes.Status201Created, code, data, additionalData);
        }

        public static BaseResponse<T> NotFoundResponseDto(T? data, object? additionalData = null, string code = HttpResponseCodeConstants.NOT_FOUND)
        {
            return new BaseResponse<T>(StatusCodes.Status404NotFound, code, data, additionalData);
        }

        public static BaseResponse<T> BadRequestResponseDto(T? data, object? additionalData = null, string code = HttpResponseCodeConstants.FAILED)
        {
            return new BaseResponse<T>(StatusCodes.Status400BadRequest, code, data, additionalData);
        }

        public static BaseResponse<T> InternalErrorResponseDto(T? data, object? additionalData = null, string code = HttpResponseCodeConstants.FAILED)
        {
            return new BaseResponse<T>(StatusCodes.Status500InternalServerError, code, data, additionalData);
        }

        public static BaseResponse<T?> OkResponseDto(string message, T? data, object? additionalData = null, string code = HttpResponseCodeConstants.SUCCESS)
        {
            return new BaseResponse<T?>(StatusCodes.Status200OK, code, data, additionalData, message);
        }

        public static BaseResponse<T?> CreatedResponseDto(string message, T? data, object? additionalData = null, string code = HttpResponseCodeConstants.SUCCESS)
        {
            return new BaseResponse<T?>(StatusCodes.Status201Created, code, data, additionalData, message);
        }

        public static BaseResponse<T> NotFoundResponseDto(string message, T? data, object? additionalData = null, string code = HttpResponseCodeConstants.NOT_FOUND)
        {
            return new BaseResponse<T>(StatusCodes.Status404NotFound, code, data, additionalData, message);
        }

        public static BaseResponse<T> BadRequestResponseDto(string message, T? data, object? additionalData = null, string code = HttpResponseCodeConstants.FAILED)
        {
            return new BaseResponse<T>(StatusCodes.Status400BadRequest, code, data, additionalData, message);
        }

        public static BaseResponse<T> InternalErrorResponseDto(string message, T? data, object? additionalData = null, string code = HttpResponseCodeConstants.FAILED)
        {
            return new BaseResponse<T>(StatusCodes.Status500InternalServerError, code, data, additionalData);
        }
    }

    public class BaseResponse : BaseResponse<object>
    {
        public BaseResponse(int statusCode, string code, object? data, object? additionalData = null, string? message = null) : base(statusCode, code, data, additionalData, message)
        {
        }

        public BaseResponse(int statusCode, string code, string? message) : base(statusCode, code, message)
        {
        }
    }
}
