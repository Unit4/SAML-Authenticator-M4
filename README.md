SAML-Authenticator-M4
=====================

UNIT4 Agresso Milestone 4 Update 1 Custom claims based authenticator supporting SAML 2.0 tokens over the WS-Federation protocol.

DISCLAIMER
----------
This authenticator is for test purposes only. It is unsupported and used at own risk. It will
only work for Agresso Business World Milestone 4 Update 1.

Federated identity authentication is delivered as standard functionality with Milestone 5. This 
authenticator is a subset of the Milestone 5 delivery and contains the authentication part
only. Limitations compared to the Milestone 5 functionality:

* Configuration in Agresso Management Console (AMC)
* Flexible claims mapping setup
* Single sign-out
* FederationMetadata XML for easy setup of Relying Party

R&D is very happy to receive feedback.


INTRODUCTION
------------

This custom authenticator enables federated identity authentication for Agresso Web
and Agresso Web (classic). The functionality is based on Windows Identity Foundation (WIF) 
which means that we support whatever WIF supports (this means no SAML-P support, only WS-Federation).

The SAML 2.0 token sent back from the Security Token Service (STS) contains claims which are used 
to identify an Agresso user. The mapping logic of this authenticator will lookup users by the 
"Domain user" (User master file / `TAG064` -> Security -> Single sign-on -> Domain user) field.

DEPENDENCIES
------------

* Agresso Business World Milestone 4 Update 1
* Windows Identity Foundation 3.5

INSTALLATION
------------

Installation is done in the Agresso Desktop client (aka Smart Client).

Install this custom authenticator through the Authenticators screen `TAG107`.
- Add authenticator and locate the U4A.ClaimsAuthenticator.dll
- "Do you want to set it up in ACT too?". Yes, this is a good idea for flexible deployment.
- If you chose to set up the authenticator in ACT. Go to ACT Setup `TAG001`
    1. Load from location on disk
        - Load from bin-directory of the web application, no changes needed.
        - Load from other location, click the authenticator and enter Path in details section
    2. Load from database
        - Click upload and locate the U4A.ClaimsAuthenticator.dll

CONFIGURATION
-------------

**Step 1 - Configure web applications**

Edit web.config of Agresso Web and/or Agresso Web (classic). Inspect the template 
(templates/selfservice-template.web.config) file in this package.

The template contains comments like "<!-- FED add as is FED -->" in the beginning and the end of 
elements that needs to be changed compared to the existing web.config of your application. The 
comment between "FED" and "FED" indicates the type of change. "Add as is" means copy/paste, for
example.

**Step 2 - Configure STS**

Since Agresso Web do not have any FederationMetadata.xml (Milestone 5 only), the STS must be manually
configured with the Agresso Web as a Relying Party. Also setup claims according to mapping 
configuration (see Step 3).

**Step 3 - Mapping**

Each Agresso user must be configured with the federated identity claim used for mapping. The E-Mail
claim can be used, for example. In that case, add E-Mail accordingly for each user to the "Domain user" 
field in User master file.

KNOWN LIMITATIONS
-----------------

**Continue button to login**

Milestone 4 Update 1 needs the STS to redirect back to the /System/Login.aspx page to avoid the "Continue"-button 
click. Since ADFS does not honor the wreply parameter, always link to /System/Login.aspx. This will 
make the wtrealm parameter and the redirect leads to a direct login (without the need of clicking the "Continue"-button).

Windows Azure Active Directory (WAAD) does honor wreply, so use this attribute in the wsFederation element
in the Identity Model configuration of web.config.
