namespace Tests.SharpArch.NHibernate.Mappings.Conventions
{
    using FluentNHibernate.Conventions;

    /// <summary>
    /// Table name convention.
    /// </summary>
    /// <remarks>Defines table name to match entity name. E.g.: <c>Color</c>.</remarks>
    /// <seealso cref="FluentNHibernate.Conventions.IClassConvention" />
    public class TableNameConvention : IClassConvention
    {
        /// <summary>
        /// Applies convention.
        /// </summary>
        public void Apply(FluentNHibernate.Conventions.Instances.IClassInstance instance)
        {
            instance.Table(instance.EntityType.Name);

        }
    }
}