namespace Northwind.Domain
{
    using System;

    using SharpArch.Domain;
    using SharpArch.Domain.DomainModel;

    public class Order : Entity
    {
        public Order(Customer orderedBy)
        {
            Check.Require(orderedBy != null, "orderedBy may not be null");

            this.OrderedBy = orderedBy;
        }

        /// <summary>
        ///   This is a placeholder constructor for NHibernate.
        ///   A no-argument constructor must be avilable for NHibernate to create the object.
        /// </summary>
        protected Order()
        {
        }

        public virtual DateTime? OrderDate { get; set; }

        public virtual Customer OrderedBy { get; protected set; }

        public virtual string ShipToName { get; set; }

        /// <summary>
        ///   Should ONLY contain the "business value signature" of the object and not the Id, 
        ///   which is handled by <see cref = "Entity" />.  This method should return a unique 
        ///   int representing a unique signature of the domain object.  For 
        ///   example, no two different orders should have the same ShipToName, OrderDate and OrderedBy;
        ///   therefore, the returned "signature" should be expressed as demonstrated below.
        /// 
        ///   Alternatively, we could decorate properties with the [DomainSignature] attribute, as shown in
        ///   <see cref = "Customer" />, but here's an example of overriding it nonetheless.
        /// </summary>
        public override bool HasSameObjectSignatureAs(BaseObject compareTo)
        {
            var orderCompareTo = compareTo as Order;

            return orderCompareTo != null && this.ShipToName.Equals(orderCompareTo.ShipToName) &&
                   (this.OrderDate ?? DateTime.MinValue).Equals(orderCompareTo.OrderDate ?? DateTime.MinValue) &&
                   this.OrderedBy.Equals(orderCompareTo.OrderedBy);
        }
    }
}