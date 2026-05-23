namespace Suteki.TardisBank.Web.Mvc.CastleWindsor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using MediatR;

    /// <summary>
    /// Installs and configures MediatR.
    /// See https://github.com/jbogard/MediatR/tree/master/src/MediatR.Examples.Windsor
    /// </summary>
    public class MediatorInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Kernel.AddHandlersFilter(new ContravariantFilter());

            container.Register(Component.For<IMediator>().ImplementedBy<Mediator>());

            container.Register(
                Component.For<SingleInstanceFactory>().UsingFactoryMethod<SingleInstanceFactory>(k => t => k.Resolve(t)));
            container.Register(
                Component.For<MultiInstanceFactory>()
                    .UsingFactoryMethod<MultiInstanceFactory>(k => t => (IEnumerable<object>) k.ResolveAll(t)));
        }

        public class ContravariantFilter : IHandlersFilter
        {
            public bool HasOpinionAbout(Type service)
            {
                if (!service.IsGenericType)
                {
                    return false;
                }

                var genericType = service.GetGenericTypeDefinition();
                var genericArguments = genericType.GetGenericArguments();
                return genericArguments.Length == 1
                    && genericArguments[0].GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant);
            }

            public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
            {
                return handlers;
            }
        }
    }
}
