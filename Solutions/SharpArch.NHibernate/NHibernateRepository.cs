namespace SharpArch.NHibernate
{
    using JetBrains.Annotations;
    using SharpArch.NHibernate.Contracts.Repositories;


    /// <summary>
    ///     Since nearly all of the domain objects you create will have a type of int Id, this
    ///     most frequently used base NHibernateRepository leverages this assumption.  If you want
    ///     an entity with a type other than int, such as string, then use
    ///     <see cref="NHibernateRepositoryWithTypedId{T, IdT}" />.
    /// </summary>
    [PublicAPI]
    public class NHibernateRepository<T> : NHibernateRepositoryWithTypedId<T, int>, INHibernateRepository<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NHibernateRepository{T}" /> class.
        /// </summary>
        /// <param name="transactionManager">The transaction manager.</param>
        public NHibernateRepository(INHibernateTransactionManager transactionManager) : base(transactionManager)
        { }
    }
}
