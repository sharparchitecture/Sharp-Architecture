using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SharpArch.AspNetCore;
using SharpArch.Domain.PersistenceSupport;

namespace SharpArch.WebApi.Filters
{
    public class HandleTransactionFilter: ActionFilterAttribute
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<HandleTransactionFilter>();

        private static ImmutableDictionary<string, TransactionAttribute> _attributeCache
            = ImmutableDictionary<string, TransactionAttribute>.Empty;
        private SpinLock _lock = new SpinLock(false);

        public HandleTransactionFilter()
        {
            Log.Debug("Created handler "+ GetHashCode().ToString("X"));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var actionId = context.ActionDescriptor.Id;
            if (!_attributeCache.TryGetValue(actionId, out var transactionAttribute))
            {
                bool lockTaken = false;
                _lock.Enter(ref lockTaken);
                try
                {
                    transactionAttribute = context.FindEffectivePolicy<TransactionAttribute>();
                    if (!_attributeCache.ContainsKey(actionId))
                        _attributeCache = _attributeCache.Add(actionId, transactionAttribute);
                }
                finally
                {
                    if (lockTaken)
                        _lock.Exit();
                }
            }
            Log.Information("Policy {@policy}", transactionAttribute);

            if (transactionAttribute != null)
            {

                var tm = context.HttpContext.RequestServices.GetRequiredService<ITransactionManager>();
                    context.HttpContext.Items["transaction-manager"] = tm;
                    tm.BeginTransaction(transactionAttribute.IsolationLevel);
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            var attr = _attributeCache[context.ActionDescriptor.Id];
            if (attr != null)
            {
                var transactionManager = (ITransactionManager)context.HttpContext.Items["transaction-manager"];

                if (context.Exception != null || context.ModelState.IsValid==false && attr.RollbackOnModelValidationError)
                    transactionManager.RollbackTransaction();
                else
                    transactionManager.CommitTransaction();
            }
        }

    }


}
