namespace SharpArch.NHibernate.MultiDb
{
    using Domain.PersistenceSupport;
    using global::NHibernate;
    using JetBrains.Annotations;


    /// <summary>
    ///     Keeps track of Sessions.
    /// </summary>
    [PublicAPI]
    public interface INHibernateSessionRegistry : ISessionRegistry
    {
        /// <summary>
        ///     Returns <see cref="INHibernateTransactionManager" /> for given database.
        /// </summary>
        /// <param name="databaseIdentifier"></param>
        /// <returns></returns>
        INHibernateTransactionManager GetNHibernateTransactionManager([NotNull] string databaseIdentifier);

        /// <summary>
        ///     Creates new <see cref="IStatelessSession" />.
        /// </summary>
        /// <param name="databaseIdentifier">Database identifier.</param>
        /// <returns>New instance of <see cref="IStatelessSession" /></returns>
        /// <remarks>Stateless sessions are not tracked by SessionRegistry and it is client's responsibility to dispose them.</remarks>
        IStatelessSession CreateStatelessSession([NotNull] string databaseIdentifier);
    }
}
