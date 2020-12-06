namespace TransactionAttribute.WebApi
{

#if NETCOREAPP3_1 || NET5_0
    using Microsoft.Extensions.Hosting;
    using Microsoft.AspNetCore.Hosting;
#else
    using Microsoft.AspNetCore.Hosting;
#endif
    using System;
    using System.IO;
    using Autofac.Extensions.DependencyInjection;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using Serilog.Events;
    using Serilog.Exceptions;
    using Serilog.Formatting.Json;
    using Serilog.Sinks.SystemConsole.Themes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [UsedImplicitly]
    public class Program
    {
        public static int Main([NotNull] string[] args)
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

        [NotNull]
        public static IWebHostBuilder CreateHostBuilder()
        {
            return new WebHostBuilder()
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    var env = hostingContext.HostingEnvironment;
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
