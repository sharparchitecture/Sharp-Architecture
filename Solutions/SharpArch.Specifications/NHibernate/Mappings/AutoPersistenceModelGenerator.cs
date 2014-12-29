namespace SharpArch.Specifications.NHibernate.Mappings
{
    #region Using Directives

    using System;

    using FluentNHibernate.Automapping;
    using FluentNHibernate.Conventions;

    using global::SharpArch.Domain.DomainModel;
    using global::SharpArch.NHibernate.FluentNHibernate;
    using global::SharpArch.Specifications.NHibernate.Mappings.Conventions;

    #endregion

    /// <summary>
    /// Generates the automapping for the domain assembly
    /// </summary>
    public class AutoPersistenceModelGenerator : IAutoPersistenceModelGenerator
    {
        public AutoPersistenceModel Generate()
        {
            var mappings = AutoMap.AssemblyOf<AutoPersistenceModelGenerator>(new AutomappingConfiguration());
            mappings.IgnoreBase<Entity>();
            mappings.IgnoreBase(typeof(EntityWithTypedId<>));
            mappings.Conventions.Setup(GetConventions());
            mappings.UseOverridesFromAssemblyOf<AutoPersistenceModelGenerator>();

            return mappings;
        }

        private static Action<IConventionFinder> GetConventions()
        {
            return c =>
                   {
                       c.Add<PrimaryKeyConvention>();
                       c.Add<CustomForeignKeyConvention>();
                       c.Add<HasManyConvention>();
                       c.Add<TableNameConvention>();
                   };
        }
    }
}