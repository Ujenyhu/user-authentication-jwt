using userauthjwt.Responses;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace userauthjwt.Helpers
{
    public class TokenHelper
    {
        public const string Issuer = "https://localhost:44364/";
        public const string Audience = "https://localhost:44364/";
        public const string Secret = "SjFmZrhHCZO9ayOno+sBp5ADpya3u72kqWwA8enBDG6Z4WdcAD5KZd5oIc/EruPxRzl+74VN/ytJPXg9ya53dg==";


        public static string GenerateToken(SignInResponse user)
        {
            var claimsIdentity = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserId),
                new Claim(ClaimTypes.Email, user.EmailAddress),
            };
            return GenerateAccessToken(claimsIdentity);
        }

        public static string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = Convert.FromBase64String(Secret);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var tokenOptions = new JwtSecurityToken(
               issuer: Issuer,
               audience: Audience,
               claims: claims,
               expires: DateTime.Now.AddDays(1),
               signingCredentials: signingCredentials
           );

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }


        public static ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(Secret))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
