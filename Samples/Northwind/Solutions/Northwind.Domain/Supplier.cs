namespace Northwind.Domain
{
    using System.Collections.Generic;

    using NHibernate.Validator.Constraints;

    using SharpArch.Domain.DomainModel;

    public class Supplier : Entity
    {
        /// <summary>
        ///   Creates valid domain object
        /// </summary>
        public Supplier(string companyName)
            : this()
        {
            this.CompanyName = companyName;
        }

        protected Supplier()
        {
            this.InitMembers();
        }

        [DomainSignature]
        [NotNullNotEmpty]
        public virtual string CompanyName { get; set; }

        /// <summary>
        ///   Note the protected set...only the ORM should set the collection reference directly
        ///   after it's been initialized in <see cref = "InitMembers" />
        /// </summary>
        public virtual IList<Product> Products { get; protected set; }

        private void InitMembers()
        {
            this.Products = new List<Product>();
        }
    }
}