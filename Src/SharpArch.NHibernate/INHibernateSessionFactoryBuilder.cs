using SharpArch.NHibernate.Configuration;

namespace SharpArch.NHibernate
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using global::FluentNHibernate.Automapping;
    using global::FluentNHibernate.Cfg.Db;
    using global::NHibernate;
    using JetBrains.Annotations;
    using NHibernateValidator;


    public interface INHibernateSessionFactoryBuilder
    {
        /// <summary>
        ///     Creates the session factory.
        /// </summary>
        /// <returns> NHibernate session factory <see cref="ISessionFactory" />.</returns>
        ISessionFactory BuildSessionFactory();

        /// <summary>
        ///     Builds NHibernate configuration.
        /// </summary>
        /// <param name="basePath">
        ///     Base directory to use for loading additional files.
        ///     If <c>null</c> base folder of the current assembly is used.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         Any changes made to configuration object after this point <b>will not be persisted</b> in configuration cache.
        ///         This can be useful to make dynamic changes to configuration or in case changes cannot be serialized
        ///         (e.g. event listeners are not marked with <see cref="System.SerializableAttribute" />.
        ///     </para>
        ///     <para>
        ///         To make persistent changes use <seealso cref="NHibernateSessionFactoryBuilder.ExposeConfiguration" />.
        ///     </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">No dependencies were specified</exception>
        global::NHibernate.Cfg.Configuration BuildConfiguration(string basePath = null);

        /// <summary>
        ///     Allows to alter configuration before creating NHibernate configuration.
        /// </summary>
        /// <remarks>
        ///     Changes to configuration will be persisted in configuration cache, if it is enabled.
        ///     In case changes must not be persisted in cache, they must be applied after <seealso cref="NHibernateSessionFactoryBuilder.BuildConfiguration" />.
        /// </remarks>
        NHibernateSessionFactoryBuilder ExposeConfiguration([NotNull] Action<global::NHibernate.Cfg.Configuration> config);

        /// <summary>
        ///     Allows to cache compiled NHibernate configuration.
        /// </summary>
        /// <param name="configurationCache">The configuration cache, see <see cref="INHibernateConfigurationCache" />. </param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Please provide configuration cache instance.</exception>
        NHibernateSessionFactoryBuilder UseConfigurationCache(
            [NotNull] INHibernateConfigurationCache configurationCache);

        /// <summary>
        ///     Allows to specify additional assemblies containing FluentNHibernate mappings.
        /// </summary>
        /// <param name="mappingAssemblies">The mapping assemblies.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Mapping assemblies are not specified.</exception>
        NHibernateSessionFactoryBuilder AddMappingAssemblies([NotNull] IEnumerable<Assembly> mappingAssemblies);

        /// <summary>
        ///     Add generic file dependency.
        ///     Used with session cache to add dependency which is not used to configure session
        ///     (e.g. application configuration, shared library, etc...)
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">File name cannot be empty</exception>
        NHibernateSessionFactoryBuilder WithFileDependency([NotNull] string fileName);

        /// <summary>
        ///     Allows to specify FluentNhibernate auto-persistence model to use..
        /// </summary>
        /// <param name="autoPersistenceModel">The automatic persistence model.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        NHibernateSessionFactoryBuilder UseAutoPersistenceModel(
            [NotNull] AutoPersistenceModel autoPersistenceModel);

        /// <summary>
        ///     Allows to specify additional NHibernate properties, see
        ///     http://nhibernate.info/doc/nhibernate-reference/session-configuration.html.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>Builder instance.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="properties" /> is <c>null</c>.</exception>
        NHibernateSessionFactoryBuilder UseProperties([NotNull] IEnumerable<KeyValuePair<string, string>> properties);

        /// <summary>
        ///     Allows to use Data Annotations and <see cref="Validator" /> to validate entities before insert/update.
        /// </summary>
        /// <remarks>
        ///     See https://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations%28v=vs.110%29.aspx for details
        ///     about Data Annotations.
        /// </remarks>
        /// <seealso cref="DataAnnotationsEventListener" />.
        NHibernateSessionFactoryBuilder UseDataAnnotationValidators(bool addDataAnnotationValidators);

        /// <summary>
        ///     Allows to specify nhibernate configuration file.
        /// </summary>
        /// <remarks>
        ///     See http://nhibernate.info/doc/nhibernate-reference/session-configuration.html#configuration-xmlconfig for more
        ///     details
        /// </remarks>
        /// <exception cref="System.ArgumentException">NHibernate config was not specified.</exception>
        NHibernateSessionFactoryBuilder UseConfigFile(string nhibernateConfigFile);

        /// <summary>
        ///     Allows to specify custom configuration using <see cref="IPersistenceConfigurer" />.
        /// </summary>
        /// <param name="persistenceConfigurer">The persistence configurer.</param>
        /// <example>
        ///     <code>
        /// var persistenceConfigurer =
        ///   SQLiteConfiguration.Standard.ConnectionString(c => c.Is("Data Source=:memory:;Version=3;New=True;"));
        /// var configuration = new NHibernateSessionFactoryBuilder()
        ///   .UsePersistenceConfigurer(persistenceConfigurer);
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentNullException"><paramref name="persistenceConfigurer" /> is <c>null</c>.</exception>
        NHibernateSessionFactoryBuilder UsePersistenceConfigurer(
            [NotNull] IPersistenceConfigurer persistenceConfigurer);
    }
}
