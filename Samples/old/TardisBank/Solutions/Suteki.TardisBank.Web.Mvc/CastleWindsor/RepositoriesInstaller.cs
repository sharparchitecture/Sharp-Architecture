namespace Suteki.TardisBank.Web.Mvc.CastleWindsor
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate;
    using SharpArch.NHibernate.Contracts.Repositories;

    public class RepositoriesInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            AddGenericRepositoriesTo(container);
            AddCustomRepositoriesTo(container);
            container.Register(Component.For<ITransactionManager>()
                .ImplementedBy<TransactionManager>().LifestylePerWebRequest());
        }
        
        private static void AddCustomRepositoriesTo(IWindsorContainer container)
        {
            container.Register(
                Classes.FromAssemblyNamed("Suteki.TardisBank.Infrastructure")
                    .BasedOn(typeof(IRepositoryWithTypedId<,>))
                    .WithService.DefaultInterfaces().LifestyleTransient());
        }

        private static void AddGenericRepositoriesTo(IWindsorContainer container)
        {
            container.Register(
                Component.For(typeof(INHibernateRepository<>))
                    .ImplementedBy(typeof(NHibernateRepository<>))
                    .Named("nhibernateRepositoryType")
                    .Forward(typeof(IRepository<>))
                    .LifestylePerWebRequest());
            
            container.Register(
                Component.For(typeof(INHibernateRepositoryWithTypedId<,>))
                    .ImplementedBy(typeof(NHibernateRepositoryWithTypedId<,>))
                    .Named("nhibernateRepositoryWithTypedId")
                    .Forward(typeof(IRepositoryWithTypedId<,>))
                    .LifestylePerWebRequest());
            
            container.Register(
                Component.For(typeof(ILinqRepository<>))
                    .ImplementedBy(typeof(LinqRepository<>))
                    .Named("nhibernateLinqWithTypedId")
                    .LifestylePerWebRequest());
        }
    }
}