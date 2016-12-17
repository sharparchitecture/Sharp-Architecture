namespace SharpArch.Domain.Specifications
{
    using System.Linq;
    using JetBrains.Annotations;

    /// <summary>
    ///     Defines a contract for the behaviour of a LINQ Specification design pattern.
    /// </summary>
    /// <typeparam name="T">The type to be used for Input / Output.</typeparam>
    public interface ILinqSpecification<T>
    {
        /// <summary>
        ///     Returns the elements from the specified candidates that are satisfying the
        ///     specification.
        /// </summary>
        /// <param name="candidates">The candidates.</param>
        /// <returns>A list of satisfying elements.</returns>
        [NotNull]
        IQueryable<T> SatisfyingElementsFrom([NotNull] IQueryable<T> candidates);
    }
}