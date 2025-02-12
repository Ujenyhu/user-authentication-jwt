using System.Security.Claims;

namespace userauthjwt.Helpers
{
    //This is to make a user or the claims in the token is an authorized user
    public class AuthenticationHelper
    {
        private readonly IHttpContextAccessor _httpContext;

        public AuthenticationHelper(IHttpContextAccessor httpContext)
        {
           _httpContext = httpContext;
        }

        public bool IsAuthenticated (string userId)
        {
            try
            {
                //retrieve details from the Identity in the httpContext
                var identityDetails = _httpContext.HttpContext.User.Identity as ClaimsIdentity;
                var claim = identityDetails.FindFirst(ClaimTypes.Name);

                if(claim == null)
                {
                    return false;
                }
                if(claim.Value != userId)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public class AuthenticationResponse
        {
            public string? Token { get; set; }
            public string? RefreshToken { get; set; }
        }
    }
}
