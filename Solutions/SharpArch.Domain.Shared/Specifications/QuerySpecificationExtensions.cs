namespace SharpArch.Domain.Specifications
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides extension methods that extend the <see cref="QuerySpecification{T}"/> class.
    /// </summary>
    [PublicAPI]
    public static class QuerySpecificationExtensions
    {
        /// <summary>
        ///     Returns a specification that joins both specified specifications together using
        ///     the AND operator.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="specification1">The first specification.</param>
        /// <param name="specification2">The second specification.</param>
        /// <returns>A query specification.</returns>
        public static QuerySpecification<T> And<T>(this QuerySpecification<T> specification1, QuerySpecification<T> specification2)
        {
            var adhocSpec1 = new AdHoc<T>(specification1.MatchingCriteria);
            var adhocSpec2 = new AdHoc<T>(specification2.MatchingCriteria);

            InvocationExpression invokedExpr = Expression.Invoke(adhocSpec2.MatchingCriteria, adhocSpec1.MatchingCriteria.Parameters);
            Expression<Func<T, bool>> dynamicClause = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(adhocSpec1.MatchingCriteria.Body, invokedExpr), adhocSpec1.MatchingCriteria.Parameters);

            return new AdHoc<T>(dynamicClause);
        }

        /// <summary>
        ///     Returns a specification that joins both specified specifications together using
        ///     the OR operator.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="specification1">The first specification.</param>
        /// <param name="specification2">The second specification.</param>
        /// <returns>A query specification.</returns>
        public static QuerySpecification<T> Or<T>(this QuerySpecification<T> specification1, QuerySpecification<T> specification2)
        {
            var adhocSpec1 = new AdHoc<T>(specification1.MatchingCriteria);
            var adhocSpec2 = new AdHoc<T>(specification2.MatchingCriteria);

            InvocationExpression invokedExpr = Expression.Invoke(adhocSpec2.MatchingCriteria, adhocSpec1.MatchingCriteria.Parameters.Cast<Expression>());
            Expression<Func<T, bool>> dynamicClause = Expression.Lambda<Func<T, bool>>(Expression.OrElse(adhocSpec1.MatchingCriteria.Body, invokedExpr), adhocSpec1.MatchingCriteria.Parameters);

            return new AdHoc<T>(dynamicClause);
        }
    }
}