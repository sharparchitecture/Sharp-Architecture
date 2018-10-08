using System.Collections.Immutable;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SharpArch.AspNetCore.Transaction
{
    /// <summary>
    ///     Base <see cref="TransactionAttribute" /> handler.
    ///     Caches transaction attribute associated with action.
    /// </summary>
    [PublicAPI]
    public abstract class ApplyTransactionFilterBase : ActionFilterAttribute
    {
        private SpinLock _lock = new SpinLock(false);

        private static ImmutableDictionary<string, TransactionAttribute> _attributeCache
            = ImmutableDictionary<string, TransactionAttribute>.Empty;

        /// <summary>
        ///     Returns <see cref="TransactionAttribute" /> associated with given action.
        /// </summary>
        /// <param name="context"></param>
        /// <returns><see cref="TransactionAttribute" /> instance or <c>null</c>.</returns>
        protected TransactionAttribute GetTransactionAttribute(FilterContext context)
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

            return transactionAttribute;
        }
    }
}
