namespace SharpArch.Domain.Specifications
{
    using JetBrains.Annotations;

    /// <summary>
    ///     Defines the behaviour of a specification which is used to select specified items from a
    ///     collection
    /// </summary>
    /// <typeparam name="T">The type that the specification is applied to.</typeparam>
    [PublicAPI]
    public interface ISpecification<T>
    {
        /// <summary>
        ///     Allows multiple specifications to be joined together.
        /// </summary>
        /// <param name="specification">The specification to add.</param>
        /// <returns>A specification object containing original specification and new addition.</returns>
        ISpecification<T> And(ISpecification<T> specification);
        
        /// <summary>
        ///     Indicates whether the item provided satisfies the specification.
        /// </summary>
        /// <param name="item">The item to evaluate.</param>
        /// <returns>A value indicating whether the specification has be satisfied.</returns>
        bool IsSatisfiedBy(T item);

        /// <summary>
        ///     Checks whether the specification is more general than a given specification.
        /// </summary>
        /// <param name="specification">The specification to test.</param>
        /// <returns>A value indicating whether the current specification is a generalisation of the supplied specification.</returns>
        bool IsGeneralizationOf(ISpecification<T> specification);

        /// <summary>
        ///     Checks whether the specification is more specific than a given specification.
        /// </summary>
        /// <param name="specification">The specification to test.</param>
        /// <returns>A value indicating whether the current specification is a special case version of the supplied specification.</returns>
        bool IsSpecialCaseOf(ISpecification<T> specification);

        /// <summary>
        ///     Allows multiple specifications to be joined together, the specification must return <c>false</c>.
        /// </summary>
        /// <param name="specification">The specification to add.</param>
        /// <returns>A specification object containing original specification and new addition.</returns>
        ISpecification<T> Not(ISpecification<T> specification);

        /// <summary>
        ///     Allows multiple specifications to be joined together, the specification can return <c>true</c>.
        /// </summary>
        /// <param name="specification">The specification to add.</param>
        /// <returns>A specification object containing original specification and new addition.</returns>
        ISpecification<T> Or(ISpecification<T> specification);

        /// <summary>
        ///     Returns the specification representing the criteria that are not met by the candidate object.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>A value indicating whether the specification is unsatisfied by the specified item.</returns>
        ISpecification<T> RemainderUnsatisfiedBy(T item);
    }
}