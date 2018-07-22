namespace Suteki.TardisBank.Web.Mvc.CastleWindsor
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using Tasks;

    public class TasksInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes
                    .FromAssemblyContaining<UserService>()
                    .Where(Component.IsInSameNamespaceAs<UserService>())
                    .WithService.DefaultInterfaces()
                    .LifestyleTransient()
                );
        }
    }
}