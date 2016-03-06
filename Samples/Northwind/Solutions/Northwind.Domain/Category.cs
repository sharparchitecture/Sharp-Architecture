namespace Northwind.Domain
{
    using NHibernate.Validator.Constraints;

    using SharpArch.Domain.DomainModel;

    public class Category : Entity
    {
        public Category()
        {
        }

        /// <summary>
        ///   Creates valid domain object
        /// </summary>
        public Category(string name)
        {
            this.CategoryName = name;
        }

        [DomainSignature]
        [NotNullNotEmpty]
        public virtual string CategoryName { get; protected set; }
    }
}