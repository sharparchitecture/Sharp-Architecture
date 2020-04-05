namespace Tests.SharpArch.Domain.DomainModel
{
    using System;
    using System.IO;
    using FluentAssertions;
    using global::SharpArch.Domain.DomainModel;
    using global::SharpArch.Testing.Helpers;
    using Newtonsoft.Json;
    using Xunit;


    public class EntityTests
    {
        readonly MockEntityObjectWithDefaultId _diffObj;

        readonly MockEntityObjectWithSetId _diffObjWithId;

        readonly MockEntityObjectWithDefaultId _obj;

        readonly MockEntityObjectWithSetId _objWithId;

        readonly MockEntityObjectWithDefaultId _sameObj;

        readonly MockEntityObjectWithSetId _sameObjWithId;

        public EntityTests()
        {
            _obj = new MockEntityObjectWithDefaultId
            {
                FirstName = "FName1",
                LastName = "LName1",
                Email = @"testus...@mail.com"
            };
            _sameObj = new MockEntityObjectWithDefaultId
            {
                FirstName = "FName1",
                LastName = "LName1",
                Email = @"testus...@mail.com"
            };
            _diffObj = new MockEntityObjectWithDefaultId
            {
                FirstName = "FName2",
                LastName = "LName2",
                Email = @"testuse...@mail.com"
            };
            _objWithId = new MockEntityObjectWithSetId
            {
                FirstName = "FName1",
                LastName = "LName1",
                Email = @"testus...@mail.com"
            };
            _sameObjWithId = new MockEntityObjectWithSetId
            {
                FirstName = "FName1",
                LastName = "LName1",
                Email = @"testus...@mail.com"
            };
            _diffObjWithId = new MockEntityObjectWithSetId
            {
                FirstName = "FName2",
                LastName = "LName2",
                Email = @"testuse...@mail.com"
            };
        }


        public abstract class MockEntityObjectBase<T> : EntityWithTypedId<T>
            where T : IEquatable<T>
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


        class AddressBeingDomainSignatureComparable : Entity
        {
            [DomainSignature]
            public string Address1 { get; set; }

            public string Address2 { get; set; }

            [DomainSignature]
            public int ZipCode { get; set; }
        }


        class InheritedObjectWithExtraDomainSignatureProperty : ObjectWithOneDomainSignatureProperty
        {
            public string Address { get; set; }

            [DomainSignature]
            public bool IsLiving { get; set; }
        }


        class MockEntityObjectBase : MockEntityObjectBase<int>
        {
        }


        class MockEntityObjectWithDefaultId : MockEntityObjectBase, IHasAssignedId<int>
        {
            public void SetAssignedIdTo(int assignedId)
            {
                Id = assignedId;
            }
        }


        class MockEntityObjectWithSetId : MockEntityObjectBase<string>, IHasAssignedId<string>
        {
            public void SetAssignedIdTo(string assignedId)
            {
                Id = assignedId;
            }
        }


        class Entity1 : Entity
        {
        }


        class Entity2 : Entity
        {
        }


        class ObjectWithAllDomainSignatureProperty : Entity
        {
            [DomainSignature]
            public int Age { get; set; }

            [DomainSignature]
            public string Name { get; set; }
        }


        class ObjectWithAssignedId : EntityWithTypedId<string>, IHasAssignedId<string>
        {
            [DomainSignature]
            public string Name { get; set; }

            public void SetAssignedIdTo(string assignedId)
            {
                Id = assignedId;
            }
        }


        class ObjectWithComplexProperties : Entity
        {
            [DomainSignature]
            public AddressBeingDomainSignatureComparable Address { get; set; }

            [DomainSignature]
            public string Name { get; set; }

            [DomainSignature]
            public PhoneBeingNotDomainObject Phone { get; set; }
        }


        class ObjectWithIdenticalTypedProperties : Entity
        {
            [DomainSignature]
            public string Address { get; set; }

            [DomainSignature]
            public string Name { get; set; }
        }


        class ObjectWithIntId : Entity
        {
            [DomainSignature]
            public string Name { get; set; }
        }


        /// <summary>
        ///     This is a nonsense object; i.e., it doesn't make sense to have
        ///     an entity without a domain signature.
        /// </summary>
        class ObjectWithNoDomainSignatureProperties : Entity
        {
            public int Age { get; set; }

            public string Name { get; set; }
        }


        class PhoneBeingNotDomainObject
        {
            public string Extension { get; set; }

            public string PhoneNumber { get; set; }
        }


        class PhoneBeingNotDomainObjectButWithOverriddenEquals : PhoneBeingNotDomainObject
        {
            public override bool Equals(object obj)
            {
                return obj is PhoneBeingNotDomainObject compareTo && PhoneNumber.Equals(compareTo.PhoneNumber);
            }

            public override int GetHashCode()
            {
                // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
                return base.GetHashCode();
            }
        }


        class Contact : Entity
        {
            public virtual string EmailAddress { get; set; }
        }


        [Fact]
        public void CanCompareDomainObjectsWithAllPropertiesBeingPartOfDomainSignature()
        {
            var obj1 = new ObjectWithAllDomainSignatureProperty();
            var obj2 = new ObjectWithAllDomainSignatureProperty();
            obj1.Equals(obj2).Should().BeTrue();

            obj1.Age = 13;
            obj2.Age = 13;
            obj1.Name = "Foo";
            obj2.Name = "Foo";
            obj1.Equals(obj2).Should().BeTrue();

            obj1.Name = "Bar";
            obj1.Equals(obj2).Should().BeFalse();

            obj1.Name = null;
            obj1.Equals(obj2).Should().BeFalse();

            obj2.Name = null;
            obj1.Equals(obj2).Should().BeTrue();
        }

        [Fact]
        public void CanCompareDomainObjectsWithOnlySomePropertiesBeingPartOfDomainSignature()
        {
            var obj1 = new ObjectWithOneDomainSignatureProperty();
            var obj2 = new ObjectWithOneDomainSignatureProperty();
            obj1.Equals(obj2).Should().BeTrue();

            obj1.Age = 13;
            obj2.Age = 13;

            // Name property isn't included in comparison
            obj1.Name = "Foo";
            obj2.Name = "Bar";
            obj1.Equals(obj2).Should().BeTrue();

            obj1.Age = 14;
            obj1.Equals(obj2).Should().BeFalse();
            obj1.Equals(obj2).Should().BeFalse();
        }

        [Fact]
        public void CanCompareEntities()
        {
            var obj1 = new ObjectWithIntId {Name = "Acme"};
            var obj2 = new ObjectWithIntId {Name = "Anvil"};

            obj1.Equals(null).Should().BeFalse();
            obj1.Equals(obj2).Should().BeFalse();

            EntityIdSetter.SetIdOf(obj1, 10);
            EntityIdSetter.SetIdOf(obj2, 10);

            // Even though the "business value signatures" are different, the persistent Ids 
            // were the same.  Call me crazy, but I put that much trust into persisted Ids.
            obj1.Equals(obj2).Should().BeTrue();
            obj1.GetHashCode().Should().Be(obj2.GetHashCode());

            var obj3 = new ObjectWithIntId {Name = "Acme"};

            // Since obj1 has an Id but obj3 doesn't, they won't be equal
            // even though their signatures are the same
            obj1.Equals(obj3).Should().BeFalse();

            var obj4 = new ObjectWithIntId {Name = "Acme"};

            // obj3 and obj4 are both transient and share the same signature
            obj3.Equals(obj4).Should().BeTrue();
        }

        [Fact]
        public void CanCompareEntitiesWithAssignedIds()
        {
            var obj1 = new ObjectWithAssignedId {Name = "Acme"};
            var obj2 = new ObjectWithAssignedId {Name = "Anvil"};

            obj1.Equals(null).Should().BeFalse();
            obj1.Equals(obj2).Should().BeFalse();

            obj1.SetAssignedIdTo("AAAAA");
            obj2.SetAssignedIdTo("AAAAA");

            // Even though the "business value signatures" are different, the persistent Ids 
            // were the same.  Call me crazy, but I put that much trust into persisted Ids.
            obj1.Equals(obj2).Should().BeTrue();

            var obj3 = new ObjectWithAssignedId {Name = "Acme"};

            // Since obj1 has an Id but obj3 doesn't, they won't be equal
            // even though their signatures are the same
            obj1.Equals(obj3).Should().BeFalse();

            var obj4 = new ObjectWithAssignedId {Name = "Acme"};

            // obj3 and obj4 are both transient and share the same signature
            obj3.Equals(obj4).Should().BeTrue();
        }

        [Fact]
        public void CanCompareInheritedDomainObjects()
        {
            var obj1 = new InheritedObjectWithExtraDomainSignatureProperty();
            var obj2 = new InheritedObjectWithExtraDomainSignatureProperty();
            obj1.Equals(obj2).Should().BeTrue();

            obj1.Age = 13;
            obj1.IsLiving = true;
            obj2.Age = 13;
            obj2.IsLiving = true;

            // Address property isn't included in comparison
            obj1.Address = "123 Oak Ln.";
            obj2.Address = "Nightmare on Elm St.";
            obj1.Equals(obj2).Should().BeTrue();

            obj1.IsLiving = false;
            obj1.Equals(obj2).Should().BeFalse();
        }

        [Fact]
        public void CanCompareObjectsWithComplexProperties()
        {
            var obj1 = new ObjectWithComplexProperties();
            var obj2 = new ObjectWithComplexProperties();

            obj1.Equals(obj2).Should().BeTrue();

            obj1.Address = new AddressBeingDomainSignatureComparable
            {
                Address1 = "123 Smith Ln.",
                Address2 = "Suite 201",
                ZipCode = 12345
            };
            obj1.Equals(obj2).Should().BeFalse();

            // Set the address of the 2nd to be different to the address of the first
            obj2.Address = new AddressBeingDomainSignatureComparable
            {
                Address1 = "123 Smith Ln.",

                // Address2 isn't marked as being part of the domain signature; 
                // therefore, it WON'T be used in the equality comparison
                Address2 = "Suite 402",
                ZipCode = 98765
            };
            obj1.Equals(obj2).Should().BeFalse();

            // Set the address of the 2nd to be the same as the first
            obj2.Address.ZipCode = 12345;
            obj1.Equals(obj2).Should().BeTrue();

            obj1.Phone = new PhoneBeingNotDomainObject {PhoneNumber = "(555) 555-5555"};
            obj1.Equals(obj2).Should().BeFalse();

            // IMPORTANT: Note that even though the phone number below has the same value as the 
            // phone number on obj1, they're not domain signature comparable; therefore, the
            // "out of the box" equality will be used which shows them as being different objects.
            obj2.Phone = new PhoneBeingNotDomainObject {PhoneNumber = "(555) 555-5555"};
            obj1.Equals(obj2).Should().BeFalse();

            // Observe as we replace the obj1.Phone with an object that isn't domain-signature
            // comparable, but DOES have an overridden Equals which will return true if the phone
            // number properties are equal.
            obj1.Phone = new PhoneBeingNotDomainObjectButWithOverriddenEquals {PhoneNumber = "(555) 555-5555"};
            obj1.Equals(obj2).Should().BeTrue();
        }

        [Fact]
        public void CanHaveEntityWithoutDomainSignatureProperties()
        {
            var invalidEntity = new ObjectWithNoDomainSignatureProperties();

            invalidEntity.GetSignatureProperties().Should().BeEmpty();
        }

        [Fact]
        public void CannotEquateObjectsWithSameIdButDifferentTypes()
        {
            var obj1 = new Entity1();
            var obj2 = new Entity2();

            EntityIdSetter.SetIdOf(obj1, 1);
            EntityIdSetter.SetIdOf(obj2, 1);

            // ReSharper disable once SuspiciousTypeConversion.Global
            obj1.Equals(obj2).Should().BeFalse();
        }

        [Fact]
        public void CanSerializeEntityToJson()
        {
            var obj1 = new Contact {EmailAddress = "serialize@this.net"};
            string result = JsonConvert.SerializeObject(obj1);
            result.Should().Contain("serialize@this.net");
        }

        [Fact]
        public void DoDefaultEntityEqualsOverrideWorkWhenIdIsAssigned()
        {
            _obj.SetAssignedIdTo(1);
            _diffObj.SetAssignedIdTo(1);
            _obj.Equals(_diffObj).Should().BeTrue();
        }

        [Fact]
        public void DoEntityEqualsOverrideWorkWhenIdIsAssigned()
        {
            _objWithId.SetAssignedIdTo("1");
            _diffObjWithId.SetAssignedIdTo("1");
            _objWithId.Equals(_diffObjWithId).Should().BeTrue();
        }

        [Fact]
        public void DoEqualDefaultEntitiesWithMatchingIdsGenerateDifferentHashCodes()
        {
            _obj.GetHashCode().Should().NotBe(_diffObj.GetHashCode());
        }

        [Fact]
        public void DoEqualDefaultEntitiesWithNoIdsGenerateSameHashCodes()
        {
            _obj.GetHashCode().Should().Be(_sameObj.GetHashCode());
        }

        [Fact]
        public void DoEqualEntitiesWithMatchingIdsGenerateDifferentHashCodes()
        {
            _objWithId.GetHashCode().Should().NotBe(_diffObjWithId.GetHashCode());
        }

        [Fact]
        public void DoEqualEntitiesWithNoIdsGenerateSameHashCodes()
        {
            _objWithId.GetHashCode().Should().Be(_sameObjWithId.GetHashCode());
        }

        [Fact]
        public void DoesDefaultEntityEqualsOverrideWorkWhenNoIdIsAssigned()
        {
            _obj.Equals(_sameObj).Should().BeTrue();
            _obj.Equals(_diffObj).Should().BeFalse();
            _obj.Equals(new MockEntityObjectWithDefaultId()).Should().BeFalse();
        }

        [Fact]
        public void DoesEntityEqualsOverrideWorkWhenNoIdIsAssigned()
        {
            _objWithId.Equals(_sameObjWithId).Should().BeTrue();
            _objWithId.Equals(_diffObjWithId).Should().BeFalse();
            _objWithId.Equals(new MockEntityObjectWithSetId()).Should().BeFalse();
        }

        [Fact]
        public void Entity_with_domain_signature_preserves_hashcode_when_transitioning_from_transient_to_persistent()
        {
            var obj = new ObjectWithOneDomainSignatureProperty {Age = 1};
            obj.IsTransient().Should().BeTrue();

            int hashcodeWhenTransient = obj.GetHashCode();
            obj.SetIdTo(1);

            obj.IsTransient().Should().BeFalse();
            obj.GetHashCode().Should().Be(hashcodeWhenTransient);
        }

        [Fact]
        public void
            Entity_with_no_signature_properties_preserves_hashcode_when_transitioning_from_transient_to_persistent()
        {
            var obj = new ObjectWithNoDomainSignatureProperties();
            obj.IsTransient().Should().BeTrue();

            int hashcodeWhenTransient = obj.GetHashCode();
            obj.SetIdTo(1);

            obj.IsTransient().Should().BeFalse();
            obj.GetHashCode().Should().Be(hashcodeWhenTransient);
        }

        [Fact]
        public void EntitySerializesAsJsonProperly()
        {
            var obj = new ObjectWithOneDomainSignatureProperty();
            obj.SetIdTo(999);
            obj.Age = 13;
            obj.Name = "Foo";

            var jsonSerializer = new JsonSerializer();

            string jsonString;
            using (var stringWriter = new StringWriter())
            {
                jsonSerializer.Serialize(stringWriter, obj);
                jsonString = stringWriter.ToString();
            }

            using (var stringReader = new StringReader(jsonString))
            {
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    var deserialized = jsonSerializer.Deserialize<ObjectWithOneDomainSignatureProperty>(jsonReader);
                    deserialized.Should().BeEquivalentTo(obj);
                }
            }
        }

        [Fact]
        public void KeepsConsistentHashThroughLifetimeOfPersistentObject()
        {
            var obj = new ObjectWithOneDomainSignatureProperty();
            EntityIdSetter.SetIdOf(obj, 1);
            int initialHash = obj.GetHashCode();

            obj.Age = 13;
            obj.Name = "Foo";
            obj.GetHashCode().Should().Be(initialHash);

            obj.Age = 14;
            obj.GetHashCode().Should().Be(initialHash);
        }

        [Fact]
        public void KeepsConsistentHashThroughLifetimeOfTransientObject()
        {
            var obj = new ObjectWithOneDomainSignatureProperty();
            int initialHash = obj.GetHashCode();

            obj.Age = 13;
            obj.Name = "Foo";

            obj.GetHashCode().Should().Be(initialHash);

            obj.Age = 14;
            obj.GetHashCode().Should().Be(initialHash);
        }

        [Fact]
        public void
            Transient_entity_with_domain_signature_preserves_hashcode_temporarily_when_its_domain_signature_changes()
        {
            var obj = new ObjectWithOneDomainSignatureProperty {Age = 1};
            int initialHash = obj.GetHashCode();

            obj.Age = 2;
            obj.GetHashCode().Should().Be(initialHash);
        }

        [Fact]
        public void Transient_entity_with_domain_signature_should_return_consistent_hashcode()
        {
            var obj = new ObjectWithOneDomainSignatureProperty {Age = 1};
            obj.GetHashCode().Should().Be(obj.GetHashCode());
        }

        [Fact]
        public void Transient_entity_without_domain_signature_should_return_consistent_hashcode()
        {
            var obj = new ObjectWithNoDomainSignatureProperties();
            obj.GetHashCode().Should().Be(obj.GetHashCode());
        }

        [Fact]
        public void Two_persistent_entities_with_different_domain_signature_and_equal_ids_generate_equal_hashcodes()
        {
            IEntityWithTypedId<int> obj1 = new ObjectWithOneDomainSignatureProperty {Age = 1}.SetIdTo(1);
            IEntityWithTypedId<int> obj2 = new ObjectWithOneDomainSignatureProperty {Age = 2}.SetIdTo(1);

            obj1.GetHashCode().Should().Be(obj2.GetHashCode());
        }

        [Fact]
        public void Two_persistent_entities_with_equal_domain_signature_and_different_ids_generate_different_hashcodes()
        {
            IEntityWithTypedId<int> obj1 = new ObjectWithOneDomainSignatureProperty {Age = 1}.SetIdTo(1);
            IEntityWithTypedId<int> obj2 = new ObjectWithOneDomainSignatureProperty {Age = 1}.SetIdTo(2);

            obj1.GetHashCode().Should().NotBe(obj2.GetHashCode());
        }

        [Fact]
        public void Two_persistent_entities_with_no_signature_properties_and_different_ids_generate_different_hashcodes(
        )
        {
            IEntityWithTypedId<int> obj1 = new ObjectWithNoDomainSignatureProperties().SetIdTo(1);
            IEntityWithTypedId<int> obj2 = new ObjectWithNoDomainSignatureProperties().SetIdTo(2);

            obj1.GetHashCode().Should().NotBe(obj2.GetHashCode());
        }

        [Fact]
        public void Two_persistent_entities_with_no_signature_properties_and_equal_ids_generate_equal_hashcodes()
        {
            IEntityWithTypedId<int> obj1 = new ObjectWithNoDomainSignatureProperties().SetIdTo(1);
            IEntityWithTypedId<int> obj2 = new ObjectWithNoDomainSignatureProperties().SetIdTo(1);

            obj1.GetHashCode().Should().Be(obj2.GetHashCode());
        }

        [Fact]
        public void Two_transient_entities_with_different_values_of_domain_signature_generate_different_hashcodes()
        {
            var obj1 = new ObjectWithOneDomainSignatureProperty {Age = 1};
            var obj2 = new ObjectWithOneDomainSignatureProperty {Age = 2};
            obj1.GetHashCode().Should().NotBe(obj2.GetHashCode());
        }

        [Fact]
        public void Two_transient_entities_with_equal_values_of_domain_signature_generate_equal_hashcodes()
        {
            var obj1 = new ObjectWithOneDomainSignatureProperty {Age = 1};
            var obj2 = new ObjectWithOneDomainSignatureProperty {Age = 1};
            obj1.GetHashCode().Should().Be(obj2.GetHashCode());
        }

        [Fact]
        public void Two_transient_entities_without_signature_properties_generate_different_hashcodes()
        {
            var obj1 = new ObjectWithNoDomainSignatureProperties();
            var obj2 = new ObjectWithNoDomainSignatureProperties();
            obj1.GetHashCode().Should().NotBe(obj2.GetHashCode());
        }

        [Fact]
        public void WontGetConfusedWithOutsideCases()
        {
            var obj1 = new ObjectWithIdenticalTypedProperties();
            var obj2 = new ObjectWithIdenticalTypedProperties();

            obj1.Address = "Henry";
            obj1.Name = "123 Lane St.";
            obj2.Address = "123 Lane St.";
            obj2.Name = "Henry";
            obj1.Equals(obj2).Should().BeFalse();

            obj1.Address = "Henry";
            obj1.Name = null;
            obj2.Address = "Henri";
            obj2.Name = null;
            obj1.Equals(obj2).Should().BeFalse();

            obj1.Address = null;
            obj1.Name = @"Supercalifragilisticexpialidocious";
            obj2.Address = null;
            obj2.Name = @"Supercalifragilisticexpialidocious";
            obj1.Equals(obj2).Should().BeTrue();

            obj1.Name = @"Supercalifragilisticexpialidocious";
            obj2.Name = @"Supercalifragilisticexpialidociouz";
            obj1.Equals(obj2).Should().BeFalse();
        }
    }
}
