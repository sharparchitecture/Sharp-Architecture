namespace Suteki.TardisBank.Domain.Events;

using MediatR;


public class NewParentCreatedEvent : INotification
{
    public Parent Parent { get; }

    public NewParentCreatedEvent(Parent parent)
    {
        if (parent == null)
        {
            throw new ArgumentNullException("parent");
        }

        Parent = parent;
    }
}
