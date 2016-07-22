using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using SharpArch.Domain.PersistenceSupport;
using SharpArch.NHibernate;

namespace Northwind.Web.Mvc.CastleWindsor
{
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
