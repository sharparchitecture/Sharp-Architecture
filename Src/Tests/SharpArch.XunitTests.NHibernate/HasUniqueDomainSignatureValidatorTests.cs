namespace Tests.SharpArch.NHibernate
{
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using FluentAssertions;
    using Xunit;


    public class HasUniqueDomainSignatureValidatorTests : HasUniqueDomainSignatureTestsBase
    {
        /// <inheritdoc />
        protected override Task LoadTestData(CancellationToken cancellationToken)
            => Task.CompletedTask;

        [Fact]
        public async Task WhenEntityWithDuplicateGuidExists_Should_MarkEntityAsInvalid()
        {
            var objectWithGuidId = new ObjectWithGuidId {Name = "codai"};
            await SaveAndEvict(objectWithGuidId, CancellationToken.None);
            var duplicateObjectWithGuidId = new ObjectWithGuidId {Name = "codai"};

            duplicateObjectWithGuidId.IsValid(ValidationContextFor(duplicateObjectWithGuidId))
                .Should().BeFalse();
        }

        [Fact]
        public async Task WhenEntityWithDuplicateIntIdExists_Should_MarkEntityAsInvalid()
        {
            var contractor = new Contractor {Name = "codai"};
            await SaveAndEvict(contractor, CancellationToken.None);
            var duplicateContractor = new Contractor {Name = "codai"};
            duplicateContractor.IsValid(ValidationContextFor(duplicateContractor))
                .Should().BeFalse();
        }

        [Fact]
        public async Task WhenEntityWithDuplicateStringIdExists_Should_MarkEntityAsInvalid()
        {
            var user = new User("user1", "123-12-1234");
            await SaveAndEvict(user, CancellationToken.None);
            var duplicateUser = new User("user2", "123-12-1234");

            duplicateUser.IsValid(ValidationContextFor(duplicateUser))
                .Should().BeFalse();
        }
    }
}
