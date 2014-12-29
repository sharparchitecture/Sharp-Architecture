namespace SharpArch.Testing.NUnit.NHibernate
{
    using global::NUnit.Framework;

    using SharpArch.NHibernate;

    /// <summary>
    ///     Initiates a transaction before each test is run and rolls back the transaction after
    ///     the test completes.  Consequently, tests make no permanent changes to the DB.
    /// 
    ///     This is appropriate as a base class if you're unit tests run against a live, development
    ///     database.  If, alternatively, you'd prefer to use an in-memory database such as SqlLite,
    ///     then use <see cref = "RepositoryTestsBase" /> or <see cref = "RepositoryBehaviorSpecificationTestsBase" />
    ///     as your base class.
    /// 
    ///     As the preferred mechanism is for in-memory unit testsing, this class is provided mainly
    ///     for backward compatibility.
    /// </summary>
    public abstract class DatabaseRepositoryTestsBase
    {
        [SetUp]
        public virtual void SetUp()
        {
            RepositoryTestsHelper.InitializeNHibernateSession();
            NHibernateSession.Current.BeginTransaction();
        }

        [TearDown]
        public virtual void TearDown()
        {
            NHibernateSession.Current.Transaction.Rollback();
            RepositoryTestsHelper.Shutdown();
        }
    }
}