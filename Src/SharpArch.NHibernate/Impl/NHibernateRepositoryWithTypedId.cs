using JetBrains.Annotations;

namespace SharpArch.NHibernate.Impl
{
    /// <summary>
    ///     NHibernate repository implementation.
    /// </summary>
    /// <remarks>
    /// This implementation should be used in simplified, single-database model.
    /// </remarks>
    /// <typeparam name="TEntity">Entity type/</typeparam>
    /// <typeparam name="TId">Entity identifier type.</typeparam>
    [PublicAPI]
    public class NHibernateRepositoryWithTypedId<TEntity, TId> : NHibernateRepositoryWithTypedIdBase<TEntity, TId>
        where TEntity : class
    {
        /// <inheritdoc />
        public NHibernateRepositoryWithTypedId([NotNull] INHibernateTransactionManager transactionManager) : base(transactionManager)
        {
        }
    }
}
