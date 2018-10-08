using System;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace SharpArch.WebApi
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                CreateHostBuilder()
                    .Build()
                    .Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Logger?.ForContext<Program>().Fatal(ex, "Unhandled exception");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateHostBuilder()
        {
            return new WebHostBuilder()
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    IHostingEnvironment env = hostingContext.HostingEnvironment;
                    loggerConfiguration
                        .Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        ;

                    if (env.IsDevelopment())
                        loggerConfiguration
                            .MinimumLevel.Verbose()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                            .MinimumLevel.Override("System", LogEventLevel.Information)
                            .WriteTo.Console( theme: AnsiConsoleTheme.Code)
                            ;
                    else
                        loggerConfiguration
                            .WriteTo.Console(new JsonFormatter(), LogEventLevel.Information)
                            ;
                })
                .UseKestrel()
                .ConfigureServices(services => services.AddAutofac())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, appConfig) =>
                {
                    var envName = hostingContext.HostingEnvironment.EnvironmentName;
                    appConfig.AddJsonFile("appsettings.json", false, false)
                        .AddJsonFile($"appsettings.{envName}.json", true, false);
                    appConfig.AddEnvironmentVariables();
                })
                .UseIISIntegration()
                .UseStartup<Startup>();
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
