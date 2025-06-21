namespace Suteki.TardisBank.Tests.Model;

using Infrastructure.NHibernateMaps;
using SharpArch.Testing.NHibernate;


public class TransientDatabaseSetup : TestDatabaseSetup
{
    /// <inheritdoc />
    public TransientDatabaseSetup()
        : base(typeof(TransientDatabaseSetup).Assembly, typeof(AutoPersistenceModelGenerator), new[]
        {
            typeof(AutoPersistenceModelGenerator).Assembly
        })
    {
    }
}