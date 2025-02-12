using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using userauthjwt.Helpers;
using userauthjwt.Models.User;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.BusinessLogic.Interfaces.User;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;

namespace userauthjwt.BusinessLogic.Services.User
{
    public class AuthenticationService : IAuthenticationService
    {
        public const string Issuer = "https://api.userauthservice.com";
        public const string Audience = "https://api.userauthservice.com";
        public string Secret = string.Empty;
        private IConfiguration _config;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IRepositoryWrapper _repository;
        public AuthenticationService(IConfiguration config, IHttpContextAccessor httpContext, IRepositoryWrapper repository)
        {
            _config = config;
            _httpContext = httpContext;
            _repository = repository;

        }

        public bool IsValidUser(string UserId)
        {
            try
            {
                // Extract Authorization header from the HTTP request
                string authorizationHeader = _httpContext?.HttpContext?.Request?.Headers["Authorization"];

                // Check if Authorization header is present and has a valid bearer token
                if (!string.IsNullOrEmpty(authorizationHeader) && (authorizationHeader.StartsWith("Bearer ") || authorizationHeader.StartsWith("bearer ")))
                {
                    // Extract ClaimsPrincipal and look for the "UserId" claim
                    ClaimsPrincipal user = _httpContext?.HttpContext?.User;
                    Claim claim = user?.FindFirst("UserId");

                    if (claim == null)
                    {
                        return false;
                    }
                    if (claim.Value != UserId)
                    {
                        Console.WriteLine("UserId in Claim does not match UserId passed in the header");
                        return false;
                    }
                    return true;
                }
                else
                {
                    Console.WriteLine("No header found");
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }


        //Important note***************
        //The secret is a base64-encoded string, always make sure to use a secure long string so no one can guess it. ever!.
        //a very recommended approach to use is through the HMACSHA256() class, to generate such a secure secret, you can refer to the below function
        // you can run a small test by calling the GenerateSecureSecret() function to generate a random secure secret once, grab it, and use it as the secret above 
        // or you can save it into appsettings.json file and then load it from them, the choice is yours


        public string GenerateToken(string userId, string userType, int expiration)
        {
            var claimsIdentity = new List<Claim>
            {
               new Claim("UserId", userId),
               new Claim("UserType", userType)
            };

            var token = GenerateAccessToken(claimsIdentity, expiration);
            return token;
        }

        public string GenerateAccessToken(IEnumerable<Claim> claims, int expiration)
        {
            var key = Convert.FromBase64String(_config.GetValue<string>("AppSettings:Secret"));
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var tokenOptions = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiration),
                signingCredentials: signingCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return tokenString;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_config.GetValue<string>("AppSettings:Secret"))),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public async void IncrementFailedLoginAttempts(UserProfile user)
        {
            // Increment the failed login attempts for the user
            if (user != null)
            {
                user.FailedLoginAttempt = user.FailedLoginAttempt == null ? 1 : user.FailedLoginAttempt += 1;
                user.LastFailedLoginAttempt = DateTime.Now;
            }

            var failedSecurityAttempts = new FailedSecurityAttempt()
            {
                RecId = AppHelper.GetNewUniqueId(),
                UserId = user.UserId,
                Type = VarHelper.SecurityAttemptTypes.LOGIN.ToString(),
                IP = "",
                DateCreated = DateTime.Now
            };

            await _repository.UserRepository.AddFailedSecurityAttemptsAsync(failedSecurityAttempts);
        }

        public void ResetFailedLoginAttempts(UserProfile user)
        {
            // Reset the failed login attempts for the user
            if (user != null)
            {
                user.FailedLoginAttempt = 0;
                user.LastFailedLoginAttempt = null;
            }
        }

        public bool IsAccountLocked(UserProfile user)
        {
            // Check if the account is locked
            if (user != null && user.IsAccountLocked == null || user.IsAccountLocked == false ? false : true)
            {
                return true;
            }
            return false;
        }

        public void LockAccount(UserProfile user, int locktime)
        {
            // Lock the account and set the lock expiration time to 1 hour from now
            if (user != null)
            {
                user.IsAccountLocked = true;
            }
        }

        public void ResetAccountLock(UserProfile user)
        {
            // Reset the account lock
            if (user != null)
            {
                user.IsAccountLocked = false;
            }
        }

        public bool IsValidCredentials(UserProfile user, string password)
        {
            // Check if the username and password are valid
            if (user != null && user.Password == password)
            {
                return true;
            }
            return false;
        }

        public bool IsAccountDisabled(UserProfile user)
        {
            // Check if the account is disabled
            if (user.UserStatus == VarHelper.UserStatus.DISABLED.ToString())
            {
                return true;
            }
            return false;
        }

        public void DisableAccount(UserProfile user)
        {
            // disable the account
            if (user != null)
            {
                user.UserStatus = VarHelper.UserStatus.DISABLED.ToString();
            }
        }

        public void ResetDisabledAccount(UserProfile user)
        {
            // Reset the account
            if (user != null)
            {
                user.UserStatus = VarHelper.UserStatus.ENABLED.ToString();
            }
        }


        #region user location and device
        public IPAddress? GetRemoteHostIpAddressUsingXForwardedFor()
        {
            IPAddress? remoteIpAddress = null;
            var forwardedFor = _httpContext.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => s.Trim());

                foreach (var ip in ips)
                {
                    if (IPAddress.TryParse(ip, out var address) &&
                        address.AddressFamily is AddressFamily.InterNetwork
                         or AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = address;
                        break;
                    }
                }
            }

            return remoteIpAddress;
        }

        public IPAddress? GetRemoteHostIpAddressUsingXRealIp()
        {
            IPAddress? remoteIpAddress = null;
            var xRealIpExists = _httpContext.HttpContext.Request.Headers.TryGetValue("X-Real-IP", out var xRealIp);

            if (xRealIpExists)
            {
                if (!IPAddress.TryParse(xRealIp, out IPAddress? address))
                {
                    return remoteIpAddress;
                }

                var isValidIP = address.AddressFamily is AddressFamily.InterNetwork
                                 or AddressFamily.InterNetworkV6;

                if (isValidIP)
                {
                    remoteIpAddress = address;
                }

                return remoteIpAddress;
            }

            return remoteIpAddress;
        }

        public UserAgentInfo GetBrowserInfo()
        {
            string u_agent = _httpContext.HttpContext.Request.Headers["User-Agent"].ToString();
            string bname = "Unrecognized";
            string platform = "Unrecognized";
            string version = "?";

            if (u_agent.Contains("Linux", StringComparison.OrdinalIgnoreCase))
            {
                platform = "Linux";
            }
            else if (u_agent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase) || u_agent.Contains("Mac OS X", StringComparison.OrdinalIgnoreCase))
            {
                platform = "Mac";
            }
            else if (u_agent.Contains("Windows", StringComparison.OrdinalIgnoreCase) || u_agent.Contains("Win32", StringComparison.OrdinalIgnoreCase))
            {
                platform = "Windows";
            }
            else if (u_agent.Contains("Android", StringComparison.OrdinalIgnoreCase))
            {
                platform = "Android";
            }
            else if (u_agent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) || u_agent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
            {
                platform = "iOS";
            }
            else if (u_agent.Contains("Windows Phone", StringComparison.OrdinalIgnoreCase))
            {
                platform = "Windows Phone";
            }

            // Check for browser name (similar to the previous logic)

            string[] known = { "Version", "ub", "other" };
            string pattern = "(?<browser>" + string.Join("|", known) + ")[/ ]+(?<version>[0-9.|a-zA-Z.]*)";
            var matches = System.Text.RegularExpressions.Regex.Matches(u_agent, pattern);

            if (matches.Count > 0)
            {
                int index = u_agent.IndexOf("Version", StringComparison.OrdinalIgnoreCase);
                if (index >= 0 && index < matches.Count)
                {
                    version = matches[index].Groups["version"].Value;
                }
                else if (matches[0].Groups["version"] != null)
                {
                    version = matches[0].Groups["version"].Value;
                }
            }

            return new UserAgentInfo
            {
                UserAgent = u_agent,
                Name = bname,
                Version = version,
                Platform = platform,
                Pattern = pattern
            };
        }

       
        public async Task<LocationData?> GetIPInfo()
        {
            HttpContext context = _httpContext.HttpContext;
            string? ip = null;

            if (IPAddress.TryParse(GetRemoteHostIpAddressUsingXForwardedFor()?.ToString(), out IPAddress forwardedFor))
            {
                ip = forwardedFor.ToString();
            }
            else if (IPAddress.TryParse(GetRemoteHostIpAddressUsingXRealIp()?.ToString(), out IPAddress realIP))
            {
                ip = realIP.ToString();
            }
            else if (IPAddress.TryParse(context.Request.Headers["HTTP_CLIENT_IP"], out IPAddress clientIP))
            {
                ip = clientIP.ToString();
            }


            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress.ToString();
            }

            if (IPAddress.TryParse(ip, out IPAddress _))
            {
                using HttpClient client = new HttpClient();
                var response = await client.GetAsync($"http://ip-api.com/json/{ip}");

                var ipdat = await response.Content.ReadAsStringAsync();

                var data = JsonConvert.DeserializeObject<LocationData>(ipdat);

                return data;
            }

            return null;
        }


        public class UserAgentInfo
        {
            public string UserAgent { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public string Platform { get; set; }
            public string Pattern { get; set; }
        }

        public class LocationData
        {
            public string status { get; set; }
            public string country { get; set; }
            public string countryCode { get; set; }
            public string region { get; set; }
            public string regionName { get; set; }
            public string city { get; set; }
            public string zip { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
            public string timezone { get; set; }
            public string isp { get; set; }
            public string org { get; set; }
            public string @as { get; set; } // @as is used as it's a reserved keyword in C#
            public string query { get; set; }
        }
        #endregion
    }
}
