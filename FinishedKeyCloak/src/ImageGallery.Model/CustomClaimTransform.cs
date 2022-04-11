using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ImageGallery.Model
{
    /// <summary>
    /// This kind of transform does not store the claims into the token and it is
    /// called multiple times
    /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/claims?view=aspnetcore-6.0
    /// </summary>
    public class CustomClaimTransform : IClaimsTransformation
    {
        private readonly IOptionsMonitor<KeyCloakConfiguration> _keyCloakConfiguratino;

        /// <summary>
        /// Can depend by stuff registered in IoC
        /// </summary>
        public CustomClaimTransform(IOptionsMonitor<KeyCloakConfiguration> keyCloakConfiguratino)
        {
            _keyCloakConfiguratino = keyCloakConfiguratino;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            ClaimsIdentity claimsIdentity = new ClaimsIdentity();
            var claimType = "InternalUserId";
            if (!principal.HasClaim(claim => claim.Type == claimType))
            {
                var idpUserSub = principal.Claims.Single(c => c.Type == "sub").Value;

                //Put here the logic to find your local user information from the external sub
                var internalUserId = "User_12";

                claimsIdentity.AddClaim(new Claim(claimType, internalUserId));
            }

            principal.AddIdentity(claimsIdentity);
            return Task.FromResult(principal);
        }
    }
}
