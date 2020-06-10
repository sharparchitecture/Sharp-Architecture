// ReSharper disable InconsistentLogPropertyNaming

namespace Suteki.TardisBank.WebApi
{
    using Autofac;
    using AutoMapper;
    using Domain;
    using Infrastructure.Configuration;
    using Infrastructure.NHibernateMaps;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate;
    using SharpArch.NHibernate.Extensions.DependencyInjection;
    using SharpArch.Web.AspNetCore.Transaction;

#if NETCOREAPP2_1 || NETCOREAPP2_2
    using System.Globalization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
#endif


    public class Startup
    {
        static readonly ILogger _logger = Log.ForContext<Startup>();

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
            services.AddControllers(options => { options.Filters.Add(new AutoTransactionHandler()); })
                .AddNewtonsoftJson();

#else
            // Add framework services.
            services.AddMvcCore(options =>
                {
                    options.Filters.Add(new AutoTransactionHandler());
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

            services.AddNHibernateWithSingleDatabase(sp =>
            {
                return new NHibernateSessionFactoryBuilder()
                    .AddMappingAssemblies(new[] {typeof(Child).Assembly})
                    .UseAutoPersistenceModel(new AutoPersistenceModelGenerator().Generate())
                    .UseConfigFile(@"NHibernate.config");
            });

            services.AddAutoMapper(typeof(AnnouncementMappingProfile));

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
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
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
            builder.RegisterType<HttpContextAccessor>()
                .As<IHttpContextAccessor>()
                .SingleInstance();

            builder.RegisterAssemblyTypes(typeof(AccountMap).Assembly)
                .AsClosedTypesOf(typeof(IAsyncRepositoryWithTypedId<,>))
#if DEBUG
                .Where(t =>
                {
                    var x = t.IsClosedTypeOf(typeof(IAsyncRepositoryWithTypedId<,>));
                    _logger.Information("{type}, register: {register}", t, x);
                    return x;
                })
#endif
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(NHibernateRepositoryWithTypedId<,>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(NHibernateRepository<>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(LinqRepositoryWithTypedId<,>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(LinqRepository<>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}
