// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace Marvin.IDP
{
    public static class Config 
    {
        // a set of Identity related resource , these maps to claim of the User
        //like firstName and lastName  
        //currently only OpenId has been aded  which maps to a suncalim also
        //known as the User.s Identifier
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                // our IDP is configured to support the OpenId and Profile Scope
                new IdentityResources.OpenId(),

                // this tells us we will ve dealing with user profile info like
                //Name, Suranme et.c
                new IdentityResources.Profile(),
                // allowing our Idp to support a new profile in this case the address profile 
                //this becomes a new resourcr which maps to the  Addreess claim  
                new IdentityResources.Address()
            };


        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            { };

        //public static IEnumerable<ApiResource> ApiScopes =>
        //    new ApiResource[]
        //    { };

        // THis property basicaly allow us to configure the Client that will relate to our IDp 
        public static IEnumerable<Client> Clients =>
            new Client[] 
            { 
              new Client
              {
                  // the client name we be the name to appear on the Consent Screen 
                   ClientName =  "Image Gallery",
                   ClientId = "imagegalleryclient",
                   RequirePkce = true,
                   // since this grant type works on redirecton URl, we need to Put that URl IN here
                   AllowedGrantTypes = GrantTypes.Code,
                   RedirectUris = new List<string>()
                   {
                       // ths is the Host address of our Web Mvc pplication
                       // Not the signin-oidc that is something we can configure at the Level of our Web Client the signin-oidc is the default value
                       "https://localhost:44389/signin-oidc"
                   },

                   // we need to tell the Logout Page, it is a default provided for us by OIDC. despite being default always specify as well
                   PostLogoutRedirectUris = new List<string>()
                   {
                         "https://localhost:44389/signout-callback-oidc"
                   },
                   // we need to configure which scopes are allowed to be requested by thus client
                   AllowedScopes =
                  {
                      // we allow our client acces to the two scopes we configured above
                       IdentityServerConstants.StandardScopes.OpenId,
                       IdentityServerConstants.StandardScopes.Profile,
                       IdentityServerConstants.StandardScopes.Address
                  },
                   // we need to configure seccrets use for Client Authentication
                   ClientSecrets =
                  {
                      // this secrets allowa the Client appllication to call the Token Endpoint
                      new Secret("secret".Sha256())
                  }
              }
            };
    }
    // LETs configure our IdentityServer to Log in with the Authorization CodeFlow 
}

