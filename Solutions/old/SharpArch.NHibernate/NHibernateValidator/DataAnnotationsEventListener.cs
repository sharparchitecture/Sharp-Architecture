namespace SharpArch.NHibernate.NHibernateValidator
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using global::NHibernate.Event;

    using Domain.DomainModel;
    using global::NHibernate;

    /// <summary>
    /// Performs entity validation using <see cref="Validator"/> class.
    /// </summary>
    [Serializable]
    internal class DataAnnotationsEventListener : IPreUpdateEventListener, IPreInsertEventListener
    {
        class SessionProvider : IServiceProvider
        {
            private readonly ISession session;

            public SessionProvider(ISession session)
            {
                this.session = session;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof (ISession))
                {
                    return session;
                }
                return null;
            }
        }

        public bool OnPreUpdate(PreUpdateEvent @event)
        {
            if (@event.Entity is ValidatableObject)
            {
                var entity = @event.Entity;
                Validator.ValidateObject(entity, CreateValidationContext(@event, entity), true);
            }
            return false;
        }

        private static ValidationContext CreateValidationContext(IDatabaseEventArgs @event, object entity)
        {
            return new ValidationContext(entity, new SessionProvider(@event.Session), null);
        }

        public bool OnPreInsert(PreInsertEvent @event)
        {
            if (@event.Entity is ValidatableObject)
            {
                var entity = @event.Entity;
                Validator.ValidateObject(entity, CreateValidationContext(@event, entity), true);
            }
            return false;
        }
    }
}
