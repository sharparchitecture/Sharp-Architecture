namespace SharpArch.NHibernate
{
    using global::NHibernate;
    using SharpArch.Domain.PersistenceSupport;


    /// <summary>
    ///     NHibernate transaction support.
    /// </summary>
    public interface INHibernateTransactionManager : ITransactionManager, IAsyncTransactionManager
    {
        /// <summary>
        ///     Returns NHibernate session.
        /// </summary>
        ISession Session { get; }

        /// <summary>
        ///     Flushes everything that has been changed since the last commit.
        /// </summary>
        void FlushChanges();
    }
}
