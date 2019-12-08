namespace SharpArch.Web.AspNetCore.Transaction
{
    using System;
    using System.Threading.Tasks;
    using Domain.PersistenceSupport;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.DependencyInjection;


    /// <summary>
    ///     Wraps controller actions marked with <see cref="TransactionAttribute" /> into transaction.
    /// </summary>
    /// <remarks>
    ///     <see cref="ITransactionManager" /> must be registered in IoC in order for this to work.
    /// </remarks>
    [PublicAPI]
    public class AutoTransactionHandler : ApplyTransactionFilterBase, IAsyncActionFilter
    {
        /// <inheritdoc />
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var transactionAttribute = GetTransactionAttribute(context);
            ITransactionManager transactionManager = null;
            IDisposable transaction = null;
            if (transactionAttribute != null)
            {
                transactionManager = context.HttpContext.RequestServices.GetRequiredService<ITransactionManager>();
                transaction = transactionManager.BeginTransaction(transactionAttribute.IsolationLevel);
            }

            var executedContext = await next().ConfigureAwait(false);

            if (transaction != null)
            {
                using (transaction)
                {
                    if (executedContext.Exception != null ||
                        transactionAttribute.RollbackOnModelValidationError && context.ModelState.IsValid == false)
                    {
                        // don't use cancellation token to ensure transaction is rolled back on error
                        await transactionManager.RollbackTransactionAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        await transactionManager.CommitTransactionAsync(context.HttpContext.RequestAborted).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
