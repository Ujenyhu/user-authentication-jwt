using userauthjwt.Helpers;
using Microsoft.AspNetCore.Http;

namespace userauthjwt.Responses
{
     #pragma warning disable CS8618
    public class ResponseBase<T>
    {
        public ResponseBase()
        {
        }

        public ResponseBase(T data)
        {
            Data = data;
        }
        public ResponseBase(T data, int statusCode, string message, string status)
        {
            StatusCode = statusCode;
            Message = message;
            Status = status;
            Data = data;
        }

        public ResponseBase(int statusCode, string message, string status)
        {
            StatusCode = statusCode;
            Message = message;
            Status = status;
        }
        public T Data { get; set; }
        public string Status { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
