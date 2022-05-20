namespace SharpArch.Domain.Specifications
{
    using System;
    using System.Linq.Expressions;
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides extension methods that extend the <see cref="QuerySpecification{T}" /> class.
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
        public static QuerySpecification<T> And<T>(this QuerySpecification<T> specification1,
            QuerySpecification<T> specification2)
        {
            if (specification1 == null) throw new ArgumentNullException(nameof(specification1));
            if (specification2 == null) throw new ArgumentNullException(nameof(specification2));
            InvocationExpression invokedExpr =
                Expression.Invoke(specification2.MatchingCriteria!, specification1.MatchingCriteria!.Parameters);
            Expression<Func<T, bool>> dynamicClause = Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(specification1.MatchingCriteria!.Body, invokedExpr),
                specification1.MatchingCriteria!.Parameters);

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
        public static QuerySpecification<T> Or<T>(this QuerySpecification<T> specification1,
            QuerySpecification<T> specification2)
        {
            InvocationExpression invokedExpr = Expression.Invoke(specification2.MatchingCriteria!,
                specification1.MatchingCriteria!.Parameters);
            Expression<Func<T, bool>> dynamicClause = Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(specification1.MatchingCriteria!.Body, invokedExpr),
                specification1.MatchingCriteria!.Parameters);

            return new AdHoc<T>(dynamicClause);
        }
    }
}
