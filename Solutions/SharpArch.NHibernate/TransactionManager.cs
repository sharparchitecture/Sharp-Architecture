namespace SharpArch.NHibernate
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NHibernate;
    using JetBrains.Annotations;


    /// <summary>
    ///     Transaction manager for NHibernate.
    /// </summary>
    [PublicAPI]
    public class TransactionManager : INHibernateTransactionManager
    {
        /// <inheritdoc />
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

        /// <inheritdoc />
        public Task CommitTransactionAsync(CancellationToken cancellationToken) => Session.Transaction.CommitAsync(cancellationToken);

        /// <inheritdoc />
        public Task RollbackTransactionAsync(CancellationToken cancellationToken) => Session.Transaction.RollbackAsync(cancellationToken);

        /// <inheritdoc />
        public IDisposable BeginTransaction(IsolationLevel isolationLevel) => Session.BeginTransaction(isolationLevel);

        /// <inheritdoc />
        public void CommitTransaction() => Session.Transaction.Commit();

        /// <inheritdoc />
        public void RollbackTransaction() => Session.Transaction.Rollback();

        /// <inheritdoc />
        public void FlushChanges() => Session.Flush();
    }
}
