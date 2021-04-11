namespace Suteki.TardisBank.Domain
{
    using System;

    using SharpArch.Domain.DomainModel;

    public class Transaction : Entity<int>
    {
        public Transaction(string? description, decimal amount, Account account)
        {
            this.Description = description;
            this.Amount = amount;
            this.Account = account;
            this.Date = DateTime.Now.Date;
        }

        protected Transaction()
        {
        }

        public virtual string? Description { get; protected set; }
        public virtual decimal Amount { get; protected set; }

        public virtual Account Account { get; protected set; } = null!;

        public virtual DateTime Date { get; protected set; }
    }
}
