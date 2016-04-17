namespace Northwind.Web.Mvc.CastleWindsor
{
    using Castle.Windsor;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Northwind.Tasks;

    public class TasksInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes
                    .FromAssemblyContaining<CategoryTasks>()
                    .Where(Component.IsInSameNamespaceAs<CategoryTasks>())
                    .WithService.DefaultInterfaces()
                    .LifestyleTransient()
                );
        }

    }
}