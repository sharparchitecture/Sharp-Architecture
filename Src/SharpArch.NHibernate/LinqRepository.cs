namespace SharpArch.NHibernate
{
    using Domain.PersistenceSupport;
    using JetBrains.Annotations;


    /// <summary>
    ///     LINQ extensions to NHibernate repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="int" />
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.ILinqRepository{T}" />
    [PublicAPI]
    public class LinqRepository<T> : LinqRepositoryWithTypedId<T, int>, ILinqRepository<T>
        where T : class
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LinqRepository{T}" /> class.
        /// </summary>
        /// <param name="transactionManager">The transaction manager.</param>
        public LinqRepository(INHibernateTransactionManager transactionManager)
            : base(transactionManager)
        {
        }
    }
}
