namespace SharpArch.NHibernate;

using System.Data;
using Domain.PersistenceSupport;
using global::NHibernate;


/// <summary>
///     Transaction manager for NHibernate.
/// </summary>
[PublicAPI]
public class TransactionManager : INHibernateTransactionManager, ISupportsTransactionStatus
{
    static readonly string _noTransactionAvailable = "No transaction is currently active.";

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

    /// <inheritdoc />
    public Task CommitTransactionAsync(CancellationToken cancellationToken)
        => GetTransaction()?.CommitAsync(cancellationToken) ?? throw new InvalidOperationException(_noTransactionAvailable);

    /// <inheritdoc />
    public Task RollbackTransactionAsync(CancellationToken cancellationToken)
        => GetTransaction()?.RollbackAsync(cancellationToken) ?? throw new InvalidOperationException(_noTransactionAvailable);

    /// <inheritdoc />
    public IDisposable BeginTransaction(IsolationLevel isolationLevel)
        => Session.BeginTransaction(isolationLevel);

    /// <inheritdoc />
    public Task FlushChangesAsync(CancellationToken cancellationToken)
        => Session.FlushAsync(cancellationToken);

    /// <inheritdoc />
    public bool IsActive => GetTransaction()?.IsActive ?? false;

    /// <summary>
    ///     Returns current transaction or <c>null</c> if no transaction was open.
    /// </summary>
    protected ITransaction? GetTransaction()
        => Session.GetCurrentTransaction();
}
