using System;
using System.Data;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Serilog;
using SharpArch.Domain.PersistenceSupport;

namespace SharpArch.WebApi.Stubs
{
    public class TransactionManagerStub : ITransactionManager, IDisposable
    {
        public const string TransactionIsolationLevel = "x-transaction-isolation-level";
        public const string TransactionState = "x-transaction-result";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private TransactionWrapper _transaction;
        private static readonly ILogger Log = Serilog.Log.ForContext<TransactionManagerStub>();

        public TransactionManagerStub([NotNull] IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public IDisposable BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction == null)
                _transaction = new TransactionWrapper(isolationLevel);
            return _transaction;
        }

        public void CommitTransaction()
        {
            _httpContextAccessor.HttpContext.Response.Headers.Add(TransactionIsolationLevel, _transaction.IsolationLevel.ToString());
            _httpContextAccessor.HttpContext.Response.Headers.Add(TransactionState, "committed");
            _transaction?.Commit();
        }

        public void RollbackTransaction()
        {
            _httpContextAccessor.HttpContext.Response.Headers.Add(TransactionIsolationLevel, _transaction.IsolationLevel.ToString());
            _httpContextAccessor.HttpContext.Response.Headers.Add(TransactionState, "rolled-back");
            _transaction?.Dispose();
        }


        private class TransactionWrapper : IDisposable
        {
            public IsolationLevel IsolationLevel { get; }

            private static readonly ILogger _log = Serilog.Log.ForContext<TransactionWrapper>();

            public TransactionWrapper(IsolationLevel isolationLevel)
            {
                IsolationLevel = isolationLevel;
            }

            public void Dispose()
            {
                _log.Information("Disposed {isolationLevel}", IsolationLevel);
            }

            public void Commit()
            {
                _log.Information("Committed {isolationLevel}", IsolationLevel);
            }

            public void Rollback()
            {
                _log.Information("Rolled back {isolationLevel}", IsolationLevel);
            }
        }


        public void Dispose()
        {
            _transaction?.Dispose();
        }
    }
}
