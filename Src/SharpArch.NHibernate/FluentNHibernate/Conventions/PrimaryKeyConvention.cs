namespace SharpArch.NHibernate.FluentNHibernate.Conventions
{
    using global::FluentNHibernate.Conventions;
    using global::FluentNHibernate.Conventions.Instances;
    using JetBrains.Annotations;


    /// <summary>
    ///     Primary Key convention.
    /// </summary>
    /// <remarks>
    ///     Defines Primary Key name as EntityType+Id. E.c. <c>ColorId</c>
    /// </remarks>
    /// <seealso cref="IIdConvention" />
    [PublicAPI]
    public class PrimaryKeyConvention : IIdConvention
    {
        /// <summary>
        ///     Applies convention.
        /// </summary>
        public void Apply(IIdentityInstance instance)
        {
            instance.Column(instance.EntityType.Name + "Id");
        }
    }
}
