namespace SharpArch.Domain.PersistenceSupport
{
    using System;
    using JetBrains.Annotations;


    /// <summary>
    ///     Allows to select database associated with given object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Database identifier selection should be based on entity type, not instance.
    ///     </para>
    ///     <para>
    ///         Provider must handle proxy objects created by ORMs.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    public interface IDatabaseIdentifierProvider
    {
        /// <summary>
        ///     Gets the database identifier based on object instance.
        /// </summary>
        /// <param name="anObject">An object that may have an attribute used to determine the database identifier.</param>
        /// <returns>Database identifier or <seealso cref="DatabaseIdentifier.Default" /> if it was not applied to the instance.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="anObject" /> is <see langword="null" /></exception>
        /// <remarks>This is short-hand method </remarks>
        [NotNull]
        string GetFromInstance(object anObject);

        /// <summary>
        ///     Gets the database identifier based on object instance.
        /// </summary>
        /// <param name="type">Type of the object that may have an attribute used to determine the database identifier.</param>
        /// <returns>Database identifier or <c>null</c> if it cannot be resolved.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="type" /> is <see langword="null" /></exception>
        [NotNull]
        string GetFromType(Type type);
    }
}
