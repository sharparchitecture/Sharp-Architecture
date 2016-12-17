namespace SharpArch.Domain.Specifications
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using JetBrains.Annotations;

    /// <summary>
    ///     Serves as the base class for query specifications.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    [PublicAPI]
    public abstract class QuerySpecification<T> : ILinqSpecification<T>
    {
        /// <summary>
        ///     Gets the matching criteria.
        /// </summary>
        public virtual Expression<Func<T, bool>> MatchingCriteria => null;

        /// <summary>
        ///     Returns the elements from the specified candidates that are satisfying the
        ///     specification.
        /// </summary>
        /// <param name="candidates">The candidates.</param>
        /// <returns>A list of satisfying elements.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="candidates"/> is <see langword="null" />.</exception>
        public virtual IQueryable<T> SatisfyingElementsFrom(IQueryable<T> candidates)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));
            if (this.MatchingCriteria != null)
            {
                return candidates.Where(this.MatchingCriteria).AsQueryable();
            }

            return candidates;
        }
    }
}