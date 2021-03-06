﻿using System;
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
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.Reflection;

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
            var connectionString = Configuration.GetConnectionString("Minimalist.OAuth");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddMvc();
            services.AddIdentityServer()
                    //Temporary sign in credential to test the identity server
                    //.AddDeveloperSigningCredential() 
                    //We use AddSigningCredential() method instead of AddDeveloperSigningCredential() method for production.
                    .AddSigningCredential(new X509Certificate2(@"/Users/gokayokutucu/Projects/Minimalist/minimalist.pfx", "password"))
                    .AddTestUsers(InMemoryConfiguration.GetUsers())
                    // this adds the configuration data from DB (clients, resources)
                    .AddConfigurationStore(options =>
                     {
                         options.ConfigureDbContext = builder =>
                            builder.UseNpgsql(connectionString,
                                 sql => sql.MigrationsAssembly(migrationsAssembly));
                     })
                    // this adds the operational data from DB (codes, tokens, consents)
                    .AddOperationalStore(options =>
                    {
                        options.ConfigureDbContext = builder =>
                            builder.UseNpgsql(connectionString,
                                sql => sql.MigrationsAssembly(migrationsAssembly));

                        // this enables automatic token cleanup. this is optional.
                        options.EnableTokenCleanup = true;
                        options.TokenCleanupInterval = 30;
                    });
                    //.AddInMemoryClients(InMemoryConfiguration.Clients())
                    //.AddInMemoryIdentityResources(InMemoryConfiguration.IdentityResources())
                    //.AddInMemoryApiResources(InMemoryConfiguration.ApiResources());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //This will do the initial DB population
            InitializeDatabase(app);

            //To get information out from console
            loggerFactory.AddConsole();

            //Diagnostic page for problems
            app.UseDeveloperExceptionPage();

            //To allow requesting a token
            app.UseIdentityServer();
           
            app.UseStaticFiles();
            //To reach the OIDC - Go to http://localhost:5000/ and click the link on the page for discovery document 
            app.UseMvcWithDefaultRoute();
        }

        private static void InitializeDatabase(IApplicationBuilder app){
            using (var serviceScope = app.ApplicationServices
                                     .GetService<IServiceScopeFactory>()
                                     .CreateScope())
            {
                PerformMigrations(serviceScope);
                Seed(serviceScope);
            }
        }

        private static void PerformMigrations(IServiceScope serviceScope)
        {
            serviceScope.ServiceProvider
                        .GetRequiredService<PersistedGrantDbContext>()
                        .Database
                        .Migrate();
            serviceScope.ServiceProvider
                        .GetRequiredService<ConfigurationDbContext>()
                        .Database
                        .Migrate();
        }

        private static void Seed(IServiceScope serviceScope)
        {
            var context = serviceScope
                       .ServiceProvider
                       .GetRequiredService<ConfigurationDbContext>();

            if (!context.Clients.Any())
            {
                foreach (var client in InMemoryConfiguration.GetClients())
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in InMemoryConfiguration.GetIdentityResources())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in InMemoryConfiguration.GetApiResources())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
        }
    }
}
