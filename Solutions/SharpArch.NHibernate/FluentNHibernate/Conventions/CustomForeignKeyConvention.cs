namespace SharpArch.NHibernate.FluentNHibernate.Conventions
{
    using System;
    using global::FluentNHibernate;
    using global::FluentNHibernate.Conventions;

    /// <summary>
    ///     Foreign key convention.
    /// </summary>
    /// <seealso cref="ForeignKeyConvention" />
    public class CustomForeignKeyConvention : ForeignKeyConvention
    {
        /// <summary>
        ///     Generates Foreign Key name.
        /// </summary>
        protected override string GetKeyName(Member property, Type type)
        {
            if (property == null) {
                return type.Name + "Id";
            }

            return property.Name + "Id";
        }
    }
}
