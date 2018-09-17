using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using SharpArch.AspNetCore;

namespace SharpArch.WebApi.Filters
{
    public class HandleTransactionFilter: ActionFilterAttribute
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<HandleTransactionFilter>();

        public HandleTransactionFilter()
        {
            Log.Debug("Created "+ GetHashCode().ToString("X"));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var policy = context.FindEffectivePolicy<TransactionAttribute>();
            Log.Information("Policy {@policy}", policy);
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }

    }


}
