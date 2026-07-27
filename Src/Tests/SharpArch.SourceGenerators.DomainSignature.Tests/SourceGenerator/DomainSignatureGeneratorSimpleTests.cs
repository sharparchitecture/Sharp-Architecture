namespace Tests.SharpArch.SourceGenerators.DomainSignature.SourceGenerator;

using global::SharpArch.SourceGenerators.DomainSignature.Impl;
using Shouldly;


public class DomainSignatureGeneratorSimpleTests
{
    [Theory]
    [InlineData("Id", SignatureMemberType.Value)]
    [InlineData("Name", SignatureMemberType.Reference)]
    [InlineData("OptionalDate", SignatureMemberType.NullableValue)]
    public void DomainSignaturePropertyInfo_WithDifferentTypes_InitializesCorrectly(string name, SignatureMemberType type)
    {
        var propertyInfo = new DomainSignaturePropertyInfo(name, type);

        propertyInfo.Name.ShouldBe(name);
        propertyInfo.MemberType.ShouldBe(type);
    }

    [Fact]
    public void EntityInfo_WithProperties_CreatesEquatableArray()
    {
        var properties = new List<DomainSignaturePropertyInfo> { new("Id", SignatureMemberType.Value), new("Name", SignatureMemberType.Reference) };

        var entityInfo = new EntityInfo(
            "Test",
            "Entity",
            properties,
            true,
            true);

        entityInfo.DomainSignatureProperties.Count.ShouldBe(2);
        entityInfo.IsPartial.ShouldBeTrue();
        entityInfo.IsBaseTypeCorrect.ShouldBeTrue();
    }

    [Fact]
    public void GetDomainSignatureProperties_WithMockProperties_ReturnsExpectedTypes()
    {
        var propertyInfo = new DomainSignaturePropertyInfo("TestProp", SignatureMemberType.Value);

        propertyInfo.Name.ShouldBe("TestProp");
        propertyInfo.MemberType.ShouldBe(SignatureMemberType.Value);
    }

    [Fact]
    public void SignatureMemberType_HasExpectedValues()
    {
        ((int)SignatureMemberType.Value).ShouldBe(0);
        ((int)SignatureMemberType.NullableValue).ShouldBe(1);
        ((int)SignatureMemberType.Reference).ShouldBe(2);
    }

    [Fact]
    public void EntityInfo_NewOptionalFields_DefaultToValidValues()
    {
        var entity = new EntityInfo("Test", "Entity", new List<DomainSignaturePropertyInfo>(), true, true);

        entity.HasNamespace.ShouldBeTrue();
        entity.IsNested.ShouldBeFalse();
        entity.IsRecord.ShouldBeFalse();
    }

    [Fact]
    public void EntityInfo_NewOptionalFields_ExplicitValuesArePreserved()
    {
        var entity = new EntityInfo("Test", "Entity", new List<DomainSignaturePropertyInfo>(), true, false,
            hasNamespace: false, isNested: true, isRecord: true);

        entity.HasNamespace.ShouldBeFalse();
        entity.IsNested.ShouldBeTrue();
        entity.IsRecord.ShouldBeTrue();
    }
}