using JetBrains.Annotations;
using NHibernate;

namespace SharpArch.NHibernate.MultiDb
{
    public interface ISessionFactoryRegistry {
        bool Contains(string databaseIdentifier);
        bool IsSessionFactoryCreated(string databaseIdentifier);
        ISessionFactory GetSessionFactory([NotNull] string databaseIdentifier);

    }

    //public interface INHibernateSessionFactoryBuilder
    //{
    //    void Add([NotNull] string databaseIdentifier, [NotNull] NHibernate.INHibernateSessionFactoryBuilder sessionFactoryBuilder);
    //    global::NHibernate.Cfg.Configuration GetConfiguration(string databaseIdentifier);
    //}
}
