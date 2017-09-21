namespace SharpArch.NHibernate
{
    using System;
    using System.Data;
    using global::NHibernate;
    using JetBrains.Annotations;
    using SharpArch.Domain.PersistenceSupport;

    /// <summary>
    ///     Transaction manager for NHibernate.
    /// </summary>
    [PublicAPI]
    public class TransactionManager : ITransactionManager
    {
        /// <summary>
        ///     Creates instance of transaction manager.
        /// </summary>
        /// <param name="session"></param>
        public TransactionManager([NotNull] ISession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            Session = session;
        }


        /// <summary>
        ///     NHibernate session.
        /// </summary>
        [NotNull]
        public ISession Session { get; }

        /// <summary>
        ///     Begins the transaction.
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level, see <see cref="IsolationLevel" /> for details.</param>
        /// <returns>The transaction instance.</returns>
        public IDisposable BeginTransaction(IsolationLevel isolationLevel)
        {
            return this.Session.BeginTransaction(isolationLevel);
        }

        /// <summary>
        ///     Commits the transaction, saving all changes.
        /// </summary>
        public void CommitTransaction()
        {
            this.Session.Transaction.Commit();
        }

        /// <summary>
        ///     Rolls the transaction back, discarding any changes.
        /// </summary>
        public void RollbackTransaction()
        {
            this.Session.Transaction.Rollback();
        }

        /// <summary>
        ///     This isn't specific to any one DAO and flushes everything that has been changed since the last commit.
        /// </summary>
        public void CommitChanges()
        {
            this.Session.Flush();
        }
    }
}
