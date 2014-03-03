using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U4A.ClaimsAuthenticator.IdentityModel
{
    public static class ClaimTypesRequested
    {
        public const string UserId = "http://agresso.services.com/schema/identity/claims/requested/userid";
        public const string UserName = "http://agresso.services.com/schema/identity/claims/requested/username";
        public const string DomainUser = "http://agresso.services.com/schema/identity/claims/requested/domainuser";
        public const string Client = "http://agresso.services.com/schema/identity/claims/requested/client";
    }
}
