namespace Suteki.TardisBank.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Events;
    using MediatR;

    public class Parent : User
    {
        public Parent(string name, string userName, string password) : base(name, userName, password)
        {
            this.Children = new List<Child>();
        }

        protected Parent()
        {
        }

        public virtual IList<Child> Children { get; protected set; }
        public virtual string ActivationKey { get; protected set; }
        // should be called when parent is first created.
        public virtual Parent Initialise(IMediator mediator)
        {
            if (mediator == null) throw new ArgumentNullException(nameof(mediator));

            this.ActivationKey = Guid.NewGuid().ToString();
            mediator.Publish(new NewParentCreatedEvent(this));
            return this;
        }

        public virtual Child CreateChild(string name, string userName, string password)
        {
            var child = new Child(name, userName, password, this.Id);

            this.Children.Add(child);
            return child;
        }

        public virtual void MakePaymentTo(Child child, decimal amount)
        {
            this.MakePaymentTo(child, amount, string.Format("Payment from {0}", this.Name));
        }

        public virtual void MakePaymentTo(Child child, decimal amount, string description)
        {
            if (!this.HasChild(child))
            {
                throw new TardisBankException("{0} is not a child of {1}", child.Name, this.Name);
            }
            child.ReceivePayment(amount, description);
        }

        public virtual bool HasChild(Child child)
        {
            return this.Children.Any(x => x == child);
        }

        public override void Activate()
        {
            this.ActivationKey = "";
            base.Activate();
        }

        public virtual bool HasChild(int childId)
        {
            return this.Children.Any(x => x.Id == childId);
        }

        public virtual void RemoveChild(int childId)
        {
            Child childToRemove = this.Children.SingleOrDefault(x => x.Id == childId);
            if (childToRemove != null)
            {
                this.Children.Remove(childToRemove);
            }
        }

        public override string[] GetRoles()
        {
            return new[] { UserRoles.Parent };
        }
    }
}
