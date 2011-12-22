namespace SharpArch.Domain.DomainModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;

    using Newtonsoft.Json;

    /// <summary>
    ///     For a discussion of this object, see 
    ///     http://devlicio.us/blogs/billy_mccafferty/archive/2007/04/25/using-equals-gethashcode-effectively.aspx
    /// </summary>
    [Serializable]
    public abstract class EntityWithTypedId<TId> : ValidatableObject, IEntityWithTypedId<TId>
    {
        /// <summary>
        ///     To help ensure hashcode uniqueness, a carefully selected random number multiplier 
        ///     is used within the calculation.  Goodrich and Tamassia's Data Structures and
        ///     Algorithms in Java asserts that 31, 33, 37, 39 and 41 will produce the fewest number
        ///     of collissions.  See http://computinglife.wordpress.com/2008/11/20/why-do-hash-functions-use-prime-numbers/
        ///     for more information.
        /// </summary>
        private const int HashMultiplier = 31;

        private int? cachedHashcode;

        /// <summary>
        ///     Id may be of type string, int, custom type, etc.
        ///     Setter is protected to allow unit tests to set this property via reflection and to allow 
        ///     domain objects more flexibility in setting this for those objects with assigned Ids.
        ///     It's virtual to allow NHibernate-backed objects to be lazily loaded.
        /// 
        ///     This is ignored for XML serialization because it does not have a public setter (which is very much by design).
        ///     See the FAQ within the documentation if you'd like to have the Id XML serialized.
        /// </summary>
        [XmlIgnore]
        [JsonProperty]
        public virtual TId Id { get; protected set; }

        public override bool Equals(object obj)
        {
            var compareTo = obj as EntityWithTypedId<TId>;

            if (ReferenceEquals(this, compareTo))
            {
                return true;
            }

            if (compareTo == null || !this.GetType().Equals(compareTo.GetTypeUnproxied()))
            {
                return false;
            }

            if (this.HasSameNonDefaultIdAs(compareTo))
            {
                return true;
            }

            // Since the Ids aren't the same, both of them must be transient to 
            // compare domain signatures; because if one is transient and the 
            // other is a persisted entity, then they cannot be the same object.
            return this.IsTransient() && compareTo.IsTransient() && this.HasSameObjectSignatureAs(compareTo);
        }

        public override int GetHashCode()
        {
            if (this.cachedHashcode.HasValue)
            {
                return this.cachedHashcode.Value;
            }

            if (this.IsTransient())
            {
                this.cachedHashcode = base.GetHashCode();
            }
            else
            {
                unchecked
                {
                    // It's possible for two objects to return the same hash code based on 
                    // identically valued properties, even if they're of two different types, 
                    // so we include the object's type in the hash calculation
                    var hashCode = this.GetType().GetHashCode();
                    this.cachedHashcode = (hashCode * HashMultiplier) ^ this.Id.GetHashCode();
                }
            }

            return this.cachedHashcode.Value;
        }

        /// <summary>
        ///     Transient objects are not associated with an item already in storage.  For instance,
        ///     a Customer is transient if its Id is 0.  It's virtual to allow NHibernate-backed 
        ///     objects to be lazily loaded.
        /// </summary>
        public virtual bool IsTransient()
        {
            return this.Id == null || this.Id.Equals(default(TId));
        }

        /// <summary>
        ///     The property getter for SignatureProperties should ONLY compare the properties which make up 
        ///     the "domain signature" of the object.
        /// 
        ///     If you choose NOT to override this method (which will be the most common scenario), 
        ///     then you should decorate the appropriate property(s) with [DomainSignature] and they 
        ///     will be compared automatically.  This is the preferred method of managing the domain
        ///     signature of entity objects.
        /// </summary>
        /// <remarks>
        ///     This ensures that the entity has at least one property decorated with the 
        ///     [DomainSignature] attribute.
        /// </remarks>
        protected override IEnumerable<PropertyInfo> GetTypeSpecificSignatureProperties()
        {
            return
                this.GetType().GetProperties().Where(
                    p => Attribute.IsDefined(p, typeof(DomainSignatureAttribute), true));
        }

        /// <summary>
        ///     Returns true if self and the provided entity have the same Id values 
        ///     and the Ids are not of the default Id value
        /// </summary>
        private bool HasSameNonDefaultIdAs(EntityWithTypedId<TId> compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Id.Equals(compareTo.Id);
        }
    }
}