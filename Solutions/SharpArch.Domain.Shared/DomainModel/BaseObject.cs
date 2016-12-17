namespace SharpArch.Domain.DomainModel
{
    using System;
    using System.Reflection;
    using JetBrains.Annotations;
    using Reflection;

    /// <summary>
    ///     Provides a standard base class for facilitating comparison of objects.
    /// </summary>
    /// <remarks>
    ///     For a discussion of the implementation of Equals/GetHashCode, see
    ///     http://devlicio.us/blogs/billy_mccafferty/archive/2007/04/25/using-equals-gethashcode-effectively.aspx
    ///     and http://groups.google.com/group/sharp-architecture/browse_thread/thread/f76d1678e68e3ece?hl=en for
    ///     an in depth and conclusive resolution.
    /// </remarks>
    [Serializable]
    [PublicAPI]
    public abstract class BaseObject
    {
        /// <summary>
        ///     To help ensure hash code uniqueness, a carefully selected random number multiplier
        ///     is used within the calculation. Goodrich and Tamassia's Data Structures and
        ///     Algorithms in Java asserts that 31, 33, 37, 39 and 41 will produce the fewest number
        ///     of collissions. See http://computinglife.wordpress.com/2008/11/20/why-do-hash-functions-use-prime-numbers/
        ///     for more information.
        /// </summary>
        private const int HashMultiplier = 31;

        /// <summary>
        ///     This static member caches the domain signature properties to avoid looking them up for
        ///     each instance of the same type.
        /// </summary>
        private static readonly ITypePropertyDescriptorCache signaturePropertiesCache
            = new TypePropertyDescriptorCache();

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with the current <see cref="object" />.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var compareTo = obj as BaseObject;

            if (ReferenceEquals(this, compareTo))
            {
                return true;
            }

            return compareTo != null && this.GetType().Equals(compareTo.GetTypeUnproxied()) &&
                HasSameObjectSignatureAs(compareTo);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        /// <remarks>
        ///     This is used to provide the hash code identifier of an object using the signature
        ///     properties of the object; although it's necessary for NHibernate's use, this can
        ///     also be useful for business logic purposes and has been included in this base
        ///     class, accordingly. Since it is recommended that GetHashCode change infrequently,
        ///     if at all, in an object's lifetime, it's important that properties are carefully
        ///     selected which truly represent the signature of an object.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                PropertyInfo[] signatureProperties = this.GetSignatureProperties();

                // If no properties were flagged as being part of the signature of the object,
                // then simply return the hashcode of the base object as the hashcode.
                if (signatureProperties.Length == 0)
                {
                    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
                    return base.GetHashCode();
                }

                // It's possible for two objects to return the same hash code based on 
                // identically valued properties, even if they're of two different types, 
                // so we include the object's type in the hash calculation
                int hashCode = this.GetType().GetHashCode();

                for (var i = 0; i < signatureProperties.Length; i++)
                {
                    PropertyInfo property = signatureProperties[i];
                    object value = property.GetValue(this, null);
                    if (value != null)
                    {
                        hashCode = (hashCode * HashMultiplier) ^ value.GetHashCode();
                    }
                }

                return hashCode;
            }
        }

        /// <summary>
        ///     Returns the properties of the current object that make up the object's signature.
        /// </summary>
        public virtual PropertyInfo[] GetSignatureProperties()
        {
            Type type = GetTypeUnproxied();

            // Since data won't be in cache on first request only, use .GetOrAdd as second attempt to prevent allocation of extra lambda object.
            TypePropertyDescriptor descriptor = signaturePropertiesCache.Find(type) ?? GetOrAdd(type);
            return descriptor.Properties;
        }

        TypePropertyDescriptor GetOrAdd(Type type)
        {
            return signaturePropertiesCache.GetOrAdd(type,
                t => new TypePropertyDescriptor(t, GetTypeSpecificSignatureProperties()));
        }

        /// <summary>
        ///     Determines whether the current object has the same object signature as the specified object.
        /// </summary>
        /// <param name="compareTo">The object to compare to.</param>
        /// <returns>
        ///     <c>true</c> if the current object has the same object signature as the specified object; otherwise,
        ///     <c>false</c>.
        /// </returns>
        /// <remarks>You may override this method to provide your own comparison routine.</remarks>
        public virtual bool HasSameObjectSignatureAs(BaseObject compareTo)
        {
            PropertyInfo[] signatureProperties = this.GetSignatureProperties();

            // if there were no signature properties, then simply return the default behavior of Equals
            if (signatureProperties.Length == 0)
            {
                // ReSharper disable once BaseObjectEqualsIsObjectEquals
                return base.Equals(compareTo);
            }

            // use for loop instead of foreach/LINQ for performance reasons.
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var index = 0; index < signatureProperties.Length; index++)
            {
                PropertyInfo property = signatureProperties[index];
                object valueOfThisObject = property.GetValue(this, null);
                object valueToCompareTo = property.GetValue(compareTo, null);

                if (valueOfThisObject == null && valueToCompareTo == null)
                {
                    continue;
                }

                if ((valueOfThisObject == null ^ valueToCompareTo == null) ||
                    (!valueOfThisObject.Equals(valueToCompareTo)))
                {
                    return false;
                }
            }

            // If we've gotten this far and signature properties were found, then we can
            // assume that everything matched
            return true;
        }

        /// <summary>
        ///     Enforces the template method pattern to have child objects determine which specific
        ///     properties should and should not be included in the object signature comparison.
        /// </summary>
        [NotNull]
        protected abstract PropertyInfo[] GetTypeSpecificSignatureProperties();

        /// <summary>
        ///     Returns the unproxied type of the current object.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         When NHibernate proxies objects, it masks the type of the actual entity object.
        ///         This wrapper burrows into the proxied object to get its actual type.
        ///     </para>
        ///     <para>
        ///         Although this assumes NHibernate is being used, it doesn't require any NHibernate
        ///         related dependencies and has no bad side effects if NHibernate isn't being used.
        ///     </para>
        ///     <para>
        ///         Related discussion is at
        ///         http://groups.google.com/group/sharp-architecture/browse_thread/thread/ddd05f9baede023a ...thanks Jay Oliver!
        ///     </para>
        /// </remarks>
        protected virtual Type GetTypeUnproxied()
        {
            return this.GetType();
        }
    }
}
