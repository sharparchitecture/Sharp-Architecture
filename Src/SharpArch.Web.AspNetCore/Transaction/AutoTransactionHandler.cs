namespace SharpArch.Web.AspNetCore.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.PersistenceSupport;
    using Infrastructure.Logging;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;


    /// <summary>
    ///     Wraps controller actions marked with <see cref="TransactionAttribute" /> into transaction.
    /// </summary>
    /// <remarks>
    ///     Must be scoped instance.
    ///     <see cref="ITransactionManager" /> must be registered in IoC in order for this to work.
    /// </remarks>
    [PublicAPI]
    public class AutoTransactionHandler : ApplyTransactionFilterBase, IAsyncActionFilter
    {
        static readonly ILog _log = LogProvider.For<AutoTransactionHandler>();

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException"><see cref="ITransactionManager" /> is not registered in container.</exception>
        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var transactionAttribute = GetTransactionAttribute(context);
            return transactionAttribute == null
                ? next()
                : WrapInTransaction(context, next, transactionAttribute);
        }

        static async Task WrapInTransaction(ActionExecutingContext context, ActionExecutionDelegate next, TransactionAttribute transactionAttribute)
        {
            var sessionRegistries = context.HttpContext.RequestServices.GetServices<ISessionRegistry>()?.ToArray() ?? Array.Empty<ISessionRegistry>();
            var options = context.HttpContext.RequestServices.GetService<IOptions<AutoTransactionHandlerOptions>>();
            if (options?.Value.EnsureSingleSessionForDatabase ?? false)
                EnsureOnlySingleRegistryHasSessionForDatabase(sessionRegistries, transactionAttribute);

            var transactionManagers = new KeyValuePair<string, ITransactionManager> [transactionAttribute.DatabaseIdentifiers.Count];
            try
            {
                for (var i = 0; i < transactionAttribute.DatabaseIdentifiers.Count; i++)
                {
                    var dbId = transactionAttribute.DatabaseIdentifiers[i];
                    var sessionRegistry = sessionRegistries.FirstOrDefault(x => x.ContainsDatabase(dbId))
                        ?? throw new DatabaseConfigurationException($"Cannot find session for database '{dbId}'.")
                        {
                            Data =
                            {
                                ["ActionRequired"] = "Please register database using SessionFactoryRegistry",
                                [DatabaseIdentifier.ParameterName] = dbId
                            }
                        };
                    var transactionManager = sessionRegistry.GetTransactionManager(dbId);
                    transactionManager.BeginTransaction(transactionAttribute.IsolationLevel);
                    transactionManagers[i] = new KeyValuePair<string, ITransactionManager>(dbId, transactionManager);
                }

                var executedContext = await next().ConfigureAwait(false);

                if (ShouldRollback(context, transactionAttribute, executedContext))
                    await RollbackTransactions(transactionManagers).ConfigureAwait(false);
                else
                    await CommitTransactions(transactionManagers, context.HttpContext.RequestAborted).ConfigureAwait(false);
            }
            catch
            {
                await RollbackTransactions(transactionManagers).ConfigureAwait(false);
                throw;
            }
        }

        static async Task RollbackTransactions(KeyValuePair<string, ITransactionManager>[] transactionManagers)
        {
            var rollbackTasks = new List<Task>(transactionManagers.Length);
            foreach (var (databaseIdentifier, transactionManager) in transactionManagers)
            {
                switch (transactionManager)
                {
                    case null:
                        _log.WarnFormat("Cannot rollback changes - TransactionManager for {DatabaseIdentifier} was not initialized",
                            databaseIdentifier);
                        continue;
                    case ISupportsTransactionStatus tranStatus when !tranStatus.IsActive:
                        _log.InfoFormat("Nothing to rollback - Transaction for {DatabaseIdentifier} is not active", databaseIdentifier);
                        continue;
                    default:
                        var rollbackTask = transactionManager.RollbackTransactionAsync()
                            .ContinueWith((rollback, dbId) => { rollback.Exception.Data[DatabaseIdentifier.ParameterName] = (string) dbId; },
                                databaseIdentifier, CancellationToken.None);
                        rollbackTasks.Add(rollbackTask);
                        break;
                }
            }

            if (rollbackTasks.Count == 0) return;
            try
            {
                await Task.WhenAll(rollbackTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // do not throw exceptions related to rollback failures, we need to keep original exception
                _log.Error(ex, "Failed to rollback transaction");
            }
        }

        static bool ShouldRollback(ActionExecutingContext context, TransactionAttribute transactionAttribute, ActionExecutedContext executedContext)
            => executedContext.Exception != null ||
                transactionAttribute.RollbackOnModelValidationError && context.ModelState.IsValid == false;

        static async Task CommitTransactions(KeyValuePair<string, ITransactionManager>[] transactionManagers, CancellationToken cancellationToken)
        {
            foreach (var (databaseIdentifier, transactionManager) in transactionManagers)
            {
                switch (transactionManager)
                {
                    case null:
                        _log.WarnFormat("Cannot commit changes - TransactionManager for {DatabaseIdentifier} was not initialized",
                            databaseIdentifier);
                        continue;
                    case ISupportsTransactionStatus tranStatus when !tranStatus.IsActive:
                        _log.InfoFormat("Nothing to commit - Transaction for {DatabaseIdentifier} is not active", databaseIdentifier);
                        continue;
                    default:
                        try
                        {
                            await transactionManager.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            ex.Data[DatabaseIdentifier.ParameterName] = databaseIdentifier;
                            throw;
                        }

                        break;
                }
            }
        }

        static void EnsureOnlySingleRegistryHasSessionForDatabase(
            ISessionRegistry[] sessionRegistries, TransactionAttribute transactionAttribute)
        {
            var registeredIn = new List<ISessionRegistry>(sessionRegistries.Length);
            List<KeyValuePair<string, string>> errors = null;
            StringBuilder errorMessage = null;
            foreach (var databaseIdentifier in transactionAttribute.DatabaseIdentifiers)
            {
                registeredIn.Clear();
                foreach (var sessionRegistry in sessionRegistries)
                    if (sessionRegistry.ContainsDatabase(databaseIdentifier))
                        registeredIn.Add(sessionRegistry);

                if (registeredIn.Count == 0)
                {
                    (errors ??= new List<KeyValuePair<string, string>>(transactionAttribute.DatabaseIdentifiers.Count))
                        .Add(new KeyValuePair<string, string>(databaseIdentifier,
                            "No Session Registry found for database. Add database registration to Session Registry."));
                }
                else if (registeredIn.Count > 1)
                {
                    errorMessage ??= new StringBuilder(256);
                    errorMessage.Append("Multiple Session registries found for database: ");
                    errorMessage.AppendJoin(",", registeredIn.Select(x => x.GetType().FullName));
                    (errors ??= new List<KeyValuePair<string, string>>(transactionAttribute.DatabaseIdentifiers.Count))
                        .Add(new KeyValuePair<string, string>(databaseIdentifier, errorMessage.ToString()));
                    errorMessage.AppendFormat(" - Make sure only one Session Registry contains definition for database '{0}'", databaseIdentifier);
                    errorMessage.Clear();
                }
            }

            if (errors != null)
                throw new InvalidOperationException("Invalid Session Registry configuration")
                {
                    Data = {["Errors"] = errors}
                };
        }
    }
}
