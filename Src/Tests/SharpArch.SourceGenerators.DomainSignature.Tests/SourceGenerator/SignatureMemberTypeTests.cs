namespace Tests.SharpArch.SourceGenerators.DomainSignature.SourceGenerator;

using global::SharpArch.SourceGenerators.DomainSignature.Impl;
using Shouldly;


public class SignatureMemberTypeTests
{
    [Fact]
    public void SignatureMemberType_AllValuesAreDefined()
    {
        var values = Enum.GetValues<SignatureMemberType>();

        values.Length.ShouldBe(3);
        values.ShouldContain(SignatureMemberType.Value);
        values.ShouldContain(SignatureMemberType.NullableValue);
        values.ShouldContain(SignatureMemberType.Reference);
    }
}