using JetBrains.Annotations;
using NHibernate;

namespace SharpArch.NHibernate.MultiDb
{
    public interface ISessionRegistry
    {
        INHibernateTransactionManager GetTransactionManager([NotNull] string databaseIdentifier);
        IStatelessSession CreateStatelessSession([NotNull] string databaseIdentifier);
    }
}
