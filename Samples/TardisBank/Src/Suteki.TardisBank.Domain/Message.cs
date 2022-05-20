namespace Suteki.TardisBank.Domain;

using SharpArch.Domain.DomainModel;


public class Message : Entity<int>
{
    public virtual DateTime Date { get; protected set; }
    public virtual string? Text { get; protected set; }

    public virtual User User { get; set; } = null!;

    public virtual bool HasBeenRead { get; protected set; }

    public Message(DateTime date, string text, User user)
    {
        Date = date;
        Text = text;
        User = user;
        HasBeenRead = false;
    }

    protected Message()
    {
    }

    public virtual void Read()
    {
        HasBeenRead = true;
    }
}
