namespace Suteki.TardisBank.Domain
{
    using System;

    using SharpArch.Domain.DomainModel;

    public class Message : Entity
    {
        public Message(DateTime date, string text, User user)
        {
            this.Date = date;
            this.Text = text;
            this.User = user;
            this.HasBeenRead = false;
        }

        protected Message()
        {
        }

        public virtual void Read()
        {
            this.HasBeenRead = true;
        }

        public virtual DateTime Date { get; protected set; }
        public virtual string Text { get; protected set; }

        public virtual User User { get; set; }

        public virtual bool HasBeenRead { get; protected set; }
    }
}