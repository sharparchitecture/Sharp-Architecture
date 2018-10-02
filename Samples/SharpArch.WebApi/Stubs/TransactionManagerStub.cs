using System;
using System.Data;
using Serilog;
using SharpArch.Domain.PersistenceSupport;

namespace SharpArch.WebApi.Stubs
{
    public class TransactionManagerStub : ITransactionManager, IDisposable
    {
        private TransactionWrapper _transaction;
        private static readonly ILogger Log = Serilog.Log.ForContext<TransactionManagerStub>();

        public IDisposable BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction == null)
                _transaction = new TransactionWrapper(isolationLevel);
            return _transaction;
        }

        public void CommitTransaction()
        {
            _transaction?.Commit();
        }

        public void RollbackTransaction()
        {
            _transaction?.Dispose();
        }


        private class TransactionWrapper : IDisposable
        {
            private readonly IsolationLevel _isolationLevel;

            private static readonly ILogger _log = Serilog.Log.ForContext<TransactionWrapper>();

            public TransactionWrapper(IsolationLevel isolationLevel)
            {
                _isolationLevel = isolationLevel;
            }

            public void Dispose()
            {
                _log.Information("Disposed {isolationLevel}", _isolationLevel);
            }

            public void Commit()
            {
                _log.Information("Committed {isolationLevel}", _isolationLevel);
            }

            public void Rollback()
            {
                _log.Information("Rolled back {isolationLevel}", _isolationLevel);
            }
        }


        public void Dispose()
        {
            _transaction?.Dispose();
        }
    }
}
