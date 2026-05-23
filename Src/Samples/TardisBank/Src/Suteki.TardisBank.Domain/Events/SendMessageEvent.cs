namespace Suteki.TardisBank.Domain.Events;

using MediatR;


public class SendMessageEvent : INotification
{
    public User User { get; }
    public string Message { get; }

    public SendMessageEvent(User user, string message)
    {
        if (user == null)
        {
            throw new ArgumentNullException("user");
        }

        if (message == null)
        {
            throw new ArgumentNullException("message");
        }

        User = user;
        Message = message;
    }
}
