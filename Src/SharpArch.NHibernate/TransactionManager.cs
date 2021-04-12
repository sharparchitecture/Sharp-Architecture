namespace SharpArch.NHibernate.Impl
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
        private static readonly string NoTransactionAvailable = "No transaction is currently active.";

        /// <summary>
        ///     Creates instance of transaction manager.
        /// </summary>
        /// <param name="session"></param>
        public TransactionManager(ISession session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }

        /// <inheritdoc />
        public ISession Session { get; }

        /// <summary>
        ///     Returns current transaction or <c>null</c> if no transaction was open.
        /// </summary>
        protected ITransaction GetTransaction()
            => Session.GetCurrentTransaction();

        /// <inheritdoc />
        public Task CommitTransactionAsync(CancellationToken cancellationToken)
            => GetTransaction()?.CommitAsync(cancellationToken) ?? throw new InvalidOperationException(NoTransactionAvailable);

        /// <inheritdoc />
        public Task RollbackTransactionAsync(CancellationToken cancellationToken)
            => GetTransaction()?.RollbackAsync(cancellationToken) ?? throw new InvalidOperationException(NoTransactionAvailable);

        /// <inheritdoc />
        public IDisposable BeginTransaction(IsolationLevel isolationLevel)
            => Session.BeginTransaction(isolationLevel);

        /// <inheritdoc />
        public Task FlushChangesAsync(CancellationToken cancellationToken)
            => Session.FlushAsync(cancellationToken);

        /// <inheritdoc />
        public bool IsActive => GetTransaction()?.IsActive ?? false;
    }
}
