using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace InstantMessagingServerApp.Services
{
    public class JwtManager
    {
        private static readonly string Secret =
            System.Configuration.ConfigurationManager.AppSettings["as:AudienceSecret"];

        public static string GenerateToken(string username, string passwordHash, int expireHours = 12)
        {
            var symmetricKey = Convert.FromBase64String(Secret);
            var tokenHandler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Hash, passwordHash)
                }),
                Expires = now.AddHours(Convert.ToInt32(expireHours)),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);
            return token;
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                if (jwtToken == null) return null;
                var symmetricKey = Convert.FromBase64String(Secret);
                var validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                };
                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, 
                	validationParameters, out securityToken);
                return principal;
            }
            catch (Exception) { return null; }
        }

        public bool ValidateUser(string userName, string passwordHash, string jsonWebToken)
        {
            var principle = GetPrincipal(jsonWebToken);
            var identity = principle.Identity as ClaimsIdentity;
            if (identity == null) return false;
            var usernNameClaim = identity.FindFirst(ClaimTypes.Name);
            var passwordHashClaim = identity.FindFirst(ClaimTypes.Hash);
            var userNameInJwt = usernNameClaim?.Value;
            var passwordHashInJwt = passwordHashClaim?.Value;
            return (userNameInJwt == userName && passwordHashInJwt == passwordHash);
        }

        public string GetUserNameFromToken(string jsonWebToken)
        {
            var principle = GetPrincipal(jsonWebToken);
            var identity = principle.Identity as ClaimsIdentity;
            var usernNameClaim = identity?.FindFirst(ClaimTypes.Name);
            return usernNameClaim?.Value;
        }

    }
}