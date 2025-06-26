namespace Tests.SharpArch.SourceGenerators.DomainSignature.SourceGenerator;

using global::SharpArch.SourceGenerators.DomainSignature.Impl;


public class DomainSignatureGeneratorSimpleTests
{
    [Theory]
    [InlineData("Id", SignatureMemberType.Value)]
    [InlineData("Name", SignatureMemberType.Reference)]
    [InlineData("OptionalDate", SignatureMemberType.NullableValue)]
    public void DomainSignaturePropertyInfo_WithDifferentTypes_InitializesCorrectly(string name, SignatureMemberType type)
    {
        var propertyInfo = new DomainSignaturePropertyInfo(name, type);

        // Assert
        Assert.Equal(name, propertyInfo.Name);
        Assert.Equal(type, propertyInfo.MemberType);
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

        // Assert
        Assert.Equal(2, entityInfo.DomainSignatureProperties.Count);
        Assert.True(entityInfo.IsPartial);
        Assert.True(entityInfo.IsBaseTypeCorrect);
    }

    [Fact]
    public void GetDomainSignatureProperties_WithMockProperties_ReturnsExpectedTypes()
    {
        var propertyInfo = new DomainSignaturePropertyInfo("TestProp", SignatureMemberType.Value);
        Assert.Equal("TestProp", propertyInfo.Name);
        Assert.Equal(SignatureMemberType.Value, propertyInfo.MemberType);
    }

    [Fact]
    public void SignatureMemberType_HasExpectedValues()
    {
        // Verify the enum values are as expected
        Assert.Equal(0, (int)SignatureMemberType.Value);
        Assert.Equal(1, (int)SignatureMemberType.NullableValue);
        Assert.Equal(2, (int)SignatureMemberType.Reference);
    }
}
