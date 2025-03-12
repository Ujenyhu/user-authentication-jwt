using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using userauthjwt.BusinessLogic.Interfaces;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Helpers;
using userauthjwt.Responses;

namespace userauthjwt.Filters
{

    public class ValidateUserFilter : IAsyncActionFilter
    {
        private readonly IServicesWrapper _services;
        public ValidateUserFilter(IServicesWrapper services)
        {
            _services = services;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext _HttpContext, ActionExecutionDelegate next)
        {
            string? userId = null;

            // Check if UserId is in Query
            if (_HttpContext.HttpContext.Request.Query.TryGetValue("UserId", out var queryUserId))
            {
                userId = queryUserId;
            }
            //  Check if UserId is in the Request Body
            else
            {
                var requestObject = _HttpContext.ActionArguments.Values.FirstOrDefault();

                if (requestObject != null)
                {
                    var userIdProperty = requestObject.GetType().GetProperty("UserId");

                    if (userIdProperty != null)
                    {
                        userId = userIdProperty.GetValue(requestObject)?.ToString();

                    }

                }
            }

            //validate userId that comes with request
            if (string.IsNullOrWhiteSpace(userId) || !_services.AuthenticationService.IsValidUser(userId))
            {
                //_HttpContext.Result = new UnauthorizedObjectResult(new ResponseBase<object>(
                //    (int)HttpStatusCode.Unauthorized,
                //    "Invalid user credentials. Please login and try again.",
                //    VarHelper.ResponseStatus.ERROR.ToString()
                //));
                //return;
                await HandleAuthResponseAsync(_HttpContext.HttpContext, "Invalid user credentials. Please login and try again.");
            }
           
            await next();

        }

        private Task HandleAuthResponseAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(JsonConvert.SerializeObject(new ResponseBase<string>(null, context.Response.StatusCode, message, VarHelper.ResponseStatus.ERROR.ToString())));
        }
    }
}
