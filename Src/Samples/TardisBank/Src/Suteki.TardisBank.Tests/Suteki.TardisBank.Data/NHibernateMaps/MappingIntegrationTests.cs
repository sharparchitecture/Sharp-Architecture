namespace Suteki.TardisBank.Tests.Suteki.TardisBank.Data.NHibernateMaps;

using Domain;
using Helpers;
using Infrastructure.NHibernateMaps;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using SharpArch.NHibernate;
using Xunit;


/// <summary>
///     Provides a means to verify that the target database is in compliance with all mappings.
///     Taken from http://ayende.com/Blog/archive/2006/08/09/NHibernateMappingCreatingSanityChecks.aspx.
///     If this is failing, the error will likely inform you that there is a missing table or column
///     which needs to be added to your database.
/// </summary>
[Trait("Category", "DatabaseTests")]
[Trait("Category", "IntegrationTests")]
public class MappingIntegrationTests : IDisposable
{
    readonly Configuration _configuration;
    readonly ISessionFactory _sessionFactory;
    readonly ISession _session;

    public MappingIntegrationTests()
    {
        var nhibernateConfigPath = CalculatePath("../../../../Suteki.TardisBank.WebApi/NHibernate.config");
        _configuration = new NHibernateSessionFactoryBuilder()
            .AddMappingAssemblies(new[] { typeof(Child).Assembly })
            .UseAutoPersistenceModel(new AutoPersistenceModelGenerator().Generate())
            .UseConfigFile(nhibernateConfigPath)
            .BuildConfiguration();
        _sessionFactory = _configuration.BuildSessionFactory();
        _session = _sessionFactory.OpenSession();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _sessionFactory.Dispose();
    }

    /// <summary>
    ///     Calculates path based on test assembly folder
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    static string CalculatePath(string path)
        => Path.Combine(".", path);

    /// <summary>
    ///     Creates/Updates database schema, this runs on database configured in
    ///     Mvc project and is marked as Explicit because it changes the database.
    /// </summary>
    [RunnableInDebugOnly]
    public void CanCreateDatabase()
    {
        new SchemaExport(_configuration).Execute(false, true, false);
    }

    [Fact]
    public void CanConfirmDatabaseMatchesMappings()
    {
        var allClassMetadata = _sessionFactory.GetAllClassMetadata();

        foreach (var entry in allClassMetadata)
        {
            _session.CreateCriteria(entry.Value.MappedClass)
                .SetMaxResults(0).List();
        }
    }

    /// <summary>
    ///     Generates and outputs the database schema SQL to the console
    /// </summary>
    [Fact]
    public void CanGenerateDatabaseSchema()
    {
        using (TextWriter stringWriter = new StreamWriter(CalculatePath("../../../../../Database/UnitTestGeneratedSchema.sql")))
        {
            new SchemaExport(_configuration).Execute(true, false, false, _session.Connection, stringWriter);
        }
    }
}
