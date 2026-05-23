namespace SharpArch.NHibernate;

using global::NHibernate;


/// <summary>
///     Base class for NHibernate query objects.
/// </summary>
[PublicAPI]
public abstract class NHibernateQuery
{
    /// <summary>
    ///     NHibernate <see cref="ISession" />.
    /// </summary>

    protected virtual ISession Session { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NHibernateQuery" /> class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="session" /> is <c>null</c>.</exception>
    protected NHibernateQuery(ISession session)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));

        Session = session;
    }
}
