using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using InstantMessagingServerApp.Repositories;

namespace InstantMessagingServerApp.Filters
{
    public class JwtAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        public string Realm { get; set; }
        public bool AllowMultiple => false;

        public async Task AuthenticateAsync(
            HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;
            var authorization = request.Headers.Authorization;
            if (authorization == null || authorization.Scheme != "Bearer")
                return;
            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing Jwt Token", request);
                return;
            }
            var token = authorization.Parameter;
            var principal = await AuthenticateJwtToken(token);
            if (principal == null)
                context.ErrorResult = new AuthenticationFailureResult("Invalid token", request);
            else
                context.Principal = principal;
        }

        private static bool ValidateToken(string token, 
        	out string username, out string passwordHash)
        {
            username = null;
            passwordHash = null;
            try
            {
                var principle = Services.JwtManager.GetPrincipal(token);
                var identity = principle.Identity as ClaimsIdentity;
                if (identity == null)
                    return false;
                if (!identity.IsAuthenticated)
                    return false;
                var usernameClaim = identity.FindFirst(ClaimTypes.Name);
                var passwordClaim = identity.FindFirst(ClaimTypes.Hash);
                username = usernameClaim?.Value;
                passwordHash = passwordClaim?.Value;
                if (string.IsNullOrEmpty(username) 
                    || string.IsNullOrEmpty(passwordHash))
                    return false;
                var unitOfWork = new UnitOfWork();
                var user = unitOfWork.UserRepository.GetByName(username);
                return (user != null && user.PasswordHash == passwordHash);
            }
            catch (NullReferenceException)
            {
                return false;
            }  
        }

        protected Task<IPrincipal> AuthenticateJwtToken(string token)
        {
            string username;
            string passwordHash;
            if (!ValidateToken(token, out username, out passwordHash)) 
            	return Task.FromResult<IPrincipal>(null);
            // based on username to get more information from database in order to build local identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Hash, passwordHash)
                // Add more claims if needed: Roles, ...
            };
            var identity = new ClaimsIdentity(claims, "Jwt");
            IPrincipal user = new ClaimsPrincipal(identity);
            return Task.FromResult(user);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context,
            CancellationToken cancellationToken)
        {
            Challenge(context);
            return Task.FromResult(0);
        }

        private void Challenge(HttpAuthenticationChallengeContext context)
        {
            string parameter = null;
            if (!string.IsNullOrEmpty(Realm))
                parameter = "realm=\"" + Realm + "\"";
            context.ChallengeWith("Bearer", parameter);
        }
    }
}