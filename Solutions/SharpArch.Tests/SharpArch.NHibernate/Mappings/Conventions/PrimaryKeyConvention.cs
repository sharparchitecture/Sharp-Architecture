namespace Tests.SharpArch.NHibernate.Mappings.Conventions
{
    using FluentNHibernate.Conventions;

    /// <summary>
    /// Primary Key convention.
    /// </summary>
    /// <remarks>
    /// Defines Primary Key name as EntityType+Id. E.c. <c>ColorId</c>
    /// </remarks>
    /// <seealso cref="FluentNHibernate.Conventions.IIdConvention" />
    public class PrimaryKeyConvention : IIdConvention
    {
        /// <summary>
        /// Applies convention.
        /// </summary>
        public void Apply(FluentNHibernate.Conventions.Instances.IIdentityInstance instance)
        {
            instance.Column(instance.EntityType.Name + "Id");
        }
    }
}