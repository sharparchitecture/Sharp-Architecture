namespace SharpArch.Testing.NUnit.NHibernate
{
    using System;
    using global::NHibernate;
    using global::NHibernate.Cfg;
    using global::NUnit.Framework;
    using JetBrains.Annotations;
    using Testing.NHibernate;


    /// <summary>
    ///     <para>
    ///         Initiates a transaction before each test is run and rolls back the transaction after
    ///         the test completes.  Consequently, tests make no permanent changes to the DB.
    ///     </para>
    ///     <para>
    ///         This is appropriate as a base class if you're unit tests run against a live, development
    ///         database.  If, alternatively, you'd prefer to use an in-memory database such as SqlLite,
    ///         then use <see cref="RepositoryTestsBase" /> as your base class.
    ///         As the preferred mechanism is in-memory unit testing, this class is provided mainly
    ///         for backward compatibility.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Descendants must have a default constructor which provides <see cref="TestDatabaseSetup" />.
    ///                 Session factory in initialized once per test fixture.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 This class expects database structure to be present and up-to-date.
    ///                 It will not run schema export on it.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [PublicAPI]
    public abstract class DatabaseRepositoryTestsBase
    {
        /// <summary>
        ///     Database initializer instance.
        /// </summary>
        protected TestDatabaseSetup Initializer { get; private set; }

        /// <summary>
        ///     Returns current NHibernate session.
        /// </summary>
        protected ISession Session { get; private set; }

        private DatabaseRepositoryTestsBase()
        {
        }

        protected DatabaseRepositoryTestsBase([NotNull] TestDatabaseSetup initializer)
        {
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        }

        /// <summary>
        ///     Creates NHibernate <see cref="ISessionFactory" />.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (Initializer == null) throw new InvalidOperationException($"{nameof(Initializer)} is not set.");
            UpdateConfiguration(Initializer.GetConfiguration());
        }

        /// <summary>
        ///     Can be used to override Session Factory settings.
        /// </summary>
        /// <param name="configuration"></param>
        protected virtual void UpdateConfiguration([NotNull] Configuration configuration)
        {
        }

        /// <summary>
        ///     Creates new <see cref="ISession" />.
        /// </summary>
        [SetUp]
        public virtual void SetUp()
        {
            if (Initializer == null) throw new InvalidOperationException($"{nameof(Initializer)} is not set.");
            Session = Initializer.GetSessionFactory().OpenSession();
            Session.BeginTransaction();
        }

        /// <summary>
        ///     Rollbacks active transaction and closes <see cref="ISession" />.
        /// </summary>
        [TearDown]
        public virtual void TearDown()
        {
            if (Session != null)
            {
                if (Session.Transaction.IsActive) Session.Transaction.Rollback();
                Session.Dispose();
                Session = null;
            }
        }

        /// <summary>
        ///     Disposes <see cref="ISessionFactory" />.
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Initializer?.Dispose();
            Initializer = null;
        }
    }
}
