using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using IdentityModel;

namespace Minimalist.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddTransient<CookieEventHandler>();
            services.AddSingleton<LogoutSessionManager>();

            //turned off the JWT claim type mapping to allow well-known claims(e.g. ‘sub’ and ‘idp’) 
            //to flow through unmolested
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            //add authentication schemes
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; //returns "Cookies" value as a string
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;//returns "OpenIdConnect" value as a string
            })
            //Add cookies with authentication scheme name
           .AddCookie("Cookies", options =>
           {
               options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

               options.EventsType = typeof(CookieEventHandler);
           })
            //Add OpenIDConnect with authentication scheme name
            .AddOpenIdConnect("OpenIdConnect", options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //authority =  "http://localhost:5000/"
                options.Authority = Configuration.GetValue<string>("Authentication:Authority");
                //client_id = "minimalist_code"
                options.ClientId = Configuration.GetValue<string>("Authentication:ClientId");
                //client_secret = "secret"
                options.ClientSecret = Configuration.GetValue<string>("Authentication:ClientSecret");
                options.RequireHttpsMetadata = false;
                //Add scopes
                options.Scope.Add("minimalist");//minimalist API
                options.Scope.Add("offline_access");
                options.Scope.Add("email");
                //Save all tokens receiving back from the authorization server
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.ResponseType = "code id_token";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role,
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //to ensure the authentication services execute on each request
            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
