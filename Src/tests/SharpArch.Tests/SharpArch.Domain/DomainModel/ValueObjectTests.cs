// ReSharper disable PublicMembersMustHaveComments
// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident
// ReSharper disable HeapView.BoxingAllocation

// ReSharper disable HeapView.DelegateAllocation
namespace Tests.SharpArch.Domain.DomainModel
{
    using System;
    using global::SharpArch.Domain.DomainModel;
    using NUnit.Framework;

    [TestFixture]
    public class ValueObjectTests
    {
        public class AnotherDummyValueType : ValueObject
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public class DummyValueType : ValueObject
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        /// <summary>
        ///     This is a nonsense object; i.e., it doesn't make sense to have
        ///     a value object with a domain signature.
        /// </summary>
        public class ValueObjectWithDomainSignature : ValueObject
        {
            [DomainSignature]
            public string Name { get; set; }
        }

        [Test]
        public void CannotHaveValueObjectWithDomainSignatureProperties()
        {
            var invalidValueObject = new ValueObjectWithDomainSignature();

            Assert.Throws<InvalidOperationException>(() => invalidValueObject.GetSignatureProperties());
        }

        [Test]
        public void Equality_DifferentReferences_SameValues_True()
        {
            var valueObject1 = new DummyValueType {Id = 1, Name = "Luis"};
            var valueObject2 = new DummyValueType {Id = 1, Name = "Luis"};
            Assert.That(valueObject1, Is.Not.SameAs(valueObject2));
            Assert.That(valueObject1, Is.EqualTo(valueObject2));
            Assert.That(valueObject1.Equals(valueObject2));
            Assert.That(valueObject1 == valueObject2);

            valueObject2.Name = "Billy";
            Assert.That(valueObject1 != valueObject2);
        }

        [Test]
        public void ShouldBeEqualSameReferenceWithNonNullValues()
        {
            var valType = new DummyValueType {Id = 1, Name = "Luis"};
            Assert.AreEqual(valType, valType);
        }

        [Test]
        public void ShouldBeEqualWithDifferentReferences()
        {
            var valType = new DummyValueType {Id = 1, Name = "Luis"};
            var anotherValType = new DummyValueType {Id = 1, Name = "Luis"};
            Assert.AreEqual(anotherValType, valType);
        }

        [Test]
        public void ShouldBeEqualWithSameReference()
        {
            var valType = new DummyValueType();
            Assert.AreEqual(valType, valType);
        }

        [Test]
        public void ShouldCompareAndReturnNotEqualWithOperators()
        {
            var valType = new DummyValueType {Id = 10, Name = @"jose"};
            var anotherValType = new DummyValueType {Id = 20, Name = @"Rui"};

            Assert.IsFalse(valType == anotherValType);
            Assert.IsTrue(valType != anotherValType);
        }

        [Test]
        public void ShouldGenerateSameHashcodeWhenEquals()
        {
            var valType = new DummyValueType {Id = 10, Name = "Miguel"};
            var anotherValType = new DummyValueType {Id = 10, Name = "Miguel"};
            Assert.AreEqual(valType.GetHashCode(), anotherValType.GetHashCode());
        }

        [Test]
        public void ShouldNotBeEqualToNull()
        {
            var valType = new DummyValueType {Id = 1, Name = "Luis"};
            Assert.AreNotEqual(null, valType);
            Assert.AreNotEqual(valType, null);
        }

        [Test]
        public void ShouldNotBeEqualToNullWithOperators()
        {
            var valType = new DummyValueType {Id = 1, Name = "Luis"};

            Assert.IsFalse(null == valType);
            Assert.IsFalse(valType == null);
            Assert.IsTrue(null != valType);
            Assert.IsTrue(valType != null);
        }

        [Test]
        public void ShouldNotBeEqualWhenComparingDifferentTypes()
        {
            var valType = new DummyValueType {Id = 1, Name = "Luis"};
            var anotherType = new AnotherDummyValueType {Id = 1, Name = "Luis"};
            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.IsFalse(anotherType.Equals(valType));
        }

        [Test]
        public void ShouldNotBeEqualWithDifferentReferencesAndDifferentIds()
        {
            var valType = new DummyValueType {Id = 1, Name = "Luis"};
            var anotherValType = new DummyValueType {Id = 10, Name = "Luis"};
            Assert.AreNotEqual(anotherValType, valType);
        }
    }
}
