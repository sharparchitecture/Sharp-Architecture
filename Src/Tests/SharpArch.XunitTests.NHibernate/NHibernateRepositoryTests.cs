namespace Tests.SharpArch.NHibernate;

using Domain;
using Shouldly;
using global::SharpArch.NHibernate;
using global::SharpArch.Testing.Xunit.NHibernate;
using Xunit;


public class NHibernateRepositoryTests : TransientDatabaseTests<NHibernateTestsSetup>
{
    readonly NHibernateRepository<Contractor, int> _repo;

    /// <inheritdoc />
    public NHibernateRepositoryTests(NHibernateTestsSetup setup)
        : base(setup)
    {
        _repo = new NHibernateRepository<Contractor, int>(TransactionManager);
    }

    /// <inheritdoc />
    protected override Task LoadTestData(CancellationToken cancellationToken)
        => Task.CompletedTask;

    [Fact]
    public async Task CanSaveAsync()
    {
        var entity = new Contractor
        {
            Name = "John Doe"
        };

        var res = await _repo.SaveAsync(entity);
        res.IsTransient().ShouldBeFalse();
        res.Id.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task CanSaveOrUpdate()
    {
        var entity = new Contractor
        {
            Name = "John Doe"
        };
        var res = await _repo.SaveOrUpdateAsync(entity);
        res.IsTransient().ShouldBeFalse();

        entity.Name = "John Doe Jr";
        res = await _repo.SaveOrUpdateAsync(entity);
        res.Name.ShouldBe("John Doe Jr");
    }
}