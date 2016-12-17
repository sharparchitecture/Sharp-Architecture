namespace Tests.SharpArch.NHibernate.Mappings.Conventions
{
    using FluentNHibernate.Conventions;
    using FluentNHibernate.Conventions.Instances;

    /// <summary>
    ///     One-to-many convention.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <listheader>
    ///             Following conventions are applied:
    ///         </listheader>
    ///         <item>
    ///             <term>
    ///                 Cascades
    ///             </term>
    ///             <description>
    ///                 All, delete orphan.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Inverse</term>
    ///         </item>
    /// <item>
    /// <term>Foreign Key</term>
    /// <description>Entity name + Id. E.g. <c>ColorId</c></description>
    /// </item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="FluentNHibernate.Conventions.IHasManyConvention" />
    public class HasManyConvention : IHasManyConvention
    {
        /// <summary>
        ///     Applies convention.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void Apply(IOneToManyCollectionInstance instance)
        {
            instance.Key.Column(instance.EntityType.Name + "Id");
            instance.Cascade.AllDeleteOrphan();
            instance.Inverse();
        }
    }
}
