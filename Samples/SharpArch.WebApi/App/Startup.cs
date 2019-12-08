namespace SharpArch.WebApi.Sample
{
    using System.Data;
    using System.Globalization;
    using Autofac;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Stubs;
    using Web.AspNetCore.Transaction;


    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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
                .AddFormatterMappings()
                .AddJsonFormatters()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }

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
