using System.Net;
using System.Security.Claims;
using userauthjwt.Models.User;
using static userauthjwt.BusinessLogic.Services.User.AuthenticationService;

namespace userauthjwt.BusinessLogic.Interfaces.User
{
    public interface IAuthenticationService
    {
        bool IsValidUser(string UserId);
        string GenerateToken(string userId, string userType, int expiration);

        string GenerateAccessToken(IEnumerable<Claim> claims, int expiration);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        void IncrementFailedLoginAttempts(UserProfile user);
        void ResetFailedLoginAttempts(UserProfile user);
        bool IsAccountLocked(UserProfile user);
        void LockAccount(UserProfile user, int locktime);
        void ResetAccountLock(UserProfile user);
        bool IsValidCredentials(UserProfile user, string password);
        bool IsAccountDisabled(UserProfile user);
        void DisableAccount(UserProfile user);
        void ResetDisabledAccount(UserProfile user);



        IPAddress? GetRemoteHostIpAddressUsingXForwardedFor();
        IPAddress? GetRemoteHostIpAddressUsingXRealIp();
        UserAgentInfo GetBrowserInfo();
        Task<LocationData?> GetIPInfo();
    }
}
