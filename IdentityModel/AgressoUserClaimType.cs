using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agresso.Foundation;

namespace U4A.ClaimsAuthenticator.IdentityModel
{
    /// <summary>
    /// Enumeration of user inforamtion attributes.
    /// </summary>
    public static class AgressoUserClaimType
    {

        /// <summary>
        /// User id.
        /// </summary>
        public const string UserId = "user_id";
        /// <summary>
        /// User name.
        /// </summary>
        public const string UserName = "user_name";
        /// <summary>
        /// Domain user.
        /// </summary>
        public const string DomainUser = "domain_user";
        ///// <summary>
        ///// Email of the user. Not supported right now.
        ///// </summary>
        //Email = 4,

        public static IEnumerable<KeyValuePair<string, string>> GetClaimTypes()
        {
            var userClaimsList = new List<KeyValuePair<string, string>>();
            userClaimsList.Add(new KeyValuePair<string, string>(UserName, AppContext.Titles.GetTitle(60652, "User name")));
            userClaimsList.Add(new KeyValuePair<string, string>(UserId, AppContext.Titles.GetTitle(25815, "User ID")));
            userClaimsList.Add(new KeyValuePair<string, string>(DomainUser, AppContext.Titles.GetTitle(44502, "Domain user")));

            return userClaimsList;
        }
    }
}
