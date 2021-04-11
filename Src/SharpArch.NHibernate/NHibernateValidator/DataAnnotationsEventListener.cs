namespace SharpArch.NHibernate.NHibernateValidator
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NHibernate.Event;

    using Domain.DomainModel;
    using global::NHibernate;

    /// <summary>
    /// Performs entity validation using <see cref="Validator"/> class.
    /// </summary>
    [Serializable]
    internal class DataAnnotationsEventListener : IPreUpdateEventListener, IPreInsertEventListener
    {
        private static readonly Task<bool> False = Task.FromResult(false);

        class SessionProvider : IServiceProvider
        {
            readonly ISession _session;

            public SessionProvider(ISession session)
            {
                _session = session;
            }

            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof (ISession))
                {
                    return _session;
                }
                return null;
            }
        }

        public Task<bool> OnPreUpdateAsync(PreUpdateEvent @event, CancellationToken cancellationToken)
        {
            OnPreUpdate(@event);
            return False;
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

        static ValidationContext CreateValidationContext(IDatabaseEventArgs @event, object entity)
        {
            return new ValidationContext(entity, new SessionProvider(@event.Session), null);
        }

        public Task<bool> OnPreInsertAsync(PreInsertEvent @event, CancellationToken cancellationToken)
        {
            OnPreInsert(@event);
            return False;
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
