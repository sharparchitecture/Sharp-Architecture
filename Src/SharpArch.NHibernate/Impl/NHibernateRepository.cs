namespace SharpArch.NHibernate.Impl
{
    using System;
    using Domain.DomainModel;
    using Domain.PersistenceSupport;
    using JetBrains.Annotations;
    using MultiDb;


    /// <summary>
    ///     NHibernate repository implementation.
    /// </summary>
    /// <remarks>
    ///     This implementation should be used in simplified, single-database model.
    /// </remarks>
    /// <typeparam name="TEntity">Entity type/</typeparam>
    /// <typeparam name="TId">Entity identifier type.</typeparam>
    [PublicAPI]
    public class NHibernateRepository<TEntity, TId> : NHibernateRepositoryBase<TEntity, TId>
        where TEntity : class, IEntity<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NHibernateRepository{TEntity,TId}" /> class.
        /// </summary>
        /// <param name="transactionManager">The transaction manager.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="transactionManager" /> is <c>null</c>.</exception>
        protected NHibernateRepository([NotNull] INHibernateTransactionManager transactionManager)
            : base(transactionManager)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="NHibernateRepository{TEntity,TId}" /> class using given transaction
        ///     manager.
        /// </summary>
        /// <param name="sessionRegistry">Session registry to retrieve <see cref="INHibernateTransactionManager" /> from.</param>
        /// <param name="databaseIdentifierProvider">Database identifier resolver.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="sessionRegistry" /> or <paramref name="databaseIdentifierProvider" />
        ///     is <c>null</c>.
        /// </exception>
        public NHibernateRepository(
            [NotNull] INHibernateSessionRegistry sessionRegistry, [NotNull] IDatabaseIdentifierProvider databaseIdentifierProvider)
            : base(GetTransactionManager(sessionRegistry, databaseIdentifierProvider))
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="NHibernateRepository{TEntity,TId}" /> class using given transaction
        ///     manager.
        /// </summary>
        /// <param name="transactionManager">The transaction manager.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="transactionManager" /> is <c>null</c>.</exception>
        public static NHibernateRepository<TEntity, TId> WithTransactionManager([NotNull] INHibernateTransactionManager transactionManager)
        {
            if (transactionManager == null) throw new ArgumentNullException(nameof(transactionManager));
            return new NHibernateRepository<TEntity, TId>(transactionManager);
        }

        static INHibernateTransactionManager GetTransactionManager(
            [NotNull] INHibernateSessionRegistry sessionRegistry, [NotNull] IDatabaseIdentifierProvider keyProvider)
        {
            if (sessionRegistry == null) throw new ArgumentNullException(nameof(sessionRegistry));
            if (keyProvider == null) throw new ArgumentNullException(nameof(keyProvider));

            return sessionRegistry.GetNHibernateTransactionManager(keyProvider.GetFromType(typeof(TEntity)));
        }
    }
}
