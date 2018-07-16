namespace Suteki.TardisBank.Domain.Events
{
    using System;
    using MediatR;

    public class SendMessageEvent : INotification
    {
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

        public User User { get; private set; }
        public string Message { get; private set; }
    }
}
