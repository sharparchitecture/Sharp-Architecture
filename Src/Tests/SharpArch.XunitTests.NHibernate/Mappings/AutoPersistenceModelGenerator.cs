namespace Tests.SharpArch.NHibernate.Mappings
{
    using System;
    using Domain;
    using FluentNHibernate.Automapping;
    using FluentNHibernate.Conventions;
    using global::SharpArch.Domain.DomainModel;
    using global::SharpArch.NHibernate.FluentNHibernate;
    using global::SharpArch.NHibernate.FluentNHibernate.Conventions;


    /// <summary>
    ///     Generates the auto-mapping for test entities.
    /// </summary>
    public class TestsPersistenceModelGenerator : IAutoPersistenceModelGenerator
    {
        /// <summary>
        ///     Generates persistence model.
        /// </summary>
        /// <returns></returns>
        public AutoPersistenceModel Generate()
        {
            var mappings = AutoMap.AssemblyOf<Customer>(new AutomappingConfiguration());
            mappings.IgnoreBase<Entity>();
            mappings.IgnoreBase(typeof(EntityWithTypedId<>));
            mappings.Conventions.Setup(GetConventions());
            mappings.UseOverridesFromAssemblyOf<TestsPersistenceModelGenerator>();

            return mappings;
        }

        static Action<IConventionFinder> GetConventions()
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
