namespace DomainLayer.Entities
{
    using System;
    using Persistence;
    using SharpArch.Domain.DomainModel;
    using SharpArch.Domain.PersistenceSupport.SharpArch.NHibernate;


    [UseDatabase(Databases.Log)]
    public class LogMessage : Entity<int>
    {
        public virtual DateTime Timestamp { get; set; }

        public virtual string Message { get; set; }

        /// <inheritdoc />
        public LogMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
            Message = message;
            Timestamp = DateTime.UtcNow;
        }
    }
}
