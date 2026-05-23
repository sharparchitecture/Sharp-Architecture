namespace TransactionAttribute.WebApi
{
    using System.Data;
    using Autofac;
    using SharpArch.Web.AspNetCore.Transaction;
    using Stubs;


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
            services.AddControllers(options =>
                {
                    options.Filters.Add(new AutoTransactionHandler());
                    options.Filters.Add(new TransactionAttribute(IsolationLevel.Chaos));
                })
                ;

            services.AddMemoryCache();
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
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
