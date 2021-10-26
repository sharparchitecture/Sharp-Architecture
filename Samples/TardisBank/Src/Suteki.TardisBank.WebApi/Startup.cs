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
            services.AddControllers(options => { options.Filters.Add(new AutoTransactionHandler()); })
                .AddNewtonsoftJson();

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
            builder.RegisterType<HttpContextAccessor>()
                .As<IHttpContextAccessor>()
                .SingleInstance();

            builder.RegisterAssemblyTypes(typeof(AccountMap).Assembly)
                .AsClosedTypesOf(typeof(IRepository<,>))
#if DEBUG
                .Where(t =>
                {
                    var x = t.IsClosedTypeOf(typeof(IRepository<,>));
                    _logger.Information("{type}, register: {register}", t, x);
                    return x;
                })
#endif
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(NHibernateRepository<,>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(LinqRepository<,>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}
