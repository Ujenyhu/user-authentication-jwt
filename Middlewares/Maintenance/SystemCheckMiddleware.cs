using Newtonsoft.Json;
using System.Net;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Helpers;
using userauthjwt.Middlewares.Exceptions;
using userauthjwt.Responses;

namespace userauthjwt.Middlewares.Maintenance
{
    public class SystemCheckMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        public SystemCheckMiddleware(ILogger<ExceptionHandlingMiddleware> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }


        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();

                var config = await repository.SysConfigRepository.FirstOrDefaultAsync();
                if (config?.LoginTokenExpiration <= 0)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    await httpContext.Response.WriteAsJsonAsync(new ResponseBase<object>
                    {
                        StatusCode = (int)HttpStatusCode.ServiceUnavailable,
                        Message = "We are currently undergoing maintenance. Check back in a few minutes.",
                        Status = VarHelper.ResponseStatus.ERROR.ToString()
                    });

                    return;
                }
            }

            await next(httpContext);
        }

    }
}
