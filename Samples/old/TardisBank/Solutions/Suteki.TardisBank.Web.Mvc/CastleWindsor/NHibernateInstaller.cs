using System.Diagnostics;
using System.Web.Hosting;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using NHibernate;
using SharpArch.NHibernate;
using Suteki.TardisBank.Infrastructure.NHibernateMaps;

namespace Suteki.TardisBank.Web.Mvc.CastleWindsor
{
    public class NHibernateInstaller : IWindsorInstaller
    {
        ISessionFactory CreateSessionFactory(IKernel kernel)
        {
            ISessionFactory sessionFactory = new NHibernateSessionFactoryBuilder()
                .AddMappingAssemblies(new[] { HostingEnvironment.MapPath(@"~/bin/Suteki.TardisBank.Infrastructure.dll") })
                .UseAutoPersistenceModel(new AutoPersistenceModelGenerator().Generate())
                .UseConfigFile(HostingEnvironment.MapPath("~/NHibernate.config"))
                .UseConfigurationCache(new NHibernateConfigurationFileCache())
                .BuildSessionFactory();

            return sessionFactory;
        }


        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<ISessionFactory>()
                    .UsingFactoryMethod(CreateSessionFactory)
                    .LifestyleSingleton()
                    .Named(NHibernateSessionFactoryBuilder.DefaultConfigurationName + ".factory")
                );

            container.Register(
                Component.For<ISession>()
                    .UsingFactoryMethod(k => k.Resolve<ISessionFactory>().OpenSession())
                    .LifestylePerWebRequest()
                    .Named(NHibernateSessionFactoryBuilder.DefaultConfigurationName + ".session")
#if DEBUG
                    .OnDestroy(s => Debug.WriteLine("Destroy session {0}", s.GetSessionImplementation().Timestamp))
                    .OnCreate(s => Debug.WriteLine("Created session {0}", s.GetSessionImplementation().Timestamp))
#endif
                );

            container.Register(
                Component.For<IStatelessSession>()
                    .UsingFactoryMethod(k => k.Resolve<ISessionFactory>().OpenStatelessSession())
                    .LifestylePerWebRequest()
                    .Named(NHibernateSessionFactoryBuilder.DefaultConfigurationName + ".stateless-session")
                );
        }
    }
}