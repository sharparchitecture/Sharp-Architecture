namespace Suteki.TardisBank.Web.Mvc.CastleWindsor
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using MediatR;
    using Suteki.TardisBank.Tasks.EventHandlers;

    /// <summary>
    /// Installs Command and Query handlers.
    /// </summary>
    public class HandlersInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromAssemblyContaining<SendMessageEmailHandler>()
                    .BasedOn(typeof (IRequestHandler<,>))
                    .WithService.AllInterfaces()
                    .LifestylePerWebRequest());

            container.Register(
                Classes.FromAssemblyContaining<SendMessageEmailHandler>()
                    .BasedOn(typeof (INotificationHandler<>))
                    .WithService.AllInterfaces()
                    .LifestylePerWebRequest());
        }
    }
}
