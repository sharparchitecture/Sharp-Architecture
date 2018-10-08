using System;
using System.Data;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SharpArch.AspNetCore.Transaction
{
    //todo: rewrite
    /// <summary>
    ///     An attribute that used to indicate that action must be wrapped in transaction.
    ///     <para>
    ///         Attribute can be applied globally, at controller or at action level.
    ///     </para>
    ///     <para>
    ///         Note: This is marker attribute only, <see cref="UnitOfWorkHandler" /> must be added to filter s
    ///         collection in order to enable auto-transactions.
    ///     </para>
    ///     <para>
    ///         Transaction is either committed or rolled back after action execution is completed.
    ///         Note: accessing database from the View is considered as a bad practice.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     Transaction will be committed after action execution is completed and no unhandled exception occurred, see
    ///     <see cref="ActionExecutedContext.ExceptionHandled" />.
    ///     Transaction will be rolled back if there was unhandled exception in action or model validation was failed and
    ///     <see cref="RollbackOnModelValidationError" /> is <c>true</c>.
    /// </remarks>
    [BaseTypeRequired(typeof(ControllerBase))]
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class TransactionAttribute : Attribute, IFilterMetadata
    {
        /// <summary>
        ///     Gets or sets a value indicating whether rollback transaction in case of model validation error.
        /// </summary>
        /// <value>
        ///     <c>true</c> if transaction must be rolled back in case of model validation error; otherwise, <c>false</c>.
        ///     Defaults to <c>true</c>.
        /// </value>
        public bool RollbackOnModelValidationError { get; }

        /// <summary>
        ///     Transaction isolation level.
        /// </summary>
        /// <value>Defaults to <c>ReadCommitted</c>.</value>
        public IsolationLevel IsolationLevel { get; }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level.</param>
        /// <param name="rollbackOnModelValidationError">
        ///     indicates that transaction should be rolled back in case of
        ///     model validation error.
        /// </param>
        public TransactionAttribute(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, bool rollbackOnModelValidationError = true)
        {
            IsolationLevel = isolationLevel;
            RollbackOnModelValidationError = rollbackOnModelValidationError;
        }
    }


#if false
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
#endif
}
