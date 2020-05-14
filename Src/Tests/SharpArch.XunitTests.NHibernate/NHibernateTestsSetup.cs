namespace Tests.SharpArch.NHibernate
{
    using System.Collections.Generic;
    using System.Reflection;
    using Domain;
    using FluentNHibernate.Cfg.Db;
    using global::NHibernate.Cfg;
    using global::SharpArch.NHibernate;
    using global::SharpArch.Testing.NHibernate;
    using Mappings;


    public class NHibernateTestsSetup : TestDatabaseSetup
    {
        public NHibernateTestsSetup()
            : base(Assembly.GetExecutingAssembly().Location,
                new[]
                {
                    typeof(ObjectWithGuidId).Assembly,
                    typeof(TestsPersistenceModelGenerator).Assembly
                })
        {
        }

        /// <inheritdoc />
        protected override void Customize(NHibernateSessionFactoryBuilder builder)
        {
            base.Customize(builder);
            builder.UsePersistenceConfigurer(new SQLiteConfiguration().InMemory());
            builder.UseProperties(new SortedList<string, string>()
            {
                [Environment.ReleaseConnections] = "on_close",
                [Environment.Hbm2ddlAuto] = "create"
            });
        }
    }
}
