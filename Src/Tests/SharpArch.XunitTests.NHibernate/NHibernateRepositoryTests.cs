namespace Tests.SharpArch.NHibernate
{
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using FluentAssertions;
    using global::SharpArch.NHibernate;
    using global::SharpArch.Testing.Xunit.NHibernate;
    using Mappings;
    using Xunit;


    public class NHibernateRepositoryTests : TransientDatabaseTests<NHibernateTestsSetup>
    {
        readonly NHibernateRepository<Contractor, int> _repo;

        /// <inheritdoc />
        public NHibernateRepositoryTests(NHibernateTestsSetup setup): base(setup)
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

            var res = await _repo.SaveAsync(entity).ConfigureAwait(false);
            res.IsTransient().Should().BeFalse();
            res.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CanSaveOrUpdate()
        {
            var entity = new Contractor
            {
                Name = "John Doe"
            };
            var res = await _repo.SaveOrUpdateAsync(entity).ConfigureAwait(false);
            res.IsTransient().Should().BeFalse();

            entity.Name = "John Doe Jr";
            res = await _repo.SaveOrUpdateAsync(entity).ConfigureAwait(false);
            res.Name.Should().Be("John Doe Jr");
        }
    }
}
