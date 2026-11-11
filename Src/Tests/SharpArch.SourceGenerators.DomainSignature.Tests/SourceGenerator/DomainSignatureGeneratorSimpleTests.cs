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

        var entityInfo = new EntityInfo("Test", "Entity", properties);

        entityInfo.DomainSignatureProperties.Count.ShouldBe(2);
        entityInfo.Errors.ShouldBe(GenerationError.None);
    }

    [Fact]
    public void EntityInfo_Defaults_HaveNoErrorsAndKindClass()
    {
        var entity = new EntityInfo("Test", "Entity", new List<DomainSignaturePropertyInfo>());

        entity.Errors.ShouldBe(GenerationError.None);
        entity.IsRecord.ShouldBeFalse();
        entity.UnsupportedPropertyNames.Count.ShouldBe(0);
    }

    [Fact]
    public void EntityInfo_ExplicitValues_ArePreserved()
    {
        var entity = new EntityInfo("Test", "Entity", new List<DomainSignaturePropertyInfo>(),
            GenerationError.NotPartial | GenerationError.NoNamespace | GenerationError.Nested,
            isRecord: true);

        entity.Errors.ShouldBe(GenerationError.NotPartial | GenerationError.NoNamespace | GenerationError.Nested);
        entity.IsRecord.ShouldBeTrue();
    }
}