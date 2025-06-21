namespace Suteki.TardisBank.Tasks.EventHandlers;

using Domain;
using Domain.Events;
using MediatR;


[UsedImplicitly]
public class SendRegistrationEmailHandler : INotificationHandler<NewParentCreatedEvent>
{
    const string EmailBodyTemplate =
        @"Please click here to activate your account: {0} (or copy and Paste this URL into your browser)";

    readonly IEmailService _emailService;

    public SendRegistrationEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <inheritdoc />
    public Task Handle(NewParentCreatedEvent newParentCreatedEvent, CancellationToken cancellationToken)
    {
        if (newParentCreatedEvent == null)
        {
            throw new ArgumentNullException(nameof(newParentCreatedEvent));
        }

        if (string.IsNullOrWhiteSpace(newParentCreatedEvent.Parent.ActivationKey))
        {
            throw new TardisBankException("Parent does not have an activation key");
        }

        var toAddress = newParentCreatedEvent.Parent.UserName;
        var subject = "Welcome to Tardis Bank";
        var url = "http://tardisbank.com/User/Activate/" + newParentCreatedEvent.Parent.ActivationKey;
        var body = string.Format(EmailBodyTemplate, url);

        _emailService.SendEmail(toAddress, subject, body);
        return Task.CompletedTask;
    }
}
