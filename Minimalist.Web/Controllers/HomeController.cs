using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Minimalist.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Minimalist.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Authorize]
        public async Task<IActionResult> DoSomething()
        {
            await RefreshTokenAsync();
            ViewData["Message"] = "Tokens renewed!";

            return View();
        }

        public async Task RefreshTokenAsync(){
            //To retrieve OpenID Connect discovery documents and key sets
            var discoveryClient = new DiscoveryClient("http://localhost:5002");
            var authorizationServerInformation = await discoveryClient.GetAsync();
            var client = new TokenClient(authorizationServerInformation.TokenEndpoint, "minimalist_code", "secret");
            //Set tokens
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            var tokenResponse = await client.RequestRefreshTokenAsync(refreshToken);
            var identityToken = await HttpContext.GetTokenAsync("id_token");
            //Set expire time
            var expiresIn = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResponse.ExpiresIn);

            var tokens = new[]{
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.IdToken,
                    Value = identityToken
                },
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.AccessToken,
                    Value = tokenResponse.AccessToken
                },
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.RefreshToken,
                    Value = tokenResponse.RefreshToken
                },
                new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.ExpiresIn,
                    Value = expiresIn.ToString("o", CultureInfo.InvariantCulture)
                }
            };

            //Set tokens to the cookie
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            result.Properties.StoreTokens(tokens);
            await HttpContext.SignInAsync(result.Principal, result.Properties);
        }

        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("OpenIdConnect");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
