namespace Suteki.TardisBank.Infrastructure.NHibernateMaps.Conventions
{
    #region Using Directives

    using System;

    using FluentNHibernate;
    using FluentNHibernate.Conventions;

    #endregion

    public class CustomForeignKeyConvention : ForeignKeyConvention 
    {
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