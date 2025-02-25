
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using System.Net;
using userauthjwt.Helpers;
using userauthjwt.Responses;

namespace userauthjwt.Middlewares.Exceptions
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {

        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) => _logger = logger;

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            try
            {
                await next(httpContext);
            }
            catch(Exception ex)
            {
               await HandleException(httpContext, ex);
            }
        }

        private Task HandleException(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(JsonConvert.SerializeObject(new ResponseBase<object>(context.Response.StatusCode, ex.Message, VarHelper.ResponseStatus.ERROR.ToString(), ex.InnerException?.Message)));
        }

    }
}
