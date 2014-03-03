using Microsoft.IdentityModel.Claims;
using System.Collections.Generic;
using System.Linq;
using U4A.ClaimsAuthenticator.IdentityModel;

namespace U4A.ClaimsAuthenticator.Mapping
{
    public class DefaultMapper
    {
        private readonly List<string> _fallbackUserInfo = new List<string> { AgressoUserClaimType.DomainUser };

        /// <summary>
        /// Maps System claims to Agresso claims.
        /// </summary>
        /// <param name="claims"></param>
        /// <returns>a list of ClaimType / Claim value pairs</returns>
        public virtual IEnumerable<AgressoUserClaim> GetUserClaims(IEnumerable<Claim> claims)
        {
            return from claim in claims
                   from userInfoAttribute in _fallbackUserInfo
                   select new AgressoUserClaim() { AgressoUserClaimType = userInfoAttribute, AgressoUserClaimValue = claim.Value };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public virtual string GetClientClaim(IEnumerable<Claim> claims)
        {
            return string.Empty;
        }
    }
}
