namespace SharpArch.Web.Http
{
    using System;
    using System.Data;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using JetBrains.Annotations;
    using SharpArch.Domain.PersistenceSupport;

    /// <summary>
    ///     An attribute that implies a transaction.
    /// </summary>
    [PublicAPI]
    [BaseTypeRequired(typeof(IHttpController))]
    public sealed class TransactionAttribute : ActionFilterAttribute
    {
        /// <summary>
        ///     Transaction isolation level.
        /// </summary>
        /// <value>Defaults to <c>ReadCommitted</c>.</value>
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

        /// <summary>
        ///     Gets or sets a value indicating whether rollback transaction in case of model validation error.
        /// </summary>
        /// <value>
        ///     <c>true</c> if transaction must be rolled back in case of model validation error; otherwise, <c>false</c>.
        ///     Defaults to <c>true</c>.
        /// </value>
        public bool RollbackOnModelValidationError { get; set; } = true;

        /// <summary>
        ///     Occurs before the action method is invoked.
        ///     Begins transaction.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            ITransactionManager transactionManager = GetTransactionManager(actionContext.Request);
            transactionManager.BeginTransaction(IsolationLevel);
            base.OnActionExecuting(actionContext);
        }

        /// <summary>
        ///     Occurs after the action method is invoked.
        ///     Commits transaction if no error occurred.
        /// </summary>
        /// <param name="actionExecutedContext">The action context.</param>
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            ITransactionManager transactionManager = GetTransactionManager(actionExecutedContext.Request);

            if (actionExecutedContext.Exception != null || ShouldRollbackOnModelError(actionExecutedContext))
            {
                transactionManager.RollbackTransaction();
            }
            else
            {
                transactionManager.CommitTransaction();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ShouldRollbackOnModelError(HttpActionExecutedContext actionExecutedContext)
        {
            return RollbackOnModelValidationError && actionExecutedContext.ActionContext.ModelState.IsValid == false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [NotNull]
        private static ITransactionManager GetTransactionManager(HttpRequestMessage request)
        {
            var transactionManager =
                (ITransactionManager) request.GetDependencyScope().GetService(typeof(ITransactionManager));
            if (transactionManager == null)
                throw new InvalidOperationException(
                    "TransactionManager was null, register an implementation of TransactionManager in the IoC container.");
            
            return transactionManager;
        }
    }
}
