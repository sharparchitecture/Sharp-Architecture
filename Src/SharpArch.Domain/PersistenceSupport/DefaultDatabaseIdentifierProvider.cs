namespace SharpArch.Domain.PersistenceSupport
{
    using System;


    /// <summary>
    ///     Default provider, which returns <see cref="DatabaseIdentifier.Default" /> for all entities.
    /// </summary>
    /// <remarks>Use with single database configuration.</remarks>
    public class DefaultDatabaseIdentifierProvider : IDatabaseIdentifierProvider
    {
        /// <summary>
        ///     Instance.
        /// </summary>
        public static IDatabaseIdentifierProvider Instance { get; } = new DefaultDatabaseIdentifierProvider();

        DefaultDatabaseIdentifierProvider()
        {
        }

        /// <inheritdoc />
        public string GetFromInstance(object anObject)
            => DatabaseIdentifier.Default;

        /// <inheritdoc />
        public string GetFromType(Type type)
            => DatabaseIdentifier.Default;
    }
}
