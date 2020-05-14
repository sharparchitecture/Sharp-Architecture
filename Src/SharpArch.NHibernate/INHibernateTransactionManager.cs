namespace SharpArch.NHibernate
{
    using Domain.PersistenceSupport;
    using global::NHibernate;


    /// <summary>
    ///     NHibernate transaction support.
    /// </summary>
    public interface INHibernateTransactionManager : ITransactionManager, ISupportsFlushChanges
    {
        /// <summary>
        ///     Returns NHibernate session.
        /// </summary>
        ISession Session { get; }
    }
}
