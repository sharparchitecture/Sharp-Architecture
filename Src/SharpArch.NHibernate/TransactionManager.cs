namespace SharpArch.NHibernate
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.PersistenceSupport;
    using global::NHibernate;
    using JetBrains.Annotations;


    /// <summary>
    ///     Transaction manager for NHibernate.
    /// </summary>
    [PublicAPI]
    public class TransactionManager : INHibernateTransactionManager, ISupportsTransactionStatus
    {
        /// <summary>
        ///     Creates instance of transaction manager.
        /// </summary>
        /// <param name="session"></param>
        public TransactionManager([NotNull] ISession session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }

        /// <inheritdoc />
        public ISession Session { get; }

        /// <inheritdoc />
        public Task CommitTransactionAsync(CancellationToken cancellationToken)
            => Session.Transaction.CommitAsync(cancellationToken);

        /// <inheritdoc />
        public Task RollbackTransactionAsync(CancellationToken cancellationToken)
            => Session.Transaction.RollbackAsync(cancellationToken);

        /// <inheritdoc />
        public IDisposable BeginTransaction(IsolationLevel isolationLevel)
            => Session.BeginTransaction(isolationLevel);

        /// <inheritdoc />
        public Task FlushChangesAsync(CancellationToken cancellationToken)
            => Session.FlushAsync(cancellationToken);

        /// <inheritdoc />
        public bool IsActive => Session.Transaction.IsActive;
    }
}
