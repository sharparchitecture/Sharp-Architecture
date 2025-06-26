namespace Tests.SharpArch.SourceGenerators.DomainSignature.SourceGenerator;

using global::SharpArch.SourceGenerators.DomainSignature.Impl;


public class SignatureMemberTypeTests
{
    [Fact]
    public void SignatureMemberType_AllValuesAreDefined()
    {
        var values = Enum.GetValues<SignatureMemberType>();

        // Assert
        Assert.Equal(3, values.Length);
        Assert.Contains(SignatureMemberType.Value, values);
        Assert.Contains(SignatureMemberType.NullableValue, values);
        Assert.Contains(SignatureMemberType.Reference, values);
    }
}
