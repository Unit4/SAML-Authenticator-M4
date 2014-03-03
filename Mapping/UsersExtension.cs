using Agresso.Foundation.Diagnostics;
using Agresso.Interface.CoreServices;
using Microsoft.IdentityModel.Claims;
using System.Collections.Generic;
using System.Linq;
using U4A.ClaimsAuthenticator.IdentityModel;

namespace U4A.ClaimsAuthenticator.Mapping
{
    public static class UserInfoClaimExtension
    {

        public static IUserInfo GetUserByClaim(this IUsers users, IList<Claim> claims)
        {
            IUserInfo userInfo = GetById(users, claims);
            if (userInfo != null)
                return userInfo;

            userInfo = GetByName(users, claims);
            if (userInfo != null)
                return userInfo;

            userInfo = GetByDomainUser(users, claims);
            if (userInfo != null)
                return userInfo;

            return userInfo;
        }

        private static IUserInfo GetByName(IUsers users, IList<Claim> claims)
        {
            IUserInfo userInfo = null;
            var userIdClaims = claims.Where(claim => claim.ClaimType == ClaimTypesRequested.UserName);
            if (userIdClaims.Count() > 0)
            {
                foreach (var claim in userIdClaims)
                {
                    userInfo = users.GetByName(claim.Value);
                    if (userInfo != null)
                        return userInfo;
                    Log.Write(string.Format("Could not get user from claim type {0} with value {1}", claim.ClaimType, claim.Value));
                }
            }

            return null;
        }

        private static IUserInfo GetByDomainUser(IUsers users, IList<Claim> claims)
        {
            IUserInfo userInfo = null;
            var userIdClaims = claims.Where(claim => claim.ClaimType == ClaimTypesRequested.DomainUser);
            if (userIdClaims.Count() > 0)
            {
                foreach (var claim in userIdClaims)
                {
                    userInfo = users.GetByDomainUser(claim.Value);
                    if (userInfo != null)
                        return userInfo;
                    Log.Write(string.Format("Could not get user from claim type {0} with value {1}", claim.ClaimType, claim.Value));
                }
            }

            return null;
        }

        private static IUserInfo GetById(IUsers users, IList<Claim> claims)
        {
            IUserInfo userInfo = null;
            var userIdClaims = claims.Where(claim => claim.ClaimType == ClaimTypesRequested.UserId);
            if (userIdClaims.Count() > 0)
            {
                foreach (var claim in userIdClaims)
                {
                    userInfo = users.GetById(claim.Value);
                    if (userInfo != null)
                        return userInfo;
                    Log.Write(string.Format("Could not get user from claim type {0} with value {1}", claim.ClaimType, claim.Value));
                }
            }

            return null;
        }

        public static IUserInfo GetUserByClaim(this IUsers users, IEnumerable<AgressoUserClaim> userClaims)
        {
            if (userClaims != null)
                return GetUserInfo(userClaims, users);
            return null;
        }

        private static IUserInfo GetUserInfo(IEnumerable<AgressoUserClaim> userClaims, IUsers users)
        {
            IUserInfo userInfo = null;
            foreach (var mappedClaim in userClaims.ToList())
            {
                userInfo = GetUserByClaimType(users, mappedClaim.AgressoUserClaimValue, mappedClaim.AgressoUserClaimType);
                if (userInfo != null)
                {
                    break;
                }

            }
            return userInfo;
        }

        private static IUserInfo GetUserByClaimType(IUsers users, string claimValue, string userKey)
        {
            switch (userKey)
            {
                case AgressoUserClaimType.DomainUser:
                    return users.GetByDomainUser(claimValue);
                case AgressoUserClaimType.UserId:
                    return users.GetById(claimValue);
                case AgressoUserClaimType.UserName:
                    return users.GetByName(claimValue);
            }

            return null;
        }


    }
}
