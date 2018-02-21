namespace SharpArch.Testing.NUnit.NHibernate
{
    using global::NHibernate;
    using global::NUnit.Framework;
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides a behavior driven approach to unit testing data persisted to a database.  
    ///     This base class creates the database from scratch using the specified connection details 
    ///     within NHibernate.config.  The original motivation for this class was to setup the database 
    ///     within an in-memory SqlLite database, although there's nothing to prevent any other target 
    ///     database being used.  If you'd prefer unit testing in the more traditional manner, but still 
    ///     using an in-memory database, then use <see cref = "RepositoryTestsBase" />.  For testing against
    ///     a "live" development database, such as a SQL Server instance, use <see cref = "DatabaseRepositoryTestsBase" />
    ///     as your test fixture base class.
    /// </summary>
    [PublicAPI]
    public abstract class RepositoryBehaviorSpecificationTestsBase : BehaviorSpecificationTestsBase
    {
        private ISession session;
        TestDatabaseInitializer dbInitializer;


        /// <summary>
        /// Closes NHibernate session.
        /// </summary>
        [TearDown]
        public virtual void TearDown() 
        {
            TestDatabaseInitializer.Close(session);
        }

        /// <summary>
        /// Flushes current NHibernate session and evicts entity from it.
        /// </summary>
        /// <param name="instance"></param>
        protected void FlushSessionAndEvict(object instance) 
        {
            session.FlushAndEvict(instance);
        }

        /// <summary>
        /// Establishes test context and runs the <see cref="BehaviorSpecificationTestsBase.Act" /> method.
        /// Execetuion exception will be captured in <see cref="BehaviorSpecificationTestsBase.ExceptionThrown" /> property.
        /// </summary>
        [SetUp]
        protected override void SetUp() 
        {
            session = dbInitializer.InitializeSession();
            base.SetUp();
        }

        /// <summary>
        /// Initializes NHibernate <see cref="ISessionFactory"/> (fixture setup).
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            dbInitializer = new TestDatabaseInitializer(TestContext.CurrentContext.TestDirectory);
        }
    }
}
