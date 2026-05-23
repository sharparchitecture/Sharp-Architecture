namespace SharpArch.Testing.Xunit.NHibernate;

using global::NHibernate;
using global::Xunit;
using Testing.NHibernate;


/// <summary>
///     <para>
///         Initiates a transaction before each test is run and rolls back the transaction after
///         the test completes.  Consequently, tests make no permanent changes to the DB.
///     </para>
///     <para>
///         This is appropriate as a base class if you're unit tests run against a live, development
///         database.  If, alternatively, you'd prefer to use an in-memory database such as SqlLite,
///         then use <see cref="TransientDatabaseTests{TDatabaseInitializer}" /> as your base class.
///         As the preferred mechanism is in-memory unit testing, this class is provided mainly
///         for backward compatibility.
///     </para>
/// </summary>
/// <remarks>
///     This class expects database structure to be present and up-to-date.
///     It will not run schema export on it.
/// </remarks>
[PublicAPI]
public abstract class LiveDatabaseTests<TDatabaseSetup> : IClassFixture<TDatabaseSetup>, IDisposable
    where TDatabaseSetup : TestDatabaseSetup, new()
{
    /// <summary>
    ///     Database setup class
    /// </summary>
    protected TDatabaseSetup DbSetup { get; private set; }

    /// <summary>
    ///     Returns current NHibernate session.
    /// </summary>
    protected ISession? Session { get; private set; }

    /// <summary>
    ///     Creates instance of live database tests.
    /// </summary>
    /// <param name="setup">Database setup, <see cref="TestDatabaseSetup" />.</param>
    protected LiveDatabaseTests(TDatabaseSetup setup)
    {
        DbSetup = setup ?? throw new ArgumentNullException(nameof(setup));
        Session = DbSetup.GetSessionFactory().OpenSession();
        Session.BeginTransaction();
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        if (Session != null)
        {
            var currentTransaction = Session.GetCurrentTransaction();
            if (currentTransaction != null && currentTransaction.IsActive) currentTransaction.Rollback();
            Session.Dispose();
            Session = null;
        }

        GC.SuppressFinalize(this);
    }
}
