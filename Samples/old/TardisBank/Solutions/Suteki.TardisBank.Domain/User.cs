namespace Suteki.TardisBank.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MediatR;
    using SharpArch.Domain;
    using SharpArch.Domain.DomainModel;

    using Suteki.TardisBank.Domain.Events;

    /// <summary>
    /// User role constants.
    /// </summary>
    public static class UserRoles
    {
        /// <summary>
        /// User is child.
        /// </summary>
        public const string Child = "Child";
        /// <summary>
        /// User is parent.
        /// </summary>
        public const string Parent = "Parent";
    }

    public abstract class User : Entity
    {
        public const int MaxMessages = 20;

        protected User()
        {
        }

        public virtual string Name { get; protected set; }
        public virtual string UserName { get; protected set; }
        public virtual string Password { get; protected set; }
        public virtual bool IsActive { get; protected set; }
        public virtual IList<Message> Messages { get; protected set; }

        protected User(string name, string userName, string password)
        {
            this.Name = name;
            this.UserName = userName;
            this.Password = password;
            this.Messages = new List<Message>();
            this.IsActive = false;
        }

        public virtual void SendMessage(string text, IMediator mediator)
        {
            if (mediator == null) throw new ArgumentNullException(nameof(mediator));

            this.Messages.Add(new Message(DateTime.Now.Date, text, this));
            this.RemoveOldMessages();

            mediator.Publish(new SendMessageEvent(this, text));
        }

        void RemoveOldMessages()
        {
            if (this.Messages.Count <= MaxMessages) return;

            var oldestMessage = this.Messages.First();
            this.Messages.Remove(oldestMessage);
        }

        public virtual void ReadMessage(int messageId)
        {
            var message = this.Messages.SingleOrDefault(x => x.Id == messageId);
            if (message == null)
            {
                throw new TardisBankException("No message with Id {0} found for user '{1}'", messageId, this.UserName);
            }
            message.Read();
        }

        public virtual void Activate()
        {
            this.IsActive = true;
        }

        public virtual void ResetPassword(string newPassword)
        {
            this.Password = newPassword;
        }

        /// <summary>
        /// Returns user roles.
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetRoles()
        {
            return new string[0];
        }
    }
}