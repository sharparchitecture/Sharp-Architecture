namespace SharpArch.Testing.Xunit.NHibernate
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NHibernate;
    using global::Xunit;
    using JetBrains.Annotations;
    using SharpArch.NHibernate;
    using SharpArch.NHibernate.Impl;
    using Testing.NHibernate;


    /// <summary>
    ///     Provides a base class for running unit tests against transient database created
    ///     during test execution.
    ///     <para>
    ///         This builds the database using the connection details within NHibernate.config.
    ///     </para>
    ///     <para>
    ///         If you'd prefer unit testing against a "live" development database
    ///         such as a SQL Server instance, then use <see cref="LiveDatabaseTests{TDatabaseInitializer}" /> instead.
    ///     </para>
    /// </summary>
    [PublicAPI]
    public abstract class TransientDatabaseTests<TDatabaseSetup> : IClassFixture<TDatabaseSetup>, IDisposable, IAsyncLifetime
        where TDatabaseSetup : TestDatabaseSetup, new()
    {
        /// <summary>
        ///     Transaction manager.
        /// </summary>
        
        protected TransactionManager TransactionManager { get; private set; }

        /// <summary>
        ///     Database initializer.
        /// </summary>
        
        protected TestDatabaseSetup DbSetup { get; private set; }

        /// <summary>
        ///     Database session.
        /// </summary>
        
        protected ISession Session => TransactionManager.Session;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="dbSetup"></param>
        protected TransientDatabaseTests(TestDatabaseSetup dbSetup)
        {
            DbSetup = dbSetup ?? throw new ArgumentNullException(nameof(dbSetup));
            TransactionManager = new TransactionManager(DbSetup.InitializeSession());
        }

        /// <inheritdoc />
        public Task InitializeAsync()
            => LoadTestData(CancellationToken.None);

        /// <inheritdoc />
        public Task DisposeAsync()
            => Task.CompletedTask;

        /// <inheritdoc />
        public virtual void Dispose()
        {
            TestDatabaseSetup.Close(Session);
        }

        /// <summary>
        ///     Flushes the session and evicts entity from it.
        /// </summary>
        /// <param name="instance">The entity instance.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> is <see langword="null" /></exception>
        
        protected Task FlushSessionAndEvict(object instance, CancellationToken cancellationToken = default)
            => Session.FlushAndEvictAsync(instance, cancellationToken);

        /// <summary>
        ///     Saves entity then flushes sessions and evicts it.
        /// </summary>
        /// <param name="instance">The entity instance.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> is <see langword="null" /></exception>
        protected async Task SaveAndEvict(object instance, CancellationToken cancellationToken = default)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            await Session.SaveAsync(instance, cancellationToken).ConfigureAwait(false);
            await FlushSessionAndEvict(instance, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Initializes database before each test run.
        /// </summary>
        
        protected abstract Task LoadTestData(CancellationToken cancellationToken);
    }
}
