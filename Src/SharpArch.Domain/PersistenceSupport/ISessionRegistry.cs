namespace SharpArch.Domain.PersistenceSupport
{
    using System.Collections.Generic;
    using JetBrains.Annotations;


    /// <summary>
    ///     Keeps track of Sessions.
    /// </summary>
    [PublicAPI]
    public interface ISessionRegistry
    {
        public bool ContainsDatabase(string databaseIdentifier);

        /// <summary>
        ///     Returns <see cref="ITransactionManager" /> for given database.
        /// </summary>
        /// <param name="databaseIdentifier"></param>
        /// <returns></returns>
        ITransactionManager GetTransactionManager([NotNull] string databaseIdentifier);

        /// <summary>
        ///     Returns snapshot of all open transactions.
        /// </summary>
        /// <returns>
        ///     Array of <see cref="KeyValuePair{TKey,TValue}" /> of database identifier and
        ///     <see cref="ITransactionManager" />.
        /// </returns>
        KeyValuePair<string, ITransactionManager>[] GetExistingTransactionsSnapshot();
    }
}
