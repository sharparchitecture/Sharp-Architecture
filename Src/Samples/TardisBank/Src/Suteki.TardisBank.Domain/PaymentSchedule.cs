namespace Suteki.TardisBank.Domain;

using SharpArch.Domain.DomainModel;


public class PaymentSchedule : Entity<int>
{
    public virtual DateTime NextRun { get; protected set; }
    public virtual Interval Interval { get; protected set; }
    public virtual decimal Amount { get; protected set; }
    public virtual string? Description { get; protected set; }

    public virtual Account Account { get; protected set; } = null!;

    public PaymentSchedule(DateTime nextRun, Interval interval, decimal amount, string description, Account account)
    {
        NextRun = nextRun;
        Interval = interval;
        Amount = amount;
        Description = description;
        Account = account;
    }

    protected PaymentSchedule()
    {
    }

    public virtual void CalculateNextRunDate()
    {
        switch (Interval)
        {
            case Interval.Day:
                NextRun = NextRun.AddDays(1);
                break;
            case Interval.Week:
                NextRun = NextRun.AddDays(7);
                break;
            case Interval.Month:
                NextRun = NextRun.AddMonths(1);
                break;
        }
    }
}
