using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using SharpArch.AspNetCore;

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
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }

    }


}
