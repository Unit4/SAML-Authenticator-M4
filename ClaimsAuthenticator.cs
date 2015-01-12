using Agresso.Foundation;
using Agresso.Foundation.Diagnostics;
using Agresso.Interface.Authentication;
using Agresso.Interface.Authentication.SingleStage;
using Agresso.Interface.CoreServices;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using U4A.ClaimsAuthenticator.IdentityModel;
using U4A.ClaimsAuthenticator.Mapping;
using System;

namespace U4A.ClaimsAuthenticator
{
    /// <summary>
    ///  Authenticator for claims based authentication (Federated identity)
    /// </summary>
    [Authenticator("U4A_CLAIMS", "Claims-based Authentication", false, PlatformConstant.Web)]
    public class ClaimsAuthenticator : Authenticator
    {
        private IUsers _users;
        private IUsers Users
        {
            get { return _users = _users ?? ObjectFactory.CreateInstance<IUsers>(); }
        }

        private IClaimsMapperExtensible CreateClaimsMappingEvent
        {
            get { return new ClaimsMapperExtensible(); }
        }

        /// <summary>
        /// Maps the claims to the coresponding agresso user and checks access rights.
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public override Response Authenticate(Credentials credentials)
        {
            var response = new Response();
            if (FederatedAuthentication.WSFederationAuthenticationModule == null)
            {
                response.DenyAccess(315197, "Claims-based (federated) authentication is not enabled. Please contact your system administrator");
                return response;
            }
            if (null == HttpContext.Current)
            {
                return response.DenyAccess();
            }

            var claimsPrincipal = HttpContext.Current.User as ClaimsPrincipal;
            
            if (claimsPrincipal == null) return response;

            var claimsIdentity = claimsPrincipal.Identity as ClaimsIdentity;

            if (claimsIdentity == null || (claimsPrincipal.Identity.IsAuthenticated && null != claimsIdentity.Claims && claimsIdentity.Claims.Any()))
                if (claimsIdentity != null) return GetUserAndCheckAccess(claimsIdentity);

            Log.Write("Federation: Claim Authenticator cannot find valid/authenticated Claims Principal. Can't proceed with user mapping.",
                TraceEventType.Warning,
                PriorityValue.AboveNormal,
                EventCategory.Security);

            return response.DenyAccess();
        }



        internal Response GetUserAndCheckAccess(ClaimsIdentity claimsIdentity)
        {
            IList<Claim> claims = claimsIdentity.Claims.ToList();
            LogClaims(claims);

            var response = new Response();

            if (claims != null)
            {
                IUserInfo userInfo = GetUserInfo(claims);

                if (userInfo != null)
                {
                    string claimClient;
                    var client = string.IsNullOrEmpty(claimClient = GetClientClaim(claims)) ? userInfo.DefaultClient : claimClient;
                    response = CheckAccess(userInfo, client);
                    if (response.Authenticated)
                    {
                        AddUserIdAndClientClaims(claimsIdentity, client, userInfo.UserId);
                    }
                }

                if (response.Authenticated) return response;

                response.DenyAccess();

                string identityDescription = IdentityDescription(claims);

                Log.Write(string.Format(
                    "Federation: Mapping resulted in no matching Agresso user. Enable information logs for details. Federated identity: {0}", identityDescription),
                    TraceEventType.Warning,
                    PriorityValue.AboveNormal,
                    EventCategory.Security, identityDescription);
            }
            else
            {
                response.DenyAccess();
            }

            return response;
        }

        private void AddUserIdAndClientClaims(ClaimsIdentity claimsIdentity, string client, string userId)
        {
            if (!claimsIdentity.Claims.Exists(new Predicate<Claim>(claim => claim.ClaimType == "http://agresso.services.com/schema/identity/claims/client")))
            {
                claimsIdentity.Claims.Add(new Claim("http://agresso.services.com/schema/identity/claims/client", string.IsNullOrEmpty(client) ? "" : client));
            }
            if (!claimsIdentity.Claims.Exists((new Predicate<Claim>(claim => claim.ClaimType == "http://agresso.services.com/schema/identity/claims/userid"))))
            {
                claimsIdentity.Claims.Add(new Claim("http://agresso.services.com/schema/identity/claims/userid", userId));

            }
        }

        private void LogClaims(IEnumerable<Claim> claims)
        {
            foreach (var claim in claims)
            {
                Log.Write(string.Format("Incomming claims: claim type is {0}, claim value is {1}", claim.ClaimType, claim.Value), TraceEventType.Verbose, PriorityValue.Low);
            }
        }

        private Response CheckAccess(IUserInfo userInfo, string client)
        {
            var response = new Response();
            if (Users.HasAccessToClient(userInfo.UserId, client))
            {
                response.GrantAccess(userInfo.UserId, client);
            }
            else
            {
                Log.Write("Federation: Federated identity could not be mapped to any Agresso user. Access to is denied.",
                    TraceEventType.Information,
                    PriorityValue.AboveNormal,
                    EventCategory.Security,
                    string.Format("User {0} has not access to client {1}", userInfo.UserId, client));
                response.DenyAccess();
            }
            return response;
        }

        private IUserInfo GetUserInfo(IList<Claim> claims)
        {
            var userInfo = Users.GetUserByClaim(claims);
            if (userInfo == null)
            {
                IEnumerable<AgressoUserClaim> userClaims = CreateClaimsMappingEvent.GetUserClaims(claims);
                return Users.GetUserByClaim(userClaims);
            }

            return userInfo;
        }

        private string GetClientClaim(IEnumerable<Claim> claims)
        {
            var client = CreateClaimsMappingEvent.GetClientClaim(claims);
            return client;
        }


        private string IdentityDescription(IList<Claim> claims)
        {
            const string unknown = "Unknown";

            if (claims == null)
                return unknown;

            Claim claim;
            if ((claim = claims.SingleOrDefault(c => ClaimTypes.Email.Equals(c.ClaimType))) != null)
                return claim.Value;
            if ((claim = claims.SingleOrDefault(c => ClaimTypes.NameIdentifier.Equals(c.ClaimType))) != null)
                return claim.Value;

            return unknown;
        }
    }

    internal static class ResponseExtensions
    {
        internal static Response GrantAccess(this Response response, string userId, string client)
        {
            response.GrantAccess(userId, client);
            return response;
        }

        internal static Response DenyAccess(this Response response)
        {
            response.DenyAccess(314435, "Unable to map Agresso user. Please contact your system administrator.");
            return response;
        }
    }

    /// <summary>
    /// Extension method for claims authenticator
    /// </summary>
    internal static class IUsersExtension
    {
        /// <summary>
        /// </summary>
        /// <param name="userId">Information about the user to check access to</param>
        /// <param name="users">The user manager to checks if a user has access to client</param>
        /// <param name="client">Which client to check</param>
        /// <returns>true if user has access, false if user does not has acccess</returns>
        internal static bool HasAccessToClient(this IUsers users, string userId, string client)
        {
            if (users.AllowedAccessClient(userId, client))
            {
                return true;
            }
            else
            {
                Log.Write(string.Format("Federation: Possible authenticaiton failure. User {0} does  not have access to client {1}",
                  userId,
                  string.IsNullOrEmpty(client) ? "NO DEFAULT CLIENT" : client),
                  TraceEventType.Information,
                  PriorityValue.AboveNormal,
                  EventCategory.Security,
                  userId, client);

                return false;
            }
        }



    }
}
