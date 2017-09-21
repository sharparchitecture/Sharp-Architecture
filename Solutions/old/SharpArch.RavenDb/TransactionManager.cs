namespace SharpArch.RavenDb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Transactions;
    using JetBrains.Annotations;
    using Raven.Client;
    using SharpArch.Domain.PersistenceSupport;
    using IsolationLevel = System.Data.IsolationLevel;

    /// <summary>
    ///     Transaction Manager for RavenDB.
    /// </summary>
    /// <remarks>
    ///     This implementation is based on <see cref="TransactionScope"/>.
    /// </remarks>
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.ITransactionManager" />
    [PublicAPI]
    public class TransactionManager : ITransactionManager
    {
        private readonly IDocumentSession session;

        private TransactionScope transaction;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionManager" /> class.
        /// </summary>
        /// <param name="session">The document session.</param>
        public TransactionManager([NotNull] IDocumentSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            this.session = session;
        }

        /// <summary>
        ///     Begins the transaction.
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level, see <see cref="System.Data.IsolationLevel" /> for details. </param>
        /// <remarks>
        /// TransactionManager will single TransactionScope, subsequent call to <see cref="BeginTransaction"/> will return same scope
        /// until <see cref="CommitTransaction"/> or <see cref="RollbackTransaction"/> is called.
        /// </remarks>
        /// <returns>The transaction instance.</returns>
        public IDisposable BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return this.transaction ??
                (this.transaction = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions {IsolationLevel = MapIsolationLevel(isolationLevel)}));
        }


        /// <summary>
        ///     Commits the transaction, saving all changes.
        /// </summary>
        public void CommitTransaction()
        {
            if (this.transaction == null)
                throw new InvalidOperationException("Transaction was not start, make sure there is matching BeginTransaction() call.");
            this.session.SaveChanges();
            this.transaction.Complete();
            this.ClearTransaction();
        }

        /// <summary>
        ///     Rolls the transaction back, discarding any changes.
        /// </summary>
        public void RollbackTransaction()
        {
            this.ClearTransaction();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static System.Transactions.IsolationLevel MapIsolationLevel(IsolationLevel isolationLevel)
        {
            switch (isolationLevel)
            {
                case IsolationLevel.Unspecified:
                    return System.Transactions.IsolationLevel.Unspecified;
                case IsolationLevel.Chaos:
                    return System.Transactions.IsolationLevel.Chaos;
                case IsolationLevel.ReadUncommitted:
                    return System.Transactions.IsolationLevel.ReadUncommitted;
                case IsolationLevel.ReadCommitted:
                    return System.Transactions.IsolationLevel.ReadCommitted;
                case IsolationLevel.RepeatableRead:
                    return System.Transactions.IsolationLevel.RepeatableRead;
                case IsolationLevel.Serializable:
                    return System.Transactions.IsolationLevel.Serializable;
                case IsolationLevel.Snapshot:
                    return System.Transactions.IsolationLevel.Snapshot;
                default:
                    throw new ArgumentOutOfRangeException(nameof(isolationLevel), isolationLevel, null);
            }
        }

        private void ClearTransaction()
        {
            this.transaction?.Dispose();
            this.transaction = null;
        }
    }
}
