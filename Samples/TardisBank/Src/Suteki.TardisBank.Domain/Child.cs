namespace Suteki.TardisBank.Domain
{
    using System;
    using MediatR;

    public class Child : User
    {
        public Child(string name, string userName, string password, int parentId) : base(name, userName, password)
        {
            ParentId = parentId;
            Account = new Account();
            IsActive = true;
        }

        protected Child()
        {
        }

        public virtual int ParentId { get; set; }
        public virtual Account Account { get; set; }

        public virtual void ReceivePayment(decimal amount, string description)
        {
            Account.AddTransaction(description, amount);
        }

        public virtual void WithdrawCashFromParent(Parent parent, decimal amount, string description, IMediator mediator)
        {
            var insufficientFundsMessage = string.Format(
                "You can not withdraw {0} because you only have {1} in your account",
                amount.ToString("c"),
                Account.Balance.ToString("c"));

            WithdrawInternal(parent, amount, description, insufficientFundsMessage);
            parent.SendMessage(string.Format("{0} would like to withdraw {1}", Name, amount.ToString("c")), mediator);
        }

        public virtual void AcceptCashFromParent(Parent parent, decimal amount, string description)
        {
            var insufficientFundsMessage = string.Format(
                "You can not withdraw {0} because {1}'s account only has {2}",
                amount.ToString("c"),
                Name,
                Account.Balance.ToString("c"));

            WithdrawInternal(parent, amount, description, insufficientFundsMessage);
        }

        void WithdrawInternal(Parent parent, decimal amount, string description, string insufficientFundsMessage)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }

            if (!parent.HasChild(this))
            {
                throw new CashWithdrawException("Not Your Parent");
            }

            if (amount > Account.Balance)
            {
                throw new CashWithdrawException(insufficientFundsMessage);
            }

            Account.AddTransaction(description, -amount);
        }

        public override string[] GetRoles()
        {
            return new[] { UserRoles.Child };
        }
    }
}