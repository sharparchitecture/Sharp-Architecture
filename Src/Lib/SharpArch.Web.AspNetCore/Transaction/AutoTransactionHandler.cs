namespace SharpArch.Web.AspNetCore.Transaction
{
    using System.Threading.Tasks;
    using Domain.PersistenceSupport;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;


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
        /// <exception cref="T:System.InvalidOperationException"><see cref="ITransactionManager" /> is not registered in container.</exception>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var transactionAttribute = GetTransactionAttribute(context);
            ITransactionManager? transactionManager = null;
            if (transactionAttribute != null)
            {
                transactionManager = context.HttpContext.RequestServices.GetRequiredService<ITransactionManager>();
                transactionManager.BeginTransaction(transactionAttribute.IsolationLevel);
            }

            var executedContext = await next().ConfigureAwait(false);

            if (transactionManager is ISupportsTransactionStatus tranStatus)
            {
                if (!tranStatus.IsActive)
                {
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<AutoTransactionHandler>>();
                    logger?.LogDebug("Transaction is already closed");
                    return;
                }

                if (executedContext.Exception != null ||
                    transactionAttribute!.RollbackOnModelValidationError && context.ModelState.IsValid == false)
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
