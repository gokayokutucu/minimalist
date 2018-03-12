using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace Minimalist.OAuth.Configuration
{
    public class InMemoryConfiguration
    {
        public static IEnumerable<ApiResource> ApiResources(){
            return new []
            {
                new ApiResource("minimalist", "Minimalist")
                {
                    //Add email claim to the token payload
                    UserClaims = { "email"},
                    ApiSecrets = { new Secret("secret".Sha256()) }
                }
            };
        }

        public static IEnumerable<IdentityResource> IdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
            };
        }

        public static IEnumerable<Client> Clients()
        {
            return new[]
            {
                new Client{
                    ClientId = "minimalist",
                    ClientSecrets = new []{ new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials, //Directly create a username and password for a token
                    AllowedScopes = new []{"minimalist"}
                },
                new Client{
                    ClientId = "minimalist_implicit",
                    ClientSecrets = new []{ new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.Implicit, //Directly create a username and password for a token
                    AllowedScopes = new []{
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "minimalist"
                    },
                    //AllowAccessTokensViaBrowser = true,
                    RedirectUris = new []{"http://localhost:5001/signin-oidc"},
                    PostLogoutRedirectUris = new []{"http://localhost:5001/signout-callback-oidc"},
                },
                new Client{
                    ClientId = "minimalist_code",
                    ClientSecrets = new []{ new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.Hybrid, //Directly create a username and password for a token
                    AllowedScopes = new []{
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "minimalist"
                    },
                    //OAuth 2.0 grant that native apps use in order to access an API
                    //RequirePkce = true,
                    //AccessTokenLifetime = 70,
                    //AllowAccessTokensViaBrowser = true,
                    //Allow to communicate between user and API without website
                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RedirectUris = new []{"http://localhost:5001/signin-oidc"},
                    PostLogoutRedirectUris = new []{"http://localhost:5001/signout-callback-oidc"},
                }
            };
        }

        public static List<TestUser> Users()
        {
            return new List<TestUser>()
            {
                new TestUser{
                    SubjectId = "1",
                    Username = "okutucugokay@gmail.com",
                    Password = "mypassword",
                    Claims = new []{ new Claim("email","okutucugokay@gmail.com")}
                }
            };
        }
    }
}
