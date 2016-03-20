namespace Northwind.Domain
{
    using NHibernate.Validator.Constraints;

    using SharpArch.Domain.DomainModel;

    public class Product : Entity
    {
        public Product()
        {
        }

        /// <summary>
        ///   Creates valid domain object
        /// </summary>
        public Product(string name, Supplier supplier)
        {
            this.Supplier = supplier;
            this.ProductName = name;
        }

        public virtual Category Category { get; set; }

        [DomainSignature]
        [NotNullNotEmpty]
        public virtual string ProductName { get; set; }

        [DomainSignature]
        [NotNull]
        public virtual Supplier Supplier { get; protected set; }
    }
}