namespace SharpArch.Testing.xUnit.NHibernate
{
    using System;
    using global::NHibernate;
    using JetBrains.Annotations;
    using SharpArch.NHibernate;
    using SharpArch.Testing.NHibernate;
    using Xunit;


    /// <summary>
    ///     Provides a base class for running unit tests against an in-memory database created
    ///     during test execution.  This builds the database using the connection details within
    ///     NHibernate.config.  If you'd prefer unit testing against a "live" development database
    ///     such as a SQL Server instance, then use <see cref="DatabaseRepositoryTestsBase" /> instead.
    /// </summary>
    [PublicAPI]
    public abstract class RepositoryTestsFixture<TDatabaseInitializer>: IClassFixture<TestDatabaseInitializer>
    where TDatabaseInitializer: TestDatabaseInitializer, new()
    {
        /// <summary>
        ///     Transaction manager.
        /// </summary>
        protected TransactionManager TransactionManager { get; private set; }

        protected TestDatabaseInitializer DbInitializer { get; private set; }

        protected ISession Session => TransactionManager.Session;



        /// <summary>
        ///     Closes NHibernate session.
        /// </summary>
        [TearDown]
        public virtual void TearDown()
        {
            TestDatabaseInitializer.Close(Session);
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

        /// <summary>
        ///     Initializes session and database before test run.
        /// </summary>
        [SetUp]
        protected virtual void SetUp()
        {
            TransactionManager = new TransactionManager(DbInitializer.InitializeSession());
            LoadTestData();
        }
    }
}
