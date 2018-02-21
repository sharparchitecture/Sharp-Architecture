namespace Tests.SharpArch.NHibernate.Mappings.Conventions
{
    using System;
    using FluentNHibernate;
    using FluentNHibernate.Conventions;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FluentNHibernate.Conventions.ForeignKeyConvention" />
    public class CustomForeignKeyConvention : ForeignKeyConvention 
    {

        /// <summary>
        /// Generates Foreign Key name.
        /// </summary>
        protected override string GetKeyName(Member property, Type type)
        {
            if (property == null)
            {
                return type.Name + "Id";
            }

            return property.Name + "Id";  
        }
    }
}