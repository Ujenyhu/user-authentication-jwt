using userauthjwt.Helpers;
using Microsoft.AspNetCore.Http;

namespace userauthjwt.Responses
{
    public class ResponseBase<T>
    {
        public ResponseBase()
        {
        }

        public ResponseBase(T data)
        {
            StatusCode = StatusCodes.Status200OK;
            Message = "Successful";
            Status = VarHelper.ResponseStatus.SUCCESS.ToString();
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

        public ResponseBase(int statusCode, string message, string status, T data)
        {
            StatusCode = statusCode;
            Message = message;
            Status = status;
            Data = data;
        }

        public ResponseBase(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }


        /// <summary>
        /// The HTTP status code of the response.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// The status of the response (e.g., SUCCESS, ERROR).
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// A message providing additional information about the response.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// The actual data being returned in the response. This field can be null
        /// </summary>
        public T Data { get; set; }
    }
}
