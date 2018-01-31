namespace SharpArch.Testing.NUnit.NHibernate
{
    using System;
    using System.Transactions;
    using global::NHibernate;
    using global::NUnit.Framework;
    using JetBrains.Annotations;
    using SharpArch.NHibernate;

    /// <summary>
    ///     Provides a base class for running unit tests against an in-memory database created
    ///     during test execution.  This builds the database using the connection details within
    ///     NHibernate.config.  If you'd prefer unit testing against a "live" development database
    ///     such as a SQL Server instance, then use <see cref = "DatabaseRepositoryTestsBase" /> instead.
    ///     If you'd prefer a more behavior driven approach to testing against the in-memory database,
    ///     use <see cref = "RepositoryBehaviorSpecificationTestsBase" /> instead.
    /// </summary>
    [PublicAPI]
    public abstract class RepositoryTestsBase
    {
        /// <summary>
        /// Transaction manager.
        /// </summary>
        protected TransactionManager TransactionManager { get; private set; }

        /// <summary>
        /// NHibernate session
        /// </summary>
        protected ISession Session { get; private set; }

        TestDatabaseInitializer dbInitializer;


        /// <summary>
        /// Initializes NHibernate <see cref="ISessionFactory"/> (fixture setup).
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            dbInitializer = new TestDatabaseInitializer(TestContext.CurrentContext.TestDirectory);
            dbInitializer.GetSessionFactory();
        }

        /// <summary>
        /// Called when [time tear down].
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            dbInitializer?.Dispose();
            dbInitializer = null;
        }

        /// <summary>
        /// Closes NHibernate session.
        /// </summary>
        [TearDown]
        public virtual void TearDown()
        {
            TestDatabaseInitializer.Close(Session);
        }

        /// <summary>
        /// Flushes the session and evicts entity from it.
        /// </summary>
        /// <param name="instance">The entity instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <see langword="null"/></exception>
        protected void FlushSessionAndEvict([NotNull] object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            Session.FlushAndEvict(instance);
        }

        /// <summary>
        /// Saves entity then flushes sessions and evicts it.
        /// </summary>
        /// <param name="instance">The entity instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <see langword="null"/></exception>
        protected void SaveAndEvict([NotNull] object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            Session.Save(instance);
            FlushSessionAndEvict(instance);
        }

        /// <summary>
        /// Initializes database before each test run.
        /// </summary>
        protected abstract void LoadTestData();

        /// <summary>
        /// Initializes session and database before test run.
        /// </summary>
        [SetUp]
        protected virtual void SetUp()
        {
            Session = dbInitializer.InitializeSession();
            TransactionManager = new TransactionManager(Session);
            this.LoadTestData();
        }
    }
}