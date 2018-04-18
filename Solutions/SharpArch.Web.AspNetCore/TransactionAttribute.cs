namespace SharpArch.AspNetCore
{
    using System;
    using System.Data;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using SharpArch.Domain.PersistenceSupport;

    //todo: rewrite

    /// <summary>
    ///     An attribute that implies a transaction per MVC action.
    ///     <para>
    ///         Transaction is either committed or rolled back after action execution is completed. See
    ///         <see cref="OnActionExecuted" />.
    ///         Note: accessing database from the View is considered as a bad practice.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     Transaction will be committed after action execution is completed and no unhandled exception occurred, see
    ///     <see cref="ActionExecutedContext.ExceptionHandled" />.
    ///     Transaction will be rolled back if there was unhandled exception in action or model validation was failed and
    ///     <see cref="RollbackOnModelValidationError" /> is <c>true</c>.
    /// </remarks>
    [PublicAPI]
    [BaseTypeRequired(typeof(ControllerBase))]
    public sealed class TransactionAttribute : ActionFilterAttribute
    {
        /// <summary>
        ///     Transaction Manager reference.
        /// </summary>
        /// <remarks>
        ///     The value should be injected by the filter provider.
        /// </remarks>
        public ITransactionManager TransactionManager { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether rollback transaction in case of model validation error.
        /// </summary>
        /// <value>
        ///     <c>true</c> if transaction must be rolled back in case of model validation error; otherwise, <c>false</c>.
        ///     Defaults to <c>true</c>.
        /// </value>
        public bool RollbackOnModelValidationError { get; set; } = true;

        /// <summary>
        ///     Transaction isolation level.
        /// </summary>
        /// <value>Defaults to <c>ReadCommitted</c>.</value>
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

        /// <summary>
        ///     Ends transaction.
        /// </summary>
        /// <param name="filterContext">Action execution context.</param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (HasUnhandledException(filterContext) || ShouldRollbackOnModelError(filterContext))
                TransactionManager.RollbackTransaction();
            else
                TransactionManager.CommitTransaction();
        }

        /// <summary>
        ///     Starts transaction.
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            throw new NotImplementedException("Needs rewrite");
            if (TransactionManager == null)
                throw new InvalidOperationException(
                    "TransactionManager was null, make sure implementation of TransactionManager is registered in the IoC container.");

            TransactionManager.BeginTransaction(IsolationLevel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasUnhandledException(ActionExecutedContext filterContext)
        {
            return filterContext.Exception != null && !filterContext.ExceptionHandled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ShouldRollbackOnModelError(FilterContext filterContext)
        {
            return RollbackOnModelValidationError && filterContext.ModelState.IsValid == false;
        }
    }
}
