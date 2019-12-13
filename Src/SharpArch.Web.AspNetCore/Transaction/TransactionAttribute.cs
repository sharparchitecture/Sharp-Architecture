namespace SharpArch.Web.AspNetCore.Transaction
{
    using System;
    using System.Data;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Mvc; 
    using Microsoft.AspNetCore.Mvc.Filters;


    /// <summary>
    ///     An attribute that used to indicate that action must be wrapped in transaction.
    ///     <para>
    ///         Attribute can be applied globally, at controller or at action level.
    ///     </para>
    ///     <para>
    ///         Note: This is marker attribute only, <see cref="AutoTransactionHandler" /> must be added to filter s
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
}
