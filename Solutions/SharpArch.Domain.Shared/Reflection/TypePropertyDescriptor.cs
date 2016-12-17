namespace SharpArch.Domain.Reflection
{
    using System;
    using System.Reflection;
    using JetBrains.Annotations;

    /// <summary>
    ///     Contains injectable properties per type.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public class TypePropertyDescriptor : IEquatable<TypePropertyDescriptor>
    {
        private static readonly PropertyInfo[] emptyArray = new PropertyInfo[0];
        private readonly Type ownerType;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypePropertyDescriptor" /> class.
        /// </summary>
        /// <param name="ownerType">Type of the object.</param>
        /// <param name="properties">The injectable properties.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public TypePropertyDescriptor([NotNull]Type ownerType, [CanBeNull] PropertyInfo[] properties)
        {
            if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));

            this.ownerType = ownerType;
            if (properties != null && properties.Length > 0)
                Properties = properties;
            else
                Properties = emptyArray;
        }

        /// <summary>
        ///     Owner type. 
        /// </summary>
        [NotNull]
        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public Type OwnerType => this.ownerType;

        /// <summary>
        ///     Gets the injectable properties.
        /// </summary>
        /// <value>
        ///     The injectable properties.
        /// </value>
        [NotNull]
        public PropertyInfo[] Properties { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether <see cref="OwnerType" /> has injectable properties.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has injectable properties; otherwise, <c>false</c>.
        /// </value>
        public bool HasProperties()
        {
            return Properties.Length > 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(TypePropertyDescriptor other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return OwnerType == other.OwnerType;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as TypePropertyDescriptor;
            return other != null && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return OwnerType.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Concat("Type: ", OwnerType.AssemblyQualifiedName, "Properties: ", Properties.Length);
        }
    }
}