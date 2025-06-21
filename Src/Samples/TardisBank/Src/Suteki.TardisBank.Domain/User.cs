// ReSharper disable MissingXmlDoc
namespace Suteki.TardisBank.Domain;

using Events;
using MediatR;
using SharpArch.Domain.DomainModel;


/// <summary>
///     User role constants.
/// </summary>
public static class UserRoles
{
    /// <summary>
    ///     User is child.
    /// </summary>
    public const string Child = "Child";

    /// <summary>
    ///     User is parent.
    /// </summary>
    public const string Parent = "Parent";
}


public abstract class User : Entity<int>
{
    public const int MaxMessages = 20;

    public virtual string Name { get; protected set; } = null!;
    public virtual string UserName { get; protected set; } = null!;
    public virtual string Password { get; protected set; } = null!;
    public virtual bool IsActive { get; protected set; }
    public virtual IList<Message> Messages { get; protected set; } = null!;

    protected User()
    {
    }

    protected User(string name, string userName, string password)
    {
        Name = name;
        UserName = userName;
        Password = password;
        Messages = new List<Message>();
        IsActive = false;
    }

    public virtual void SendMessage(string text, IMediator mediator)
    {
        if (mediator == null) throw new ArgumentNullException(nameof(mediator));

        Messages.Add(new Message(DateTime.Now.Date, text, this));
        RemoveOldMessages();

        mediator.Publish(new SendMessageEvent(this, text));
    }

    void RemoveOldMessages()
    {
        if (Messages.Count <= MaxMessages) return;

        var oldestMessage = Messages.First();
        Messages.Remove(oldestMessage);
    }

    public virtual void ReadMessage(int messageId)
    {
        var message = Messages.SingleOrDefault(x => x.Id == messageId);
        if (message == null)
        {
            throw new TardisBankException("No message with Id {0} found for user '{1}'", messageId, UserName);
        }

        message.Read();
    }

    public virtual void Activate()
    {
        IsActive = true;
    }

    public virtual void ResetPassword(string newPassword)
    {
        Password = newPassword;
    }

    /// <summary>
    ///     Returns user roles.
    /// </summary>
    /// <returns></returns>
    public virtual string[] GetRoles()
        => new string[0];
}
