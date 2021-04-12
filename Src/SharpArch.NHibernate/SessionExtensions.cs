namespace SharpArch.NHibernate
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NHibernate;
    using global::NHibernate.Engine;
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
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="session" /> or <paramref name="entity" /> is
        ///     <c>null</c>.
        /// </exception>
        public static void FlushAndEvict(this ISession session, object entity)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            // Commits any changes up to this point to the database
            session.Flush();

            // Evicts the entity from the current session so that it can be loaded during testing;
            // this gives the test a clean slate, if you will, to work with
            session.Evict(entity);
        }

        /// <summary>
        ///     Flushes session and evict entity from the session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="session" /> or <paramref name="entity" /> is <c>null</c>.
        /// </exception>
        public static async Task FlushAndEvictAsync(this ISession session, object entity, CancellationToken cancellationToken)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            // Commits any changes up to this point to the database
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);

            // Evicts the entity from the current session so that it can be loaded during testing;
            // this gives the test a clean slate, if you will, to work with
            await session.EvictAsync(entity, cancellationToken);
        }

        /// <summary>
        ///     Force version increment for versioned entity.
        /// </summary>
        /// <param name="session">
        ///     <see cref="ISession" />
        /// </param>
        /// <param name="entity">Entity to increment version for.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ArgumentNullException"><paramref name="session" /> is <c>null</c>.</exception>
        public static Task IncrementVersionAsync(this ISession session, object? entity, CancellationToken cancellationToken)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (entity == null) return Task.CompletedTask;

            // don't process deleted entity.
            var entry = session.GetSessionImplementation().PersistenceContext.GetEntry(entity);
            if (entry == null || entry.Status == Status.Deleted || entry.Status == Status.Gone) return Task.CompletedTask;
            return session.LockAsync(entity, LockMode.Force, cancellationToken);
        }

        /// <summary>
        ///     Force version increment for versioned entity.
        /// </summary>
        /// <param name="session">
        ///     <see cref="ISession" />
        /// </param>
        /// <param name="entity">Entity to increment version for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="session" /> is <c>null</c>.</exception>
        public static void IncrementVersion(this ISession session, object? entity)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (entity == null) return;

            // ignore deleted entity
            var entry = session.GetSessionImplementation().PersistenceContext.GetEntry(entity);
            if (entry == null || entry.Status == Status.Deleted || entry.Status == Status.Gone) return;
            session.Lock(entity, LockMode.Force);
        }

        /// <summary>
        ///     Checks whether entity is modified.
        /// </summary>
        /// <param name="session">
        ///     <see cref="ISession" />
        /// </param>
        /// <param name="entity"></param>
        /// <returns><c>true</c> id entity was modified.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="entity" /> or <paramref name="session" /> is <c>null</c>.
        /// </exception>
        public static bool IsModified(this ISession session, object entity)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var sessionImplementation = session.GetSessionImplementation();
            var entry = sessionImplementation.PersistenceContext.GetEntry(entity);
            if (entry == null) return false;

            var persister = entry.Persister;
            if (entry.Status == Status.Deleted) return true;
            if (!entry.RequiresDirtyCheck(entity)) return false;

            var currentValue = persister.GetPropertyValues(entity);
            var dirtyPropertyIndexes = persister.FindDirty(currentValue, entry.LoadedState, entity, sessionImplementation);
            return dirtyPropertyIndexes != null;
        }
    }
}
