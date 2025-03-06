using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Net;
using userauthjwt.BusinessLogic.Interfaces;
using userauthjwt.BusinessLogic.Interfaces.User;
using userauthjwt.Helpers;
using userauthjwt.Middlewares.Exceptions;
using userauthjwt.Responses;

namespace userauthjwt.Middlewares.Authorization
{
    public class UserAuthorizationFilter : IMiddleware
    {
        private readonly ILogger<UserAuthorizationFilter> _logger;
        private readonly IServicesWrapper _services;
        public UserAuthorizationFilter(ILogger<UserAuthorizationFilter> logger, IServicesWrapper services)
        {
           _logger = logger;
            _services = services;
        } 

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            try
            {
                var endpoint = httpContext.GetEndpoint();
                var allowAnon = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();


                await next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleAuthException(httpContext, ex);
            }
        }

        private Task HandleAuthException(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(JsonConvert.SerializeObject(new ResponseBase<object>(context.Response.StatusCode, ex.Message, VarHelper.ResponseStatus.ERROR.ToString(), ex.InnerException?.Message)));
        }
    }
}
