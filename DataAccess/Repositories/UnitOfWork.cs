using Microsoft.EntityFrameworkCore.Storage;
using userauthjwt.BusinessLogic.Interfaces.User;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Models;

namespace userauthjwt.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthenticationService _authenticationService;


        public UnitOfWork(AppDbContext context, IHttpContextAccessor httpContextAccessor, IAuthenticationService authenticationService)
        {
            _dbContext = context;
            _httpContextAccessor = httpContextAccessor;
            _authenticationService = authenticationService;
        }


        public async Task CommitAsync()
        {
            await _dbContext.SaveChangesAsync();
        }


        public async Task CommitAsync(bool callCustomMethod = true)
        {
            _dbContext.CallCustomMethod = callCustomMethod;

            HttpContext context = _httpContextAccessor.HttpContext;


            if (context != null && context.Request != null)
            {
                _dbContext.UserId = _authenticationService.GetUserIdFromClaim();

                var userAgents = _authenticationService.GetBrowserInfo();

                var ip = _authenticationService.GetIPInfo();
            }


            if (context != null && context.Request.RouteValues != null)
            {
                _dbContext.Controller = context.Request.RouteValues["controller"].ToString();
                _dbContext.Action = context.Request.RouteValues["action"].ToString();
            }


            try
            {
                await _dbContext.SaveChangesAsync();
            }
            finally
            {
                _dbContext.CallCustomMethod = true; // Reset the flag
            }
        }

    }
}
