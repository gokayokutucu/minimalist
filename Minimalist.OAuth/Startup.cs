using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minimalist.OAuth.Configuration;

namespace Minimalist.OAuth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddIdentityServer()
                    //.AddDeveloperSigningCredential() //Temporary sign in credential to test the identity server
                    .AddSigningCredential(new X509Certificate2(@"/Users/gokayokutucu/Projects/Minimalist/minimalist.pfx" , "password"))
                    .AddTestUsers(InMemoryConfiguration.Users())
                    .AddInMemoryClients(InMemoryConfiguration.Clients())
                    .AddInMemoryIdentityResources(InMemoryConfiguration.IdentityResources())
                    .AddInMemoryApiResources(InMemoryConfiguration.ApiResources());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //To get information out from console
            loggerFactory.AddConsole();

            //Diagnostic page for problems
            app.UseDeveloperExceptionPage();

            //To allow requesting a token
            app.UseIdentityServer();
           
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
