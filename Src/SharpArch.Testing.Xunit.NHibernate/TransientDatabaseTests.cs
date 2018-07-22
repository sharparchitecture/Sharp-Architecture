namespace SharpArch.Testing.Xunit.NHibernate
{
    using System;
    using global::NHibernate;
    using global::Xunit;
    using JetBrains.Annotations;
    using SharpArch.NHibernate;
    using SharpArch.Testing.NHibernate;


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
    public abstract class TransientDatabaseTests<TDatabaseSetup> : IClassFixture<TDatabaseSetup>, IDisposable
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

        /// <inheritdoc />
        protected TransientDatabaseTests(TestDatabaseSetup dbSetup)
        {
            DbSetup = dbSetup;
            TransactionManager = new TransactionManager(DbSetup.InitializeSession());
            // ReSharper disable once VirtualMemberCallInConstructor
            LoadTestData();
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            TestDatabaseSetup.Close(Session);
        }

        /// <summary>
        ///     Flushes the session and evicts entity from it.
        /// </summary>
        /// <param name="instance">The entity instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> is <see langword="null" /></exception>
        protected void FlushSessionAndEvict([NotNull] object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            Session.FlushAndEvict(instance);
        }

        /// <summary>
        ///     Saves entity then flushes sessions and evicts it.
        /// </summary>
        /// <param name="instance">The entity instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> is <see langword="null" /></exception>
        protected void SaveAndEvict([NotNull] object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            Session.Save(instance);
            FlushSessionAndEvict(instance);
        }

        /// <summary>
        ///     Initializes database before each test run.
        /// </summary>
        protected abstract void LoadTestData();
    }
}
