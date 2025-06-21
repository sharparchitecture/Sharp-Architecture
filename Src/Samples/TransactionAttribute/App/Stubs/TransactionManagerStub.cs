namespace TransactionAttribute.WebApi.Stubs
{
    using System.Data;
    using Serilog;
    using SharpArch.Domain.PersistenceSupport;


    public class TransactionManagerStub : ITransactionManager, IDisposable, ISupportsTransactionStatus
    {
        public const string TransactionIsolationLevel = "x-transaction-isolation-level";
        public const string TransactionState = "x-transaction-result";
        readonly IHttpContextAccessor _httpContextAccessor;
        TransactionWrapper? _transaction;

        public TransactionManagerStub(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public void Dispose()
        {
            _transaction?.Dispose();
        }

        /// <inheritdoc />
        public bool IsActive => true;

        public IDisposable BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return _transaction ??= new TransactionWrapper(isolationLevel);
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            _httpContextAccessor.HttpContext!.Response.Headers.Add(TransactionIsolationLevel, _transaction?.IsolationLevel.ToString() ?? "unknown");
            _httpContextAccessor.HttpContext.Response.Headers.Add(TransactionState, "committed");
            _transaction?.Commit();
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            _httpContextAccessor.HttpContext!.Response.Headers.Add(TransactionIsolationLevel, _transaction?.IsolationLevel.ToString() ?? "unknown");
            _httpContextAccessor.HttpContext.Response.Headers.Add(TransactionState, "rolled-back");
            _transaction?.Rollback();
            return Task.CompletedTask;
        }


        class TransactionWrapper : IDisposable
        {
            static readonly ILogger _log = Log.ForContext<TransactionWrapper>();
            public IsolationLevel IsolationLevel { get; }

            public TransactionWrapper(IsolationLevel isolationLevel)
            {
                IsolationLevel = isolationLevel;
            }

            public void Dispose()
            {
                _log.Information("Disposed {IsolationLevel}", IsolationLevel);
            }

            public void Commit()
            {
                _log.Information("Committed {IsolationLevel}", IsolationLevel);
            }

            public void Rollback()
            {
                _log.Information("Rolled back {IsolationLevel}", IsolationLevel);
            }
        }
    }
}
