namespace Suteki.TardisBank.Web.Mvc.CastleWindsor
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using SharpArch.NHibernate;

    public class QueriesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromAssemblyNamed("Suteki.TardisBank.Web.Mvc")
                    .BasedOn<NHibernateQuery>()
                    .WithService.DefaultInterfaces()
                    .LifestyleTransient()
                );


            container.Register(
                Classes.FromAssemblyNamed("Suteki.TardisBank.Infrastructure")
                    .BasedOn(typeof (NHibernateQuery))
                    .WithService.DefaultInterfaces()
                    .LifestyleTransient()
                );
        }
    }
}