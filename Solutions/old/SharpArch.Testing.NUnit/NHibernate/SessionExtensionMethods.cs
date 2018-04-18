namespace SharpArch.Testing.NUnit.NHibernate
{
    using System;
    using global::NHibernate;
    using JetBrains.Annotations;

    /// <summary>
    /// NHibernate <see cref="ISession"/> extension methods.
    /// </summary>
    [PublicAPI]
    
    public static class SessionExtensionMethods
    {
        /// <summary>
        /// Flushes session and evict the object.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="instance">The instance.</param>
        public static void FlushAndEvict([NotNull] this ISession session, [NotNull] object instance)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            // Commits any changes up to this point to the database
            session.Flush();

            // Evicts the instance from the current session so that it can be loaded during testing;
            // this gives the test a clean slate, if you will, to work with
            session.Evict(instance);
        }
    }
}
