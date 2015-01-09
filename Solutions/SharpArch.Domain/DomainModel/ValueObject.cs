namespace SharpArch.Domain.DomainModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Provides a standard base class for facilitating comparison of value objects using all the object's properties.
    /// 
    ///     For a discussion of the implementation of Equals/GetHashCode, see 
    ///     http://devlicio.us/blogs/billy_mccafferty/archive/2007/04/25/using-equals-gethashcode-effectively.aspx
    ///     and http://groups.google.com/group/sharp-architecture/browse_thread/thread/f76d1678e68e3ece?hl=en for 
    ///     an in depth and conclusive resolution.
    /// </summary>
    [Serializable]
    public abstract class ValueObject : BaseObject
    {
        public static bool operator ==(ValueObject valueObject1, ValueObject valueObject2)
        {
            if ((object)valueObject1 == null)
            {
                return (object)valueObject2 == null;
            }

            return valueObject1.Equals(valueObject2);
        }

        public static bool operator !=(ValueObject valueObject1, ValueObject valueObject2)
        {
            return !(valueObject1 == valueObject2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        ///     The getter for SignatureProperties for value objects should include the properties 
        ///     which make up the entirety of the object's properties; that's part of the definition 
        ///     of a value object.
        /// </summary>
        /// <remarks>
        ///     This ensures that the value object has no properties decorated with the 
        ///     [DomainSignature] attribute.
        /// </remarks>
        protected override PropertyInfo[] GetTypeSpecificSignatureProperties()
        {
            var invalidlyDecoratedProperties =
                this.GetType().GetProperties().Where(
                    p => Attribute.IsDefined(p, typeof(DomainSignatureAttribute), true));

            string message = "Properties were found within " + this.GetType() +
                             @" having the
                [DomainSignature] attribute. The domain signature of a value object includes all
                of the properties of the object by convention; consequently, adding [DomainSignature]
                to the properties of a value object's properties is misleading and should be removed. 
                Alternatively, you can inherit from Entity if that fits your needs better.";

            Check.Require(
                !invalidlyDecoratedProperties.Any(), 
                message);

            return this.GetType().GetProperties();
        }
    }
}