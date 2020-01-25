using System;
using JetBrains.Annotations;
using NHibernate;

namespace SharpArch.NHibernate.Impl
{
    /// <summary>
    /// Base class for NHibernate query objects.
    /// </summary>
    [PublicAPI]
    public abstract class NHibernateQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateQuery"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="session"/> is <c>null</c>.</exception>
        protected NHibernateQuery([NotNull] ISession session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }

        /// <summary>
        /// NHibernate <see cref="ISession"/>.
        /// </summary>
        [NotNull]
        protected virtual ISession Session { get; }
    }
}
