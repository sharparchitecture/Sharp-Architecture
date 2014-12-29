using System.IO;

using Newtonsoft.Json;

namespace Tests.SharpArch.Domain.DomainModel
{
    using global::SharpArch.Domain.DomainModel;

    using NUnit.Framework;

    using global::SharpArch.Testing.NUnit;
    using global::SharpArch.Testing.NUnit.Helpers;

    [TestFixture]
    public class EntityTests
    {
        private MockEntityObjectWithDefaultId _diffObj;

        private MockEntityObjectWithSetId _diffObjWithId;

        private MockEntityObjectWithDefaultId _obj;

        private MockEntityObjectWithSetId _objWithId;

        private MockEntityObjectWithDefaultId _sameObj;

        private MockEntityObjectWithSetId _sameObjWithId;

        [Test]
        public void CanCompareDomainObjectsWithAllPropertiesBeingPartOfDomainSignature()
        {
            var object1 = new ObjectWithAllDomainSignatureProperty();
            var object2 = new ObjectWithAllDomainSignatureProperty();
            Assert.That(object1, Is.EqualTo(object2));

            object1.Age = 13;
            object2.Age = 13;
            object1.Name = "Foo";
            object2.Name = "Foo";
            Assert.That(object1, Is.EqualTo(object2));

            object1.Name = "Bar";
            Assert.That(object1, Is.Not.EqualTo(object2));

            object1.Name = null;
            Assert.That(object1, Is.Not.EqualTo(object2));

            object2.Name = null;
            Assert.That(object1, Is.EqualTo(object2));
        }

        [Test]
        public void CanCompareDomainObjectsWithOnlySomePropertiesBeingPartOfDomainSignature()
        {
            var object1 = new ObjectWithOneDomainSignatureProperty();
            var object2 = new ObjectWithOneDomainSignatureProperty();
            Assert.That(object1, Is.EqualTo(object2));

            object1.Age = 13;
            object2.Age = 13;

            // Name property isn't included in comparison
            object1.Name = "Foo";
            object2.Name = "Bar";
            Assert.That(object1, Is.EqualTo(object2));

            object1.Age = 14;
            Assert.That(object1, Is.Not.EqualTo(object2));
        }

        [Test]
        public void CanCompareEntitiesWithAssignedIds()
        {
            var object1 = new ObjectWithAssignedId { Name = "Acme" };
            var object2 = new ObjectWithAssignedId { Name = "Anvil" };

            Assert.That(object1, Is.Not.EqualTo(null));
            Assert.That(object1, Is.Not.EqualTo(object2));

            object1.SetAssignedIdTo("AAAAA");
            object2.SetAssignedIdTo("AAAAA");

            // Even though the "business value signatures" are different, the persistent Ids 
            // were the same.  Call me crazy, but I put that much trust into persisted Ids.
            Assert.That(object1, Is.EqualTo(object2));

            var object3 = new ObjectWithAssignedId { Name = "Acme" };

            // Since object1 has an Id but object3 doesn't, they won't be equal
            // even though their signatures are the same
            Assert.That(object1, Is.Not.EqualTo(object3));

            var object4 = new ObjectWithAssignedId { Name = "Acme" };

            // object3 and object4 are both transient and share the same signature
            Assert.That(object3, Is.EqualTo(object4));
        }

        [Test]
        public void CanCompareEntitys()
        {
            var object1 = new ObjectWithIntId { Name = "Acme" };
            var object2 = new ObjectWithIntId { Name = "Anvil" };

            Assert.That(object1, Is.Not.EqualTo(null));
            Assert.That(object1, Is.Not.EqualTo(object2));

            EntityIdSetter.SetIdOf(object1, 10);
            EntityIdSetter.SetIdOf(object2, 10);

            // Even though the "business value signatures" are different, the persistent Ids 
            // were the same.  Call me crazy, but I put that much trust into persisted Ids.
            Assert.That(object1, Is.EqualTo(object2));
            Assert.That(object1.GetHashCode(), Is.EqualTo(object2.GetHashCode()));

            var object3 = new ObjectWithIntId { Name = "Acme" };

            // Since object1 has an Id but object3 doesn't, they won't be equal
            // even though their signatures are the same
            Assert.That(object1, Is.Not.EqualTo(object3));

            var object4 = new ObjectWithIntId { Name = "Acme" };

            // object3 and object4 are both transient and share the same signature
            Assert.That(object3, Is.EqualTo(object4));
        }

        [Test]
        public void CanCompareInheritedDomainObjects()
        {
            var object1 = new InheritedObjectWithExtraDomainSignatureProperty();
            var object2 = new InheritedObjectWithExtraDomainSignatureProperty();
            Assert.That(object1, Is.EqualTo(object2));

            object1.Age = 13;
            object1.IsLiving = true;
            object2.Age = 13;
            object2.IsLiving = true;

            // Address property isn't included in comparison
            object1.Address = "123 Oak Ln.";
            object2.Address = "Nightmare on Elm St.";
            Assert.That(object1, Is.EqualTo(object2));

            object1.IsLiving = false;
            Assert.That(object1, Is.Not.EqualTo(object2));
        }

        [Test]
        public void CanCompareObjectsWithComplexProperties()
        {
            var object1 = new ObjectWithComplexProperties();
            var object2 = new ObjectWithComplexProperties();

            Assert.AreEqual(object1, object2);

            object1.Address = new AddressBeingDomainSignatureComparble
                {
                   Address1 = "123 Smith Ln.", Address2 = "Suite 201", ZipCode = 12345 
                };
            Assert.AreNotEqual(object1, object2);

            // Set the address of the 2nd to be different to the address of the first
            object2.Address = new AddressBeingDomainSignatureComparble
                {
                    Address1 = "123 Smith Ln.", 

                    // Address2 isn't marked as being part of the domain signature; 
                    // therefore, it WON'T be used in the equality comparison
                    Address2 = "Suite 402", 
                    ZipCode = 98765
                };
            Assert.AreNotEqual(object1, object2);

            // Set the address of the 2nd to be the same as the first
            object2.Address.ZipCode = 12345;
            Assert.AreEqual(object1, object2);

            object1.Phone = new PhoneBeingNotDomainObject { PhoneNumber = "(555) 555-5555" };
            Assert.AreNotEqual(object1, object2);

            // IMPORTANT: Note that even though the phone number below has the same value as the 
            // phone number on object1, they're not domain signature comparable; therefore, the
            // "out of the box" equality will be used which shows them as being different objects.
            object2.Phone = new PhoneBeingNotDomainObject { PhoneNumber = "(555) 555-5555" };
            Assert.AreNotEqual(object1, object2);

            // Observe as we replace the object1.Phone with an object that isn't domain-signature
            // comparable, but DOES have an overridden Equals which will return true if the phone
            // number properties are equal.
            object1.Phone = new PhoneBeingNotDomainObjectButWithOverriddenEquals { PhoneNumber = "(555) 555-5555" };
            Assert.AreEqual(object1, object2);
        }

        [Test]
        public void CanHaveEntityWithoutDomainSignatureProperties()
        {
            var invalidEntity = new ObjectWithNoDomainSignatureProperties();

            invalidEntity.GetSignatureProperties();
        }

        [Test]
        public void CannotEquateObjectsWithSameIdButDifferentTypes()
        {
            var object1Type = new Object1();
            var object2Type = new Object2();

            EntityIdSetter.SetIdOf(object1Type, 1);
            EntityIdSetter.SetIdOf(object2Type, 1);

            Assert.That(object1Type, Is.Not.EqualTo(object2Type));
        }

        [Test]
        public void DoDefaultEntityEqualsOverrideWorkWhenIdIsAssigned()
        {
            this._obj.SetAssignedIdTo(1);
            this._diffObj.SetAssignedIdTo(1);
            Assert.That(this._obj.Equals(this._diffObj));
        }

        [Test]
        public void DoEntityEqualsOverrideWorkWhenIdIsAssigned()
        {
            this._objWithId.SetAssignedIdTo("1");
            this._diffObjWithId.SetAssignedIdTo("1");
            Assert.That(this._objWithId.Equals(this._diffObjWithId));
        }

        [Test]
        public void DoEqualDefaultEntitiesWithMatchingIdsGenerateDifferentHashCodes()
        {
            Assert.That(!this._obj.GetHashCode().Equals(this._diffObj.GetHashCode()));
        }

        [Test]
        public void DoEqualDefaultEntitiesWithNoIdsGenerateSameHashCodes()
        {
            Assert.That(this._obj.GetHashCode().Equals(this._sameObj.GetHashCode()));
        }

        [Test]
        public void DoEqualEntitiesWithMatchingIdsGenerateDifferentHashCodes()
        {
            Assert.That(!this._objWithId.GetHashCode().Equals(this._diffObjWithId.GetHashCode()));
        }

        [Test]
        public void DoEqualEntitiesWithNoIdsGenerateSameHashCodes()
        {
            Assert.That(this._objWithId.GetHashCode().Equals(this._sameObjWithId.GetHashCode()));
        }

        [Test]
        public void DoesDefaultEntityEqualsOverrideWorkWhenNoIdIsAssigned()
        {
            Assert.That(this._obj.Equals(this._sameObj));
            Assert.That(!this._obj.Equals(this._diffObj));
            Assert.That(!this._obj.Equals(new MockEntityObjectWithDefaultId()));
        }

        [Test]
        public void DoesEntityEqualsOverrideWorkWhenNoIdIsAssigned()
        {
            Assert.That(this._objWithId.Equals(this._sameObjWithId));
            Assert.That(!this._objWithId.Equals(this._diffObjWithId));
            Assert.That(!this._objWithId.Equals(new MockEntityObjectWithSetId()));
        }

        [Test]
        public void Entity_with_domain_signature_preserves_hashcode_when_transitioning_from_transient_to_persistent()
        {
            var sut = new ObjectWithOneDomainSignatureProperty { Age = 1 };

            Assert.That(sut.IsTransient());

            var hashcodeWhenTransient = sut.GetHashCode();

            sut.SetIdTo(1);

            Assert.That(sut.IsTransient(), Is.False);
            Assert.That(sut.GetHashCode(), Is.EqualTo(hashcodeWhenTransient));
        }

        [Test]
        public void
            Entity_with_no_signature_properties_preserves_hashcode_when_transitioning_from_transient_to_persistent()
        {
            var sut = new ObjectWithNoDomainSignatureProperties();

            Assert.That(sut.IsTransient());

            var hashcodeWhenTransient = sut.GetHashCode();

            sut.SetIdTo(1);

            Assert.That(sut.IsTransient(), Is.False);
            Assert.That(sut.GetHashCode(), Is.EqualTo(hashcodeWhenTransient));
        }

        [Test]
        public void KeepsConsistentHashThroughLifetimeOfPersistentObject()
        {
            var object1 = new ObjectWithOneDomainSignatureProperty();
            EntityIdSetter.SetIdOf(object1, 1);
            var initialHash = object1.GetHashCode();

            object1.Age = 13;
            object1.Name = "Foo";

            Assert.AreEqual(initialHash, object1.GetHashCode());

            object1.Age = 14;
            Assert.AreEqual(initialHash, object1.GetHashCode());
        }

        [Test]
        public void KeepsConsistentHashThroughLifetimeOfTransientObject()
        {
            var object1 = new ObjectWithOneDomainSignatureProperty();
            var initialHash = object1.GetHashCode();

            object1.Age = 13;
            object1.Name = "Foo";

            Assert.AreEqual(initialHash, object1.GetHashCode());

            object1.Age = 14;
            Assert.AreEqual(initialHash, object1.GetHashCode());
        }

        [Test]
        public void EntitySerializesAsJsonProperly()
        {
            var object1 = new ObjectWithOneDomainSignatureProperty();
            object1.SetIdTo(999);
            object1.Age = 13;
            object1.Name = "Foo";

            var jsonSerializer = new JsonSerializer();

            string jsonString;
            using (var stringWriter = new StringWriter())
            {
                jsonSerializer.Serialize(stringWriter, object1);
                jsonString = stringWriter.ToString();
            }

            using (var stringReader = new StringReader(jsonString))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                var deserialized = jsonSerializer.Deserialize<ObjectWithOneDomainSignatureProperty>(jsonReader);
                Assert.IsNotNull(deserialized);
                Assert.AreEqual(999, deserialized.Id);
                Assert.AreEqual(13, deserialized.Age);
                Assert.AreEqual("Foo", deserialized.Name);
            }
        }

        [SetUp]
        public void Setup()
        {
            this._obj = new MockEntityObjectWithDefaultId
                {
                   FirstName = "FName1", LastName = "LName1", Email = "testus...@mail.com" 
                };
            this._sameObj = new MockEntityObjectWithDefaultId
                {
                   FirstName = "FName1", LastName = "LName1", Email = "testus...@mail.com" 
                };
            this._diffObj = new MockEntityObjectWithDefaultId
                {
                   FirstName = "FName2", LastName = "LName2", Email = "testuse...@mail.com" 
                };
            this._objWithId = new MockEntityObjectWithSetId
                {
                   FirstName = "FName1", LastName = "LName1", Email = "testus...@mail.com" 
                };
            this._sameObjWithId = new MockEntityObjectWithSetId
                {
                   FirstName = "FName1", LastName = "LName1", Email = "testus...@mail.com" 
                };
            this._diffObjWithId = new MockEntityObjectWithSetId
                {
                   FirstName = "FName2", LastName = "LName2", Email = "testuse...@mail.com" 
                };
        }

        [TearDown]
        public void Teardown()
        {
            this._obj = null;
            this._sameObj = null;
            this._diffObj = null;
        }

        [Test]
        public void
            Transient_entity_with_domain_signature_preserves_hashcode_temporarily_when_its_domain_signature_changes()
        {
            var sut = new ObjectWithOneDomainSignatureProperty { Age = 1 };

            var initialHashcode = sut.GetHashCode();

            sut.Age = 2;

            Assert.That(sut.GetHashCode(), Is.EqualTo(initialHashcode));
        }

        [Test]
        public void Transient_entity_with_domain_signature_should_return_consistent_hashcode()
        {
            var sut = new ObjectWithOneDomainSignatureProperty { Age = 1 };

            Assert.That(sut.GetHashCode(), Is.EqualTo(sut.GetHashCode()));
        }

        [Test]
        public void Transient_entity_without_domain_signature_should_return_consistent_hashcode()
        {
            var sut = new ObjectWithNoDomainSignatureProperties();

            Assert.That(sut.GetHashCode(), Is.EqualTo(sut.GetHashCode()));
        }

        [Test]
        public void Two_persistent_entities_with_different_domain_signature_and_equal_ids_generate_equal_hashcodes()
        {
            var sut1 = new ObjectWithOneDomainSignatureProperty { Age = 1 }.SetIdTo(1);
            var sut2 = new ObjectWithOneDomainSignatureProperty { Age = 2 }.SetIdTo(1);

            Assert.That(sut1.GetHashCode(), Is.EqualTo(sut2.GetHashCode()));
        }

        [Test]
        public void Two_persistent_entities_with_equal_domain_signature_and_different_ids_generate_different_hashcodes()
        {
            var sut1 = new ObjectWithOneDomainSignatureProperty { Age = 1 }.SetIdTo(1);
            var sut2 = new ObjectWithOneDomainSignatureProperty { Age = 1 }.SetIdTo(2);

            Assert.That(sut1.GetHashCode(), Is.Not.EqualTo(sut2.GetHashCode()));
        }

        [Test]
        public void Two_persistent_entities_with_no_signature_properties_and_different_ids_generate_different_hashcodes(
            )
        {
            var sut1 = new ObjectWithNoDomainSignatureProperties().SetIdTo(1);
            var sut2 = new ObjectWithNoDomainSignatureProperties().SetIdTo(2);

            Assert.That(sut1.GetHashCode(), Is.Not.EqualTo(sut2.GetHashCode()));
        }

        [Test]
        public void Two_persistent_entities_with_no_signature_properties_and_equal_ids_generate_equal_hashcodes()
        {
            var sut1 = new ObjectWithNoDomainSignatureProperties().SetIdTo(1);
            var sut2 = new ObjectWithNoDomainSignatureProperties().SetIdTo(1);

            Assert.That(sut1.GetHashCode(), Is.EqualTo(sut2.GetHashCode()));
        }

        [Test]
        public void Two_transient_entities_with_different_values_of_domain_signature_generate_different_hashcodes()
        {
            var sut1 = new ObjectWithOneDomainSignatureProperty { Age = 1 };
            var sut2 = new ObjectWithOneDomainSignatureProperty { Age = 2 };

            Assert.That(sut1.GetHashCode(), Is.Not.EqualTo(sut2.GetHashCode()));
        }

        [Test]
        public void Two_transient_entities_without_signature_properties_generate_different_hashcodes()
        {
            var sut1 = new ObjectWithNoDomainSignatureProperties();
            var sut2 = new ObjectWithNoDomainSignatureProperties();

            Assert.That(sut1.GetHashCode(), Is.Not.EqualTo(sut2.GetHashCode()));
        }

        [Test]
        public void Two_transient_entitites_with_equal_values_of_domain_signature_generate_equal_hashcodes()
        {
            var sut1 = new ObjectWithOneDomainSignatureProperty { Age = 1 };
            var sut2 = new ObjectWithOneDomainSignatureProperty { Age = 1 };

            Assert.That(sut1.GetHashCode(), Is.EqualTo(sut2.GetHashCode()));
        }

        [Test]
        public void WontGetConfusedWithOutsideCases()
        {
            var object1 = new ObjectWithIdenticalTypedProperties();
            var object2 = new ObjectWithIdenticalTypedProperties();

            object1.Address = "Henry";
            object1.Name = "123 Lane St.";
            object2.Address = "123 Lane St.";
            object2.Name = "Henry";
            Assert.That(object1, Is.Not.EqualTo(object2));

            object1.Address = "Henry";
            object1.Name = null;
            object2.Address = "Henri";
            object2.Name = null;
            Assert.That(object1, Is.Not.EqualTo(object2));

            object1.Address = null;
            object1.Name = "Supercalifragilisticexpialidocious";
            object2.Address = null;
            object2.Name = "Supercalifragilisticexpialidocious";
            Assert.That(object1, Is.EqualTo(object2));

            object1.Name = "Supercalifragilisticexpialidocious";
            object2.Name = "Supercalifragilisticexpialidociouz";
            Assert.That(object1, Is.Not.EqualTo(object2));
        }

        [Test]
        public void CanSerializeEntityToJson()
        {
            var object1 = new Contact() { EmailAddress = "serialize@this.net" };
            string result = JsonConvert.SerializeObject(object1);
            result.ShouldContain("serialize@this.net");
        }

        public class MockEntityObjectBase<T> : EntityWithTypedId<T>
        {
            public string Email { get; set; }

            [DomainSignature]
            public string FirstName { get; set; }

            [DomainSignature]
            public string LastName { get; set; }
        }

        public class ObjectWithOneDomainSignatureProperty : Entity
        {
            [DomainSignature]
            public int Age { get; set; }

            public string Name { get; set; }
        }

        private class AddressBeingDomainSignatureComparble : Entity
        {
            [DomainSignature]
            public string Address1 { get; set; }

            public string Address2 { get; set; }

            [DomainSignature]
            public int ZipCode { get; set; }
        }

        private class InheritedObjectWithExtraDomainSignatureProperty : ObjectWithOneDomainSignatureProperty
        {
            public string Address { get; set; }

            [DomainSignature]
            public bool IsLiving { get; set; }
        }

        private class MockEntityObjectBase : MockEntityObjectBase<int>
        {
        }

        private class MockEntityObjectWithDefaultId : MockEntityObjectBase, IHasAssignedId<int>
        {
            public void SetAssignedIdTo(int assignedId)
            {
                this.Id = assignedId;
            }
        }

        private class MockEntityObjectWithSetId : MockEntityObjectBase<string>, IHasAssignedId<string>
        {
            public void SetAssignedIdTo(string assignedId)
            {
                this.Id = assignedId;
            }
        }

        private class Object1 : Entity
        {
        }

        private class Object2 : Entity
        {
        }

        private class ObjectWithAllDomainSignatureProperty : Entity
        {
            [DomainSignature]
            public int Age { get; set; }
            [DomainSignature]
            public string Name { get; set; }
        }

        private class ObjectWithAssignedId : EntityWithTypedId<string>, IHasAssignedId<string>
        {
            [DomainSignature]
            public string Name { get; set; }

            public void SetAssignedIdTo(string assignedId)
            {
                this.Id = assignedId;
            }
        }

        private class ObjectWithComplexProperties : Entity
        {
            [DomainSignature]
            public AddressBeingDomainSignatureComparble Address { get; set; }
            [DomainSignature]
            public string Name { get; set; }

            [DomainSignature]
            public PhoneBeingNotDomainObject Phone { get; set; }
        }

        private class ObjectWithIdenticalTypedProperties : Entity
        {
            [DomainSignature]
            public string Address { get; set; }
            [DomainSignature]
            public string Name { get; set; }
        }

        private class ObjectWithIntId : Entity
        {
            [DomainSignature]
            public string Name { get; set; }
        }

        /// <summary>
        ///     This is a nonsense object; i.e., it doesn't make sense to have 
        ///     an entity without a domain signature.
        /// </summary>
        private class ObjectWithNoDomainSignatureProperties : Entity
        {
            public int Age { get; set; }

            public string Name { get; set; }
        }

        private class PhoneBeingNotDomainObject
        {
            public string Extension { get; set; }

            public string PhoneNumber { get; set; }
        }

        private class PhoneBeingNotDomainObjectButWithOverriddenEquals : PhoneBeingNotDomainObject
        {
            public override bool Equals(object obj)
            {
                var compareTo = obj as PhoneBeingNotDomainObject;

                return compareTo != null && this.PhoneNumber.Equals(compareTo.PhoneNumber);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private class Contact : Entity
        {
            public virtual string EmailAddress { get; set; }
        }

    }
}