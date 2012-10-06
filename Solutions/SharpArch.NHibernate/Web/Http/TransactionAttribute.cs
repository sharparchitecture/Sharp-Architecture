namespace SharpArch.NHibernate.Web.Http
{
    using System.Data;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    /// <summary>
    /// An attribute that implies a transaction.
    /// </summary>
    public class TransactionAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Optionally holds the factory key to be used when beginning/committing a transaction.
        /// </summary>
        private readonly string factoryKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAttribute" /> class.
        /// </summary>
        public TransactionAttribute()
            : base()
        {
            IsolationLevel = IsolationLevel.Unspecified;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAttribute" /> class.
        /// </summary>
        /// <param name="factoryKey">The factory key.</param>
        public TransactionAttribute(string factoryKey)
            : base()
        {
            this.factoryKey = factoryKey;
        }

        /// <summary>
        /// Gets or sets the isolation level.
        /// </summary>
        /// <value>The isolation level.</value>
        public IsolationLevel IsolationLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            global::NHibernate.ISession session = NHibernateSession.CurrentFor(this.GetEffectiveFactoryKey());

            if (this.IsolationLevel != IsolationLevel.Unspecified)
            {
                session.BeginTransaction(this.IsolationLevel);
            }
            else
            {
                session.BeginTransaction();
            }
        }

        /// <summary>
        /// Occurs after the action method is invoked.
        /// </summary>
        /// <param name="actionExecutedContext">The action executed context.</param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            string effectiveFactoryKey = this.GetEffectiveFactoryKey();
            global::NHibernate.ITransaction currentTransaction = NHibernateSession.CurrentFor(effectiveFactoryKey).Transaction;

            try
            {
                if (currentTransaction.IsActive)
                {
                    if (actionExecutedContext.Exception != null)
                    {
                        currentTransaction.Rollback();
                    }
                    else
                    {
                        currentTransaction.Commit();
                    }
                }
            }
            finally
            {
                currentTransaction.Dispose();
            }
        }

        /// <summary>
        /// Gets the effective factory key.
        /// </summary>
        /// <returns>The effective factory key.</returns>
        private string GetEffectiveFactoryKey()
        {
            return string.IsNullOrEmpty(this.factoryKey) ? SessionFactoryKeyHelper.GetKey() : this.factoryKey;
        }
    }
}
