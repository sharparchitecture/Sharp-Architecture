namespace SharpArch.NHibernate
{
    using System;
    using global::NHibernate;
    using JetBrains.Annotations;


    /// <summary>
    ///     NHibernate <see cref="ISession" /> extension methods.
    /// </summary>
    [PublicAPI]
    public static class SessionExtensions
    {
        /// <summary>
        ///     Flushes session and evict entity from the session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="entity">The entity.</param>
        public static void FlushAndEvict([NotNull] this ISession session, [NotNull] object entity)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            // Commits any changes up to this point to the database
            session.Flush();

            // Evicts the entity from the current session so that it can be loaded during testing;
            // this gives the test a clean slate, if you will, to work with
            session.Evict(entity);
        }
    }
}
