// ReSharper disable MissingXmlDoc
namespace Suteki.TardisBank.Domain;

using SharpArch.Domain.DomainModel;


public class Account : Entity<int>
{
    public const int MaxTransactions = 100;
    public virtual decimal OldTransactionsBalance { get; protected set; }

    public virtual IList<Transaction> Transactions { get; protected set; }

    public virtual decimal Balance
    {
        get { return OldTransactionsBalance + Transactions.Sum(x => x.Amount); }
    }

    public virtual IList<PaymentSchedule> PaymentSchedules { get; protected set; }

    public Account()
    {
        Transactions = new List<Transaction>();
        PaymentSchedules = new List<PaymentSchedule>();
        OldTransactionsBalance = 0M;
    }

    public virtual void AddTransaction(string? description, decimal amount)
    {
        Transactions.Add(new Transaction(description, amount, this));

        RemoveOldTransactions();
    }

    void RemoveOldTransactions()
    {
        if (Transactions.Count <= MaxTransactions) return;

        var oldestTransaction = Transactions.First();
        Transactions.Remove(oldestTransaction);
        OldTransactionsBalance += oldestTransaction.Amount;
    }

    public virtual void AddPaymentSchedule(DateTime startDate, Interval interval, decimal amount, string description)
    {
        PaymentSchedules.Add(new PaymentSchedule(startDate, interval, amount, description, this));
    }

    public virtual void TriggerScheduledPayments(DateTime now)
    {
        var overdueSchedules = PaymentSchedules.Where(x => x.NextRun <= now);
        foreach (var overdueSchedule in overdueSchedules)
        {
            AddTransaction(overdueSchedule.Description, overdueSchedule.Amount);
            overdueSchedule.CalculateNextRunDate();
        }
    }

    public virtual void RemovePaymentSchedule(int paymentScheduleId)
    {
        var scheduleToRemove = PaymentSchedules.SingleOrDefault(x => x.Id == paymentScheduleId);
        if (scheduleToRemove == null) return;

        PaymentSchedules.Remove(scheduleToRemove);
    }
}
