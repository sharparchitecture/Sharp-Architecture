namespace SharpArch.NHibernate
{
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.PersistenceSupport;
    using global::NHibernate;


    /// <summary>
    ///     NHibernate transaction support.
    /// </summary>
    public interface INHibernateTransactionManager : ITransactionManager
    {
        /// <summary>
        ///     Returns NHibernate session.
        /// </summary>
        ISession Session { get; }

        /// <summary>
        ///     Flushes everything that has been changed since the last commit.
        /// </summary>
        Task FlushChangesAsync(CancellationToken cancellationToken = default);
    }
}
