namespace Suteki.TardisBank.Domain;

using SharpArch.Domain.DomainModel;


public class Transaction : Entity<int>
{
    public virtual string? Description { get; protected set; }
    public virtual decimal Amount { get; protected set; }

    public virtual Account Account { get; protected set; } = null!;

    public virtual DateTime Date { get; protected set; }

    public Transaction(string? description, decimal amount, Account account)
    {
        Description = description;
        Amount = amount;
        Account = account;
        Date = DateTime.Now.Date;
    }

    protected Transaction()
    {
    }
}
