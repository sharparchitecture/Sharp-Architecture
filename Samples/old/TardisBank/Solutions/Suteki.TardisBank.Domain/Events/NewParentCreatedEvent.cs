namespace Suteki.TardisBank.Domain.Events
{
    using System;
    using MediatR;

    public class NewParentCreatedEvent : INotification
    {
        public NewParentCreatedEvent(Parent parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            Parent = parent;
        }

        public Parent Parent { get; private set; }
    }
}
