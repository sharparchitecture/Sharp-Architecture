namespace Tests.SharpArch.NHibernate;

using Domain;
using FluentAssertions;
using NUnit.Framework;


[TestFixture]
// ReSharper disable once TestFileNameWarning
class HasUniqueDomainSignatureValidatorTests : HasUniqueDomainSignatureTestsBase
{
    [Test]
    public void WhenEntityWithDuplicateGuidExists_Should_MarkEntityAsInvalid()
    {
        var objectWithGuidId = new ObjectWithGuidId { Name = "codai" };
        SaveAndEvict(objectWithGuidId);
        var duplicateObjectWithGuidId = new ObjectWithGuidId { Name = "codai" };

        duplicateObjectWithGuidId.IsValid(ValidationContextFor(duplicateObjectWithGuidId))
            .Should().BeFalse();
    }

    [Test]
    public void WhenEntityWithDuplicateIntIdExists_Should_MarkEntityAsInvalid()
    {
        var contractor = new Contractor { Name = "codai" };
        SaveAndEvict(contractor);
        var duplicateContractor = new Contractor { Name = "codai" };
        duplicateContractor.IsValid(ValidationContextFor(duplicateContractor))
            .Should().BeFalse();
    }

    [Test]
    public void WhenEntityWithDuplicateStringIdExists_Should_MarkEntityAsInvalid()
    {
        var user = new User("user1", "123-12-1234");
        SaveAndEvict(user);
        var duplicateUser = new User("user2", "123-12-1234");

        duplicateUser.IsValid(ValidationContextFor(duplicateUser))
            .Should().BeFalse();
    }
}
