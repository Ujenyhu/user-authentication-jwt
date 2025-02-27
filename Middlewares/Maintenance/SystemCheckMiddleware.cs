using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Middlewares.Exceptions;

namespace userauthjwt.Middlewares.Maintenance
{
    public class SystemCheckMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IRepositoryWrapper _repository;
        public SystemCheckMiddleware(ILogger<ExceptionHandlingMiddleware> logger, IRepositoryWrapper repository)
        {
            _logger = logger;
            _repository = repository;
        }
        

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {

        }

    }
}
