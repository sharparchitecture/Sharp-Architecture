namespace Suteki.TardisBank.Web.Mvc.CastleWindsor
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate;

    /// <summary>
    /// Installs S#Arch 
    /// </summary>
    public class SharpArchInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For(typeof (IEntityDuplicateChecker))
                    .ImplementedBy(typeof (EntityDuplicateChecker))
                    .Named("entityDuplicateChecker")
                    .LifestyleTransient());

        }
    }
}
