using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U4A.ClaimsAuthenticator.IdentityModel
{
    /// <summary>
    /// User information attribute and value used to lookup mapped user.
    /// </summary>
    public struct AgressoUserClaim
    {
        /// <summary>
        /// User information attribute used to select user
        /// </summary>
        public string AgressoUserClaimType { get; set; }

        /// <summary>
        /// Value of user information attribute
        /// </summary>
        public string AgressoUserClaimValue { get; set; }
    }
}
