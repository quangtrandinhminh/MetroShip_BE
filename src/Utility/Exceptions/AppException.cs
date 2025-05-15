using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using MetroShip.Utility.Constants;

namespace MetroShip.Utility.Exceptions
{
    public class AppException : Exception
    {
        public AppException(string code, string message = "", int statusCode = StatusCodes.Status500InternalServerError)
            : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }


        public string Code { get; }

        public int StatusCode { get; set; }

        [Newtonsoft.Json.JsonExtensionData] public Dictionary<string, object> AdditionalData { get; set; }
    }


    public class BadRequestException : ErrorException
    {
        public BadRequestException(string errorCode, string message = null)
            : base(400, errorCode, message)
        {
        }
        public BadRequestException(
            ICollection<KeyValuePair<string, ICollection<string>>> errors)
            : base(400, new ErrorDetail
            {
                ErrorCode = ErrorCode.BadRequest,
                ErrorMessage = errors
            })
        {
        }
    }

    public class ErrorException : Exception
    {
        public int StatusCode { get; }

        public ErrorDetail ErrorDetail { get; }

        public ErrorException(int statusCode, string errorCode, string message = null)
        {
            StatusCode = statusCode;
            ErrorDetail = new ErrorDetail
            {
                ErrorCode = errorCode,
                ErrorMessage = message
            };
        }

        public ErrorException(int statusCode, ErrorDetail errorDetail)
        {
            StatusCode = statusCode;
            ErrorDetail = errorDetail;
        }
    }

    public class ErrorDetail
    {
        [JsonPropertyName("errorCode")] public string ErrorCode { get; set; }

        [JsonPropertyName("errorMessage")] public object ErrorMessage { get; set; }
    }
}
