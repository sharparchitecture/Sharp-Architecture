namespace Suteki.TardisBank.Tasks.EventHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using Domain.Events;
    using JetBrains.Annotations;
    using MediatR;


    [UsedImplicitly]
    public class SendMessageEmailHandler : INotificationHandler<SendMessageEvent>
    {
        private readonly IEmailService _emailService;

        public SendMessageEmailHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public Task Handle(SendMessageEvent sendMessageEvent, CancellationToken token)
        {
            if (sendMessageEvent == null)
            {
                throw new ArgumentNullException(nameof(sendMessageEvent));
            }

            if (sendMessageEvent.User is Child)
            {
                // we cannot send email messages to children.
                return Task.CompletedTask;
            }

            var toAddress = sendMessageEvent.User.UserName;
            const string subject = "Message from Tardis Bank";
            var body = sendMessageEvent.Message;

            _emailService.SendEmail(toAddress, subject, body);
            return Task.CompletedTask;
        }
    }
}
