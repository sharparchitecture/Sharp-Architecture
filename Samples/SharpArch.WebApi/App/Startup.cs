namespace SharpArch.WebApi.Sample
{
    using System.Data;
    using Autofac;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
#if NETCOREAPP2_1 || NETCOREAPP2_2
    using System.Globalization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
#endif
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Stubs;
    using Web.AspNetCore.Transaction;


    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
#if NETCOREAPP3_1
            services.AddControllers(options =>
                {
                    options.Filters.Add(new AutoTransactionHandler());
                    options.Filters.Add(new TransactionAttribute(isolationLevel: IsolationLevel.Chaos));
                })
                .AddNewtonsoftJson();

#else
            // Add framework services.
            services.AddMvcCore(options =>
                {
                    options.Filters.Add(new AutoTransactionHandler());
                    options.Filters.Add(new TransactionAttribute(isolationLevel: IsolationLevel.Chaos));
                })
                .AddDataAnnotations()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Culture = CultureInfo.InvariantCulture;
                    options.SerializerSettings.Formatting = Formatting.None;
                })
                .AddAuthorization()
                .AddFormatterMappings()
                .AddJsonFormatters()
#if NETCOREAPP2_1
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
#elif NETCOREAPP2_2
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
#endif
                ;
#endif

            services.AddMemoryCache();
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app)
        {
#if NETCOREAPP3_1
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
#else
            app.UseAuthentication();
            app.UseMvc();
#endif
        }

        /// <summary>
        ///     Configure Autofac container.
        ///     This method is automatically called by Autofac MVC integration package.
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            // register dependencies 
            builder.RegisterType<TransactionManagerStub>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterType<HttpContextAccessor>()
                .As<IHttpContextAccessor>()
                .SingleInstance();
        }
    }
}
