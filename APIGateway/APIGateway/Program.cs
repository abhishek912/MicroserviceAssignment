using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Administration;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Eureka;
using Ocelot.Provider.Polly;


namespace APIGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                //.UseUrls("http://localhost:49905")
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true,
                            true)
                        .AddJsonFile("OcelotConfiguration/OcelotConfig.json", false, false)
                        //.AddJsonFile("Ocelotloadbalancer.json", false, false)
                        //.AddJsonFile("OcelotFaultTolerance.json", false, false)
                        //.AddJsonFile("OcelotAuth.json", false, false)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices(services =>
                {
                    var authenticationProviderKey = "IdentityApiKey";

                    services.AddAuthentication()
                             .AddJwtBearer(authenticationProviderKey, x =>
                             {
                                 x.Authority = "https://localhost:44310"; // IDENTITY SERVER URL
                                                                          //x.RequireHttpsMetadata = false;
                                 x.TokenValidationParameters = new TokenValidationParameters
                                 {
                                     ValidateAudience = false
                                 };
                             });

                    services.AddCors(options =>
                    {
                        options.AddPolicy("CorsPolicy",
                                           builder => builder.SetIsOriginAllowed(hostName => true)
                                            .AllowAnyMethod()
                                            .AllowAnyHeader()
                                            .AllowCredentials()
                                          );
                    });

                    services.AddOcelot()
                    //.AddTransientDefinedAggregator<FakeDefinedAggregator>() //for aggregates
                    .AddEureka()
                    .AddPolly()
                    .AddAdministration("/administration", "secret");
                })
                .Configure(a =>
                {
                    a.UseOcelot().Wait();
                    a.UseDeveloperExceptionPage();
                    a.UseCors("CorsPolicy");

                })
                .Build();
        }
    }
}
