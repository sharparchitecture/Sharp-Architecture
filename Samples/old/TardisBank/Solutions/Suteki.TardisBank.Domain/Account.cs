namespace Suteki.TardisBank.Domain
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using SharpArch.Domain.DomainModel;

    public class Account : Entity
    {
        public const int MaxTransactions = 100;
        public virtual decimal OldTransactionsBalance { get; protected set; }

        public Account()
        {
            this.Transactions = new List<Transaction>();
            this.PaymentSchedules = new List<PaymentSchedule>();
            this.OldTransactionsBalance = 0M;
        }

        public virtual IList<Transaction> Transactions { get; protected set; }

        public virtual decimal Balance
        {
            get { return this.OldTransactionsBalance + this.Transactions.Sum(x => x.Amount); }
        }

        public virtual IList<PaymentSchedule> PaymentSchedules { get; protected set; }

        public virtual void AddTransaction(string description, decimal amount)
        {
            this.Transactions.Add(new Transaction(description, amount, this));

            this.RemoveOldTransactions();
        }

        void RemoveOldTransactions()
        {
            if (this.Transactions.Count <= MaxTransactions) return;

            var oldestTransaction = this.Transactions.First();
            this.Transactions.Remove(oldestTransaction);
            this.OldTransactionsBalance += oldestTransaction.Amount;
        }

        public virtual void AddPaymentSchedule(DateTime startDate, Interval interval, decimal amount, string description)
        {
            this.PaymentSchedules.Add(new PaymentSchedule(startDate, interval, amount, description, this));
        }

        public virtual void TriggerScheduledPayments(DateTime now)
        {
            var overdueSchedules = this.PaymentSchedules.Where(x => x.NextRun <= now);
            foreach (var overdueSchedule in overdueSchedules)
            {
                this.AddTransaction(overdueSchedule.Description, overdueSchedule.Amount);
                overdueSchedule.CalculateNextRunDate();
            }
        }

        public virtual void RemovePaymentSchedule(int paymentScheduleId)
        {
            var scheduleToRemove = this.PaymentSchedules.SingleOrDefault(x => x.Id == paymentScheduleId);
            if (scheduleToRemove == null) return;

            this.PaymentSchedules.Remove(scheduleToRemove);
        }
    }
}