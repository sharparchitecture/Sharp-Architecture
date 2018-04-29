namespace SharpArch.Domain.PersistenceSupport
{
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;


    /// <summary>
    ///     Defines the public members of a class that represents an asynchronous database context which handles
    ///     application wide DB activities such as committing any pending changes, beginning a
    ///     transaction, rolling back a transaction, etc.
    /// </summary>
    /// <remarks>
    ///     Note that outside of <see cref="T:IDbContext.CommitChanges" />, you shouldn't have to
    ///     invoke this object very often. If you're using on of the the <c>TransactionAttribute</c>
    ///     atrributes provided by SharpArch on your controller actions, then the transaction
    ///     opening/committing will be taken care of for you.
    /// </remarks>
    [PublicAPI]
    public interface IAsyncTransactionManager
    {
        /// <summary>
        ///     Commits the transaction, saving all changes.
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     Rolls the transaction back, discarding any changes.
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
    }
}
