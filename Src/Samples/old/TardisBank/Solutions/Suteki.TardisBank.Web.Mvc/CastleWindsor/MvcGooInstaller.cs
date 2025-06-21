namespace Suteki.TardisBank.Web.Mvc.CastleWindsor
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using Suteki.TardisBank.Tasks;
    using Suteki.TardisBank.Web.Mvc.Utilities;

    public class MvcGooInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                    Component.For<TardisConfiguration>().Named("TardisConfiguration").LifeStyle.Singleton
                );

            container.Register(
                    Component.For<IHttpContextService>().ImplementedBy<HttpContextService>().LifeStyle.Transient
                );

            container.Register(
                    Component.For<IFormsAuthenticationService>().ImplementedBy<FormsAuthenticationService>().LifeStyle.Transient
                );
        }
    }
}