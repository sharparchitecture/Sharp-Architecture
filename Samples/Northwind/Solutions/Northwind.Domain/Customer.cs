namespace Northwind.Domain
{
    using System.Collections.Generic;

    using NHibernate.Validator.Constraints;

    using SharpArch.Domain;
    using SharpArch.Domain.DomainModel;

    /// <summary>
    ///   I'd like to be perfectly clear that I think assigned IDs are almost always a terrible
    ///   idea; this is a major complaint I have with the Northwind database.  With that said, 
    ///   some legacy databases require such techniques.
    /// </summary>
    public class Customer : EntityWithTypedId<string>, IHasAssignedId<string>
    {
        public Customer()
        {
            this.InitMembers();
        }

        /// <summary>
        ///   Creates valid domain object
        /// </summary>
        public Customer(string companyName)
            : this()
        {
            this.CompanyName = companyName;
        }

        [DomainSignature]
        [NotNullNotEmpty]
        public virtual string CompanyName { get; set; }

        [DomainSignature]
        public virtual string ContactName { get; set; }

        public virtual string Country { get; set; }

        public virtual string Fax { get; set; }

        /// <summary>
        ///   Note the protected set...only the ORM should set the collection reference directly
        ///   after it's been initialized in <see cref = "InitMembers" />
        /// </summary>
        public virtual IList<Order> Orders { get; protected set; }

        public virtual void SetAssignedIdTo(string assignedId)
        {
            Check.Require(!string.IsNullOrEmpty(assignedId), "assignedId may not be null or empty");
            Check.Require(assignedId.Trim().Length == 5, "assignedId must be exactly 5 characters");

            this.Id = assignedId.Trim().ToUpper();
        }

        /// <summary>
        ///   Since we want to leverage automatic properties, init appropriate members here.
        /// </summary>
        private void InitMembers()
        {
            this.Orders = new List<Order>();
        }
    }
}