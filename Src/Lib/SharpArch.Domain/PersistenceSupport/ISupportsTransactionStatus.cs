namespace SharpArch.Domain.PersistenceSupport;

/// <summary>
///     Returns transaction status.
/// </summary>
[PublicAPI]
public interface ISupportsTransactionStatus
{
    /// <summary>
    ///     Checks whether transaction is active or not.
    /// </summary>
    bool IsActive { get; }
}
