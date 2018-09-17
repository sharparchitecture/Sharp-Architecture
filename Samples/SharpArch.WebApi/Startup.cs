using System.Data;
using System.Globalization;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SharpArch.AspNetCore;
using SharpArch.WebApi.Filters;

namespace SharpArch.WebApi
{
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
                    options.Filters.Add<HandleTransactionFilter>();
                    options.Filters.Add(new TransactionAttribute(IsolationLevel.Chaos));
                })
                .AddDataAnnotations()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Culture = CultureInfo.InvariantCulture;
                    options.SerializerSettings.Formatting = Formatting.None;
                })
                .AddFormatterMappings()
                .AddJsonFormatters()
                .AddXmlSerializerFormatters()
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
        }
    }
}
