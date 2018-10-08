using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SharpArch.Domain.PersistenceSupport;

namespace SharpArch.AspNetCore.Transaction
{
    /// <summary>
    ///     Wraps controller actions marked with <see cref="TransactionAttribute" /> into transaction.
    /// </summary>
    /// <remarks>
    ///     <see cref="ITransactionManager" /> must be registered in IoC in order for this to work.
    /// </remarks>
    [PublicAPI]
    public class UnitOfWorkHandler : ApplyTransactionFilterBase
    {
        /// <summary>
        ///     HttpContext key for Transaction Manger.
        /// </summary>
        private const string TransactionManagerKey = "SharpArch.AspNetCore.UnitOfWork.TransactionManager";

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var transactionAttribute = GetTransactionAttribute(context);
            if (transactionAttribute != null)
            {
                var tm = context.HttpContext.RequestServices.GetRequiredService<ITransactionManager>();
                context.HttpContext.Items[TransactionManagerKey] = tm;
                tm.BeginTransaction(transactionAttribute.IsolationLevel);
            }
        }

        /// <inheritdoc />
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var transactionAttribute = GetTransactionAttribute(context);
            if (transactionAttribute != null)
            {
                var transactionManager = (ITransactionManager) context.HttpContext.Items[TransactionManagerKey];
                if (transactionManager == null)
                    throw new InvalidOperationException(nameof(ITransactionManager) +
                        " was not found in HttpContext. Please contact SharpArch dev team.");

                if (context.Exception != null || transactionAttribute.RollbackOnModelValidationError && context.ModelState.IsValid == false)
                    transactionManager.RollbackTransaction();
                else
                    transactionManager.CommitTransaction();
            }
        }
    }
}
