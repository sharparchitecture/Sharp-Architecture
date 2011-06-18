namespace SharpArch.Testing.NUnit.NHibernate
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;

    using FluentNHibernate.Automapping;

    using SharpArch.Domain;
    using SharpArch.NHibernate;
    using SharpArch.NHibernate.FluentNHibernate;

    using global::NHibernate.Tool.hbm2ddl;

    using Configuration = global::NHibernate.Cfg.Configuration;

    /// <summary>
    ///     Provides helper methods for consolidating duplicated code from test fixture base classes.
    /// </summary>
    public class RepositoryTestsHelper
    {
        public static void FlushSessionAndEvict(object instance)
        {
            // Commits any changes up to this point to the database
            NHibernateSession.Current.Flush();

            // Evicts the instance from the current session so that it can be loaded during testing;
            // this gives the test a clean slate, if you will, to work with
            NHibernateSession.Current.Evict(instance);
        }

        public static AutoPersistenceModel GetAutoPersistenceModel(string[] assemblies)
        {
            return (from asmName in assemblies
                    select Assembly.Load(asmName)
                    into asm 
                    from asmType in asm.GetTypes()
                    where typeof(IAutoPersistenceModelGenerator).IsAssignableFrom(asmType)
                    select Activator.CreateInstance(asmType) as IAutoPersistenceModelGenerator
                    into generator 
                    select generator.Generate()).FirstOrDefault();
        }

        public static string[] GetMappingAssemblies()
        {
            var mappingAssembliesSetting = ConfigurationManager.AppSettings["nhibernate.mapping.assembly"];

            Check.Require(
                !string.IsNullOrEmpty(mappingAssembliesSetting), 
                "Please add an AppSetting to your app.config for 'nhibernate.mapping.assembly.' This setting " +
                "takes a comma delimited list of assemblies containing NHibernate mapping files. Including '.dll' " +
                "at the end of each is optional.");

            return mappingAssembliesSetting.Split(',');
        }

        public static void InitializeDatabase()
        {
            var cfg = InitializeNHibernateSession();
            var connection = NHibernateSession.Current.Connection;
            new SchemaExport(cfg).Execute(false, true, false, connection, null);
        }

        public static Configuration InitializeNHibernateSession()
        {
            var mappingAssemblies = GetMappingAssemblies();
            var autoPersistenceModel = GetAutoPersistenceModel(mappingAssemblies);
            return NHibernateSession.Init(new SimpleSessionStorage(), mappingAssemblies, autoPersistenceModel);
        }

        public static void Shutdown()
        {
            NHibernateSession.CloseAllSessions();
            NHibernateSession.Reset();
        }
    }
}