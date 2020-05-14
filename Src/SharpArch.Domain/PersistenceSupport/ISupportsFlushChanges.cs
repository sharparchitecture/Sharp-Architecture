namespace SharpArch.Domain.PersistenceSupport
{
    using System.Threading;
    using System.Threading.Tasks;


    /// <summary>
    ///     Allows to flush changes to database.
    /// </summary>
    public interface ISupportsFlushChanges
    {
        /// <summary>
        ///     Flushes everything that has been changed since the last commit.
        /// </summary>
        Task FlushChangesAsync(CancellationToken cancellationToken = default);
    }
}
