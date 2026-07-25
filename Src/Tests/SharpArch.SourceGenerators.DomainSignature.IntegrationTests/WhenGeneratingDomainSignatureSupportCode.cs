namespace Tests.SharpArch.SourceGenerators.DomainSignature;

using System.ComponentModel.DataAnnotations;
using global::SharpArch.Domain.DomainModel;
using Shouldly;


[GenerateDomainSignature]
public partial class EntityWithReferenceType : Entity<int>
{
    [Required]
    [DomainSignature]
    public string Name { get; set; } = default!;

    [DomainSignature]
    public string? Description { get; set; }
}


[GenerateDomainSignature]
public partial class EntityWithMixedTypes : Entity<int>
{
    [DomainSignature]
    public int Code { get; set; }

    [Required]
    [DomainSignature]
    public string Name { get; set; } = default!;

    [DomainSignature]
    public string? Description { get; set; }

    [DomainSignature]
    public int? AltCode { get; set; }
}


public class WhenGeneratingDomainSignatureSupportCode
{
    [Theory]
    [InlineData(1, "Name1", "Desc1", 10)]
    [InlineData(2, "Name2", null, null)]
    [InlineData(3, "Name3", "Desc3", null)]
    [InlineData(4, "Name4", null, 40)]
    public void EntityWithMixedTypes_GetHashCode_IsConsistentForSameValues(int code, string name, string? description, int? altCode)
    {
        var entity1 = new EntityWithMixedTypes { Code = code, Name = name, Description = description, AltCode = altCode };

        var entity2 = new EntityWithMixedTypes { Code = code, Name = name, Description = description, AltCode = altCode };

        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2);
        entity1.HasSameObjectSignatureAs(entity2).ShouldBeTrue();
    }

    [Fact]
    public void EntityWithMixedTypes_GetHashCode_GeneratesConsistentHashCodes()
    {
        var entity1 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = "Test Description", AltCode = 456 };

        var entity2 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = "Test Description", AltCode = 456 };

        var entity3 = new EntityWithMixedTypes { Code = 124, Name = "Test Entity", Description = "Test Description", AltCode = 456 };

        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();
        var hash3 = entity3.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2);
        hash1.ShouldNotBe(hash3);
    }

    [Fact]
    public void EntityWithMixedTypes_GetHashCode_HandlesNullableValues()
    {
        var entity1 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = null, AltCode = null };

        var entity2 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = null, AltCode = null };

        var entity3 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = null, AltCode = 456 };

        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();
        var hash3 = entity3.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2);
        hash1.ShouldNotBe(hash3);
    }

    [Fact]
    public void EntityWithMixedTypes_HasSameObjectSignatureAs_ComparesAllProperties()
    {
        var entity1 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = "Test Description", AltCode = 456 };

        var entity2 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = "Test Description", AltCode = 456 };

        var entityDifferentCode = new EntityWithMixedTypes { Code = 124, Name = "Test Entity", Description = "Test Description", AltCode = 456 };

        var entityDifferentName = new EntityWithMixedTypes { Code = 123, Name = "Different Entity", Description = "Test Description", AltCode = 456 };

        var entityDifferentDescription = new EntityWithMixedTypes
        {
            Code = 123, Name = "Test Entity", Description = "Different Description", AltCode = 456
        };

        var entityDifferentAltCode = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = "Test Description", AltCode = 457 };

        entity1.HasSameObjectSignatureAs(entity2).ShouldBeTrue();
        entity1.HasSameObjectSignatureAs(entityDifferentCode).ShouldBeFalse();
        entity1.HasSameObjectSignatureAs(entityDifferentName).ShouldBeFalse();
        entity1.HasSameObjectSignatureAs(entityDifferentDescription).ShouldBeFalse();
        entity1.HasSameObjectSignatureAs(entityDifferentAltCode).ShouldBeFalse();
    }

    [Fact]
    public void EntityWithMixedTypes_HasSameObjectSignatureAs_HandlesNullableValues()
    {
        var entity1 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = null, AltCode = null };

        var entity2 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = null, AltCode = null };

        var entity3 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = "Some Description", AltCode = null };

        var entity4 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = null, AltCode = 456 };

        entity1.HasSameObjectSignatureAs(entity2).ShouldBeTrue();
        entity1.HasSameObjectSignatureAs(entity3).ShouldBeFalse();
        entity1.HasSameObjectSignatureAs(entity4).ShouldBeFalse();
    }

    [Fact]
    public void EntityWithMixedTypes_HasSameObjectSignatureAs_ReturnsFalseForDifferentType()
    {
        var entity1 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = "Test Description", AltCode = 456 };

        var entity2 = new EntityWithReferenceType { Name = "Test Entity", Description = "Test Description" };

        entity1.HasSameObjectSignatureAs(entity2).ShouldBeFalse();
    }

    [Fact]
    public void EntityWithMixedTypes_HasSameObjectSignatureAs_ReturnsFalseForNull()
    {
        var entity = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = "Test Description", AltCode = 456 };

        entity.HasSameObjectSignatureAs(null!).ShouldBeFalse();
    }

    [Fact]
    public void EntityWithReferenceType_GetHashCode_GeneratesConsistentHashCodes()
    {
        var entity1 = new EntityWithReferenceType { Name = "Test Entity", Description = "Test Description" };

        var entity2 = new EntityWithReferenceType { Name = "Test Entity", Description = "Test Description" };

        var entity3 = new EntityWithReferenceType { Name = "Different Entity", Description = "Test Description" };

        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();
        var hash3 = entity3.GetHashCode();

        hash1.ShouldBe(hash2);
        hash1.ShouldNotBe(hash3);
    }

    [Fact]
    public void EntityWithReferenceType_GetHashCode_HandlesNullDescription()
    {
        var entity1 = new EntityWithReferenceType { Name = "Test Entity", Description = null };

        var entity2 = new EntityWithReferenceType { Name = "Test Entity", Description = null };

        var entity3 = new EntityWithReferenceType { Name = "Test Entity", Description = "Some Description" };

        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();
        var hash3 = entity3.GetHashCode();

        hash1.ShouldBe(hash2);
        hash1.ShouldNotBe(hash3);
    }

    [Fact]
    public void EntityWithReferenceType_HasSameObjectSignatureAs_ComparesCorrectly()
    {
        var entity1 = new EntityWithReferenceType { Name = "Test Entity", Description = "Test Description" };

        var entity2 = new EntityWithReferenceType { Name = "Test Entity", Description = "Test Description" };

        var entity3 = new EntityWithReferenceType { Name = "Different Entity", Description = "Test Description" };

        var entity4 = new EntityWithReferenceType { Name = "Test Entity", Description = "Different Description" };

        entity1.HasSameObjectSignatureAs(entity2).ShouldBeTrue();
        entity1.HasSameObjectSignatureAs(entity3).ShouldBeFalse();
        entity1.HasSameObjectSignatureAs(entity4).ShouldBeFalse();
    }

    [Fact]
    public void EntityWithReferenceType_HasSameObjectSignatureAs_HandlesNullDescription()
    {
        var entity1 = new EntityWithReferenceType { Name = "Test Entity", Description = null };

        var entity2 = new EntityWithReferenceType { Name = "Test Entity", Description = null };

        var entity3 = new EntityWithReferenceType { Name = "Test Entity", Description = "Some Description" };

        entity1.HasSameObjectSignatureAs(entity2).ShouldBeTrue();
        entity1.HasSameObjectSignatureAs(entity3).ShouldBeFalse();
    }

    [Fact]
    public void EntityWithReferenceType_HasSameObjectSignatureAs_ReturnsFalseForDifferentType()
    {
        var entity1 = new EntityWithReferenceType { Name = "Test Entity", Description = "Test Description" };

        var entity2 = new EntityWithMixedTypes { Code = 123, Name = "Test Entity", Description = "Test Description", AltCode = null };

        entity1.HasSameObjectSignatureAs(entity2).ShouldBeFalse();
    }

    [Fact]
    public void EntityWithReferenceType_HasSameObjectSignatureAs_ReturnsFalseForNull()
    {
        var entity = new EntityWithReferenceType { Name = "Test Entity", Description = "Test Description" };

        entity.HasSameObjectSignatureAs(null!).ShouldBeFalse();
    }
}
