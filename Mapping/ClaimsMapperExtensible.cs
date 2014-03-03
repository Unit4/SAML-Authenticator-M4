using System;
using System.Collections.Generic;
using Agresso.Foundation.Extension.NamedEvents;
using Microsoft.IdentityModel.Claims;
using U4A.ClaimsAuthenticator.IdentityModel;

namespace U4A.ClaimsAuthenticator.Mapping
{
    /// <summary>
    /// Fires named events used to customize claims based authenticator related user mapping.
    /// </summary>
    public interface IClaimsMapperExtensible
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        IEnumerable<AgressoUserClaim> GetUserClaims(IEnumerable<Claim> claims);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        string GetClientClaim(IEnumerable<Claim> claims);
    }

    /// <summary>
    /// Fires a NamedEvent to allow ACT to customise the user mapping and wraps the result into a list of user claims.
    /// </summary>
    internal class ClaimsMapperExtensible : DefaultMapper, IClaimsMapperExtensible
    {
        public override IEnumerable<AgressoUserClaim> GetUserClaims(IEnumerable<Claim> claims)
        {
            INamedEventResult userMappingEventResult = NamedEventRaiser.Raise("CLAIMS_AUTHENTICATION", "USER_MAPPING", "claims", claims);
            if (userMappingEventResult.NamedEventWasRaised())
                return GetUserClaimFromEvent(userMappingEventResult);

            return base.GetUserClaims(claims);

        }

        private IEnumerable<AgressoUserClaim> GetUserClaimFromEvent(INamedEventResult userMappingEventResult)
        {
            // event was fired and cancel was set to true -> the result should contain a valid user id
            var userId = userMappingEventResult["userId"] as string;
            var userName = userMappingEventResult["userName"] as string;
            var domainInfo = userMappingEventResult["domainInfo"] as string;


            // build mapped claims list
            if (!String.IsNullOrEmpty(userId))
            {
                yield return new AgressoUserClaim() { AgressoUserClaimType = AgressoUserClaimType.UserId, AgressoUserClaimValue = userId };
            }
            if (!String.IsNullOrEmpty(userName))
            {
                yield return new AgressoUserClaim() { AgressoUserClaimType = AgressoUserClaimType.UserName, AgressoUserClaimValue = userName };
            }
            if (!String.IsNullOrEmpty(domainInfo))
            {
                yield return new AgressoUserClaim() { AgressoUserClaimType = AgressoUserClaimType.DomainUser, AgressoUserClaimValue = domainInfo };
            }
        }

        public override string GetClientClaim(IEnumerable<Claim> claims)
        {

            var clientMappingEventResults = NamedEventRaiser.Raise("CLAIMS_AUTHENTICATION", "CLIENT_MAPPING", "claims", claims);
            if (clientMappingEventResults.NamedEventWasRaised())
                return clientMappingEventResults["client"] as string;

            return base.GetClientClaim(claims);
        }
    }

    internal static class INamedEventResultExtension
    {
        internal static bool NamedEventWasRaised(this INamedEventResult result)
        {
            if (!result.WasFired)
                return false;

            return result.Cancel.HasValue && result.Cancel.Value;
        }
    }
}
