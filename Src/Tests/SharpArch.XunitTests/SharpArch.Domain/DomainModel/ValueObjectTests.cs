namespace Tests.SharpArch.Domain.DomainModel;

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using global::SharpArch.Domain.DomainModel;
using Xunit;


public class ValueObjectTests
{
    public class AnotherDummyValueType : ValueObject
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }


    public class DummyValueType : ValueObject
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }


    /// <summary>
    ///     This is a nonsense object; i.e., it doesn't make sense to have
    ///     a value object with a domain signature.
    /// </summary>
    public class ValueObjectWithDomainSignature : ValueObject
    {
        [DomainSignature]
        public string? Name { get; set; }
    }


    [Fact]
    public void CannotHaveValueObjectWithDomainSignatureProperties()
    {
        var invalidValueObject = new ValueObjectWithDomainSignature();

        Assert.Throws<InvalidOperationException>(() => invalidValueObject.GetSignatureProperties());
    }

    [Fact]
    public void Equality_DifferentReferences_SameValues_True()
    {
        var valueObj1 = new DummyValueType { Id = 1, Name = "Luis" };
        var valueObj2 = new DummyValueType { Id = 1, Name = "Luis" };
        valueObj1.Should().NotBeSameAs(valueObj2);
        valueObj1.Equals(valueObj2).Should().BeTrue();
        (valueObj1 == valueObj2).Should().BeTrue();

        valueObj2.Name = "Billy";
        (valueObj1 == valueObj2).Should().BeFalse();
    }

    [Fact]
    public void ShouldBeEqualSameReferenceWithNonNullValues()
    {
        var val1 = new DummyValueType { Id = 1, Name = "Luis" };
        // ReSharper disable once EqualExpressionComparison
        val1.Equals(val1).Should().BeTrue();
    }

    [Fact]
    public void ShouldBeEqualWithDifferentReferences()
    {
        var val1 = new DummyValueType { Id = 1, Name = "Luis" };
        var val2 = new DummyValueType { Id = 1, Name = "Luis" };
        val1.Equals(val2).Should().BeTrue();
    }

    [Fact]
    public void ShouldBeEqualWithSameReference()
    {
        var val1 = new DummyValueType();
        // ReSharper disable once EqualExpressionComparison
        val1.Equals(val1).Should().BeTrue();
    }

    [Fact]
    public void ShouldCompareAndReturnNotEqualWithOperators()
    {
        var val1 = new DummyValueType { Id = 10, Name = @"jose" };
        var val2 = new DummyValueType { Id = 20, Name = @"Rui" };

        (val1 == val2).Should().BeFalse();
        (val1 != val2).Should().BeTrue();
    }

    [Fact]
    public void ShouldGenerateSameHashcodeWhenEquals()
    {
        var val1 = new DummyValueType { Id = 10, Name = "Miguel" };
        var val2 = new DummyValueType { Id = 10, Name = "Miguel" };
        val1.GetHashCode().Should().Be(val2.GetHashCode());
    }

    [Fact]
    public void ShouldNotBeEqualToNull()
    {
        var val1 = new DummyValueType { Id = 1, Name = "Luis" };
        val1.Equals(null).Should().BeFalse();
    }

    [Fact]
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse", Justification = "unit tests")]
    public void ShouldNotBeEqualToNullWithOperators()
    {
        var val1 = new DummyValueType { Id = 1, Name = "Luis" };

        (null == val1).Should().BeFalse();
        (val1 == null).Should().BeFalse();
        (null! != val1).Should().BeTrue();
        (val1 != null).Should().BeTrue();
    }

    [Fact]
    public void ShouldNotBeEqualWhenComparingDifferentTypes()
    {
        var val1 = new DummyValueType { Id = 1, Name = "Luis" };
        var val2 = new AnotherDummyValueType { Id = 1, Name = "Luis" };
        // ReSharper disable once SuspiciousTypeConversion.Global
        val2.Equals(val1).Should().BeFalse();
    }

    [Fact]
    public void ShouldNotBeEqualWithDifferentReferencesAndDifferentIds()
    {
        var val1 = new DummyValueType { Id = 1, Name = "Luis" };
        var val2 = new DummyValueType { Id = 10, Name = "Luis" };
        val2.Equals(val1).Should().BeFalse();
    }
}
