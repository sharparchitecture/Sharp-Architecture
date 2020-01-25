namespace SharpArch.Domain.PersistenceSupport
{
    namespace SharpArch.NHibernate
    {
        using System;
        using JetBrains.Annotations;


        /// <summary>
        ///     Provides the ability to decorate repositories with an attribute defining the factory key
        ///     for the given repository; accordingly, the respective connection factory will be used to
        ///     communicate with the database.  This allows you to declare different repositories to
        ///     communicate with different databases.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        public sealed class UseDatabaseAttribute : Attribute
        {
            /// <summary>
            ///     Session factory key.
            /// </summary>
            public string DatabaseIdentifier { get; }

            /// <summary>
            ///     Creates instance of the attribute.
            /// </summary>
            /// <param name="databaseIdentifier">Session factory key.</param>
            /// <exception cref="T:System.ArgumentException">When <paramref name="databaseIdentifier" />is <c>null</c> or whitespace.</exception>
            public UseDatabaseAttribute([NotNull] string databaseIdentifier)
            {
                if (string.IsNullOrWhiteSpace(databaseIdentifier))
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(databaseIdentifier));
                DatabaseIdentifier = databaseIdentifier;
            }
        }
    }
}
