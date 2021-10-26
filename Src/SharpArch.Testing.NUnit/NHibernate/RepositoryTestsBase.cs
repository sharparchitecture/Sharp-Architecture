namespace SharpArch.Testing.NUnit.NHibernate
{
    using System;
#if NULLABLE_REFERENCE_TYPES
    using System.Diagnostics.CodeAnalysis;
#endif
    using global::NHibernate;
    using global::NUnit.Framework;
    using JetBrains.Annotations;
    using SharpArch.NHibernate;
    using SharpArch.NHibernate.FluentNHibernate;
    using Testing.NHibernate;


    /// <summary>
    ///     Provides a base class for running unit tests against an in-memory database created
    ///     during test execution.  This builds the database using the connection details within
    ///     NHibernate.config.  If you'd prefer unit testing against a "live" development database
    ///     such as a SQL Server instance, then use <see cref="DatabaseRepositoryTestsBase" /> instead.
    /// </summary>
    [PublicAPI]
    public abstract class RepositoryTestsBase
    {
        /// <summary>
        ///     Transaction manager.
        /// </summary>
        protected TransactionManager TransactionManager { get; private set; } = null!;

        /// <summary>
        ///     Database initializer.
        /// </summary>
#if NULLABLE_REFERENCE_TYPES
        [MaybeNull]
#endif
        protected TestDatabaseSetup DbInitializer { get; private set; } = null!;

        /// <summary>
        ///     Current NHibernate session.
        /// </summary>
        protected ISession Session => TransactionManager.Session;

        RepositoryTestsBase()
        {
        }

        /// <summary>
        /// Initializes database tests fixture.
        /// </summary>
        /// <param name="dbInitializer">Database initializer instance.</param>
        protected RepositoryTestsBase(TestDatabaseSetup dbInitializer)
        {
            DbInitializer = dbInitializer ?? throw new ArgumentNullException(nameof(dbInitializer));
            DbInitializer.GetSessionFactory();
        }

        /// <summary>
        ///     Called when [time tear down].
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if NULLABLE_REFERENCE_TYPES
            DbInitializer?.Dispose();
#else
            DbInitializer.Dispose();
#endif
            DbInitializer = null!;
        }

        /// <summary>
        ///     Closes NHibernate session.
        /// </summary>
        [TearDown]
        public virtual void TearDown()
        {
            TestDatabaseSetup.Close(Session);
        }

        /// <summary>
        ///     Flushes the session and evicts entity from it.
        /// </summary>
        /// <param name="instance">The entity instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> is <see langword="null" /></exception>
        protected void FlushSessionAndEvict(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            Session.FlushAndEvict(instance);
        }

        /// <summary>
        ///     Saves entity then flushes sessions and evicts it.
        /// </summary>
        /// <param name="instance">The entity instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> is <see langword="null" /></exception>
        protected void SaveAndEvict(object instance)
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
#if NULLABLE_REFERENCE_TYPES
            TransactionManager = new TransactionManager(DbInitializer!.InitializeSession());
#else
            TransactionManager = new TransactionManager(DbInitializer.InitializeSession());
#endif
            LoadTestData();
        }
    }
}
