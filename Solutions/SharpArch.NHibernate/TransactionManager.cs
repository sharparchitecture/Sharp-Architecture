namespace SharpArch.NHibernate
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NHibernate;
    using JetBrains.Annotations;
    using SharpArch.Domain.PersistenceSupport;


    /// <summary>
    ///     Transaction manager for NHibernate.
    /// </summary>
    [PublicAPI]
    public class TransactionManager : ITransactionManager, IAsyncTransactionManager
    {
        /// <summary>
        ///     NHibernate session.
        /// </summary>
        [NotNull]
        public ISession Session { get; }

        /// <summary>
        ///     Creates instance of transaction manager.
        /// </summary>
        /// <param name="session"></param>
        public TransactionManager([NotNull] ISession session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }

        /// <summary>
        ///     Begins the transaction.
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level, see <see cref="IsolationLevel" /> for details.</param>
        /// <returns>The transaction instance.</returns>
        public IDisposable BeginTransaction(IsolationLevel isolationLevel) => Session.BeginTransaction(isolationLevel);

        /// <summary>
        ///     Commits the transaction, saving all changes.
        /// </summary>
        public void CommitTransaction() => Session.Transaction.Commit();

        /// <summary>
        ///     Rolls the transaction back, discarding any changes.
        /// </summary>
        public void RollbackTransaction() => Session.Transaction.Rollback();

        /// <summary>
        ///     This isn't specific to any one DAO and flushes everything that has been changed since the last commit.
        /// </summary>
        public void CommitChanges() => Session.Flush();

        /// <inheritdoc />
        public Task CommitTransactionAsync(CancellationToken cancellationToken) => Session.Transaction.CommitAsync(cancellationToken);

        /// <inheritdoc />
        public Task RollbackTransactionAsync(CancellationToken cancellationToken) => Session.Transaction.RollbackAsync(cancellationToken);
    }
}
