namespace SharpArch.Web.AspNetCore.Transaction
{
    using System.Collections.Immutable;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Mvc.Filters;


    /// <summary>
    ///     Base <see cref="TransactionAttribute" /> handler.
    ///     Caches transaction attribute associated with action.
    /// </summary>
    [PublicAPI]
    public abstract class ApplyTransactionFilterBase 
    {
        static readonly object _lock = new object();

        static ImmutableDictionary<string, TransactionAttribute> _attributeCache
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
                lock(_lock)
                {
                    transactionAttribute = context.FindEffectivePolicy<TransactionAttribute>();
                    if (!_attributeCache.ContainsKey(actionId))
                        _attributeCache = _attributeCache.Add(actionId, transactionAttribute);
                }
            }

            return transactionAttribute;
        }
    }
}
