namespace SharpArch.Web.AspNetCore.Transaction;

using Microsoft.AspNetCore.Mvc.Filters;


/// <summary>
///     Base <see cref="TransactionAttribute" /> handler.
///     Caches transaction attribute associated with action.
/// </summary>
[PublicAPI]
public abstract class ApplyTransactionFilterBase
{
    static readonly object _lock = new();

    static IReadOnlyDictionary<string, TransactionAttribute?> _attributeCache
        = new Dictionary<string, TransactionAttribute?>(0, StringComparer.Ordinal);

    /// <summary>
    ///     Returns <see cref="TransactionAttribute" /> associated with given action.
    /// </summary>
    /// <param name="context"></param>
    /// <returns><see cref="TransactionAttribute" /> instance or <c>null</c>.</returns>
    protected TransactionAttribute? GetTransactionAttribute(FilterContext context)
    {
        var actionId = context.ActionDescriptor.Id;
        if (!_attributeCache.TryGetValue(actionId, out var transactionAttribute))
        {
            lock (_lock)
            {
                transactionAttribute = context.FindEffectivePolicy<TransactionAttribute>();
                var cache = _attributeCache;
                if (!cache.ContainsKey(actionId))
                {
                    cache = new Dictionary<string, TransactionAttribute?>((Dictionary<string, TransactionAttribute?>)cache, StringComparer.Ordinal)
                    {
                        { actionId, transactionAttribute }
                    };
                    _attributeCache = cache;
                }
            }
        }

        return transactionAttribute;
    }
}
