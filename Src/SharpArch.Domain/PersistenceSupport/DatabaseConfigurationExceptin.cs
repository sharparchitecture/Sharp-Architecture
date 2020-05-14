namespace SharpArch.Domain.PersistenceSupport
{
    using System;
    using System.Runtime.Serialization;
    using JetBrains.Annotations;


    /// <summary>
    ///     Communicates configuration error.
    /// </summary>
    [PublicAPI]
    public class DatabaseConfigurationException : Exception
    {
        /// <inheritdoc />
        public DatabaseConfigurationException()
        {
        }

        /// <inheritdoc />
        protected DatabaseConfigurationException([NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <inheritdoc />
        public DatabaseConfigurationException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public DatabaseConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
