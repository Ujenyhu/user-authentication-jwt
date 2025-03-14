﻿
using Newtonsoft.Json;
using System.Net;
using System.Text.Json;
using userauthjwt.BusinessLogic.Interfaces.User;
using userauthjwt.Helpers;
using userauthjwt.Responses;

namespace userauthjwt.Middlewares.Authorization
{

    /* Initially, I used IMiddleware for my authorization filter to restrict unauthorized users 
     * before the request reaches the controller. This ensures that endpoints requiring JWT authentication 
     * are validated early, before routing, preventing unnecessary processing.
     *
     * However, this approach introduced additional implementation, such as:
     * 1. Checking if the endpoint allows anonymous access.
     * 2. Determining whether the user ID is passed in the query, path, or request body.
     *
     * While these checks are valid, it's important to choose the most suitable implementation for the application's needs. 
     * Since my goal is to enforce user validation at the method or class level, I decided to use Action Filters instead of middleware.
     */


    //This middleware will not be registered in the pipeline
    public class UserAuthorizationFilter : IMiddleware
    {
        private readonly ILogger<UserAuthorizationFilter> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        public UserAuthorizationFilter(ILogger<UserAuthorizationFilter> logger, IServiceScopeFactory scopeFactory)
        {
           _logger = logger;
            _scopeFactory = scopeFactory;
        } 

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

                    var endpoint = httpContext.GetEndpoint();
                    var allowAnon = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();

                    if (allowAnon == null)
                    {
                        string userId = httpContext.Request.Query["UserId"].ToString();

                        //If not found in query, check request body
                        if (string.IsNullOrEmpty(userId) && httpContext.Request.ContentType == "application/json")
                        {
                            httpContext.Request.EnableBuffering();

                            using (var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true))
                            {
                                var body = await reader.ReadToEndAsync();
                                httpContext.Request.Body.Position = 0;

                                if (!string.IsNullOrEmpty(body))
                                {
                                    var jsonDoc = JsonDocument.Parse(body);
                                    if (jsonDoc.RootElement.TryGetProperty("userId", out var userIdReq))
                                    {
                                        userId = userIdReq.GetString();
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrWhiteSpace(userId))
                        {
                            await HandleAuthException(httpContext, null);
                        }
                        else
                        {
                            var isValidUser = _authService.IsValidUser(userId);
                            if (!isValidUser) await HandleAuthException(httpContext, null);
                        }

                    }

                }
                await next(httpContext);

            }
            catch (Exception ex)
            {
                await HandleAuthException(httpContext, ex);
            }
        }

        private Task HandleAuthException(HttpContext context, Exception? ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(JsonConvert.SerializeObject(new ResponseBase<object>(context.Response.StatusCode, ex.Message ?? "Invalid User. Please, login and try again.", VarHelper.ResponseStatus.ERROR.ToString())));
        }
    }
}
