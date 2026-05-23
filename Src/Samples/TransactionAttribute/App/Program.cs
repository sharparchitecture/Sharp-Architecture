namespace TransactionAttribute.WebApi
{
    using Autofac.Extensions.DependencyInjection;
    using Serilog;
    using Serilog.Events;
    using Serilog.Exceptions;
    using Serilog.Formatting.Json;
    using Serilog.Sinks.SystemConsole.Themes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [UsedImplicitly]
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

        public static IHostBuilder CreateHostBuilder(Action<IWebHostBuilder>? webHostOverrides = null)
            => new HostBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
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
                            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                            ;
                    else
                        loggerConfiguration
                            .WriteTo.Console(new JsonFormatter(), LogEventLevel.Information)
                            ;
#if DEBUG
                    loggerConfiguration.WriteTo.Seq("http://localhost:5341");
#endif
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, appConfig) =>
                {
                    appConfig.AddJsonFile("appsettings.json", false, false)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, false);
                    appConfig.AddEnvironmentVariables();
                })
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseKestrel();
                    webHost.UseStartup<Startup>();
                    webHostOverrides?.Invoke(webHost);
                });
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
