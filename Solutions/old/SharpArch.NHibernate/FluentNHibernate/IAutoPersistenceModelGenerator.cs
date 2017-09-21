namespace SharpArch.NHibernate.FluentNHibernate
{
    using global::FluentNHibernate.Automapping;
    using JetBrains.Annotations;

    /// <summary>
    /// Fluent NHibernate auto-mapping model generator.
    /// </summary>
    /// <remarks>
    /// Interface implementors will be automatically executed by TestDatabaseInitializer during test database initialization.
    /// </remarks>
    [PublicAPI]
    public interface IAutoPersistenceModelGenerator
    {
        /// <summary>
        /// Generates persistence model.
        /// </summary>
        [NotNull]
        AutoPersistenceModel Generate();
    }
}
