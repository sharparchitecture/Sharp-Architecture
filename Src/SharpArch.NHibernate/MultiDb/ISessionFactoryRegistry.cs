using JetBrains.Annotations;
using NHibernate;

namespace SharpArch.NHibernate.MultiDb
{
    public interface ISessionFactoryRegistry
    {
        void Add([NotNull] string databaseIdentifier, [NotNull] INHibernateSessionFactoryBuilder sessionFactoryBuilder);
        ISessionFactory GetSessionFactory([NotNull] string databaseIdentifier);
        global::NHibernate.Cfg.Configuration GetConfiguration(string databaseIdentifier);
        bool Contains(string databaseIdentifier);
        bool IsSessionFactoryCreated(string databaseIdentifier);
    }
}
