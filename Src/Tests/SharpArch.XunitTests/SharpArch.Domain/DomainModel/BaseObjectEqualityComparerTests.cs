namespace Tests.SharpArch.Domain.DomainModel
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using global::SharpArch.Domain.DomainModel;
    using global::SharpArch.Testing.Helpers;
    using Xunit;


    public class BaseObjectEqualityComparerTests
    {
        class ConcreteBaseObject : BaseObject
        {
            public string Name { get; set; }

            protected override PropertyInfo[] GetTypeSpecificSignatureProperties()
            {
                return GetType().GetProperties();
            }
        }


        class ConcreteEntityWithDomainSignatureProperties : Entity<int>
        {
            [DomainSignature]
            public string Name { get; set; }
        }


        class ConcreteEntityWithNoDomainSignatureProperties : Entity<int>
        {
            public string Name { get; set; }
        }


        class ConcreteValueObject : ValueObject
        {
            public string Name { get; set; }
        }


        [Fact]
        public void CanBeUsedByLinqSetOperatorsSuchAsIntersect()
        {
            IList<ConcreteEntityWithDomainSignatureProperties> objects1 =
                new List<ConcreteEntityWithDomainSignatureProperties>();
            var obj1 = new ConcreteEntityWithDomainSignatureProperties {Name = @"Billy McCafferty"};
            EntityIdSetter.SetIdOf(obj1, 2);
            objects1.Add(obj1);

            IList<ConcreteEntityWithDomainSignatureProperties> objects2 =
                new List<ConcreteEntityWithDomainSignatureProperties>();
            var obj2 = new ConcreteEntityWithDomainSignatureProperties {Name = @"Jimi Hendrix"};
            EntityIdSetter.SetIdOf(obj2, 1);
            objects2.Add(obj2);
            var obj3 = new ConcreteEntityWithDomainSignatureProperties
            {
                Name =
                    "Doesn't Matter since the Id will match and the presence of the domain signature will go overridden"
            };
            EntityIdSetter.SetIdOf(obj3, 2);
            objects2.Add(obj3);

            objects1.Intersect(
                    objects2, new BaseObjectEqualityComparer<ConcreteEntityWithDomainSignatureProperties>())
                .Should().HaveCount(1);

            objects1.Intersect(
                    objects2, new BaseObjectEqualityComparer<ConcreteEntityWithDomainSignatureProperties>()).First()
                .Equals(obj1).Should().BeTrue();

            objects1.Intersect(
                    objects2, new BaseObjectEqualityComparer<ConcreteEntityWithDomainSignatureProperties>()).First()
                .Equals(obj3).Should().BeTrue();
        }

        [Fact]
        public void CanCompareBaseObjects()
        {
            var comparer = new BaseObjectEqualityComparer<BaseObject>();

            var obj1 = new ConcreteBaseObject {Name = "Whatever"};
            var obj2 = new ConcreteBaseObject {Name = "Whatever"};
            comparer.Equals(obj1, obj2).Should().BeTrue();

            obj2.Name = "Mismatch";
            comparer.Equals(obj1, obj2).Should().BeFalse();
        }

        [Fact]
        public void CanCompareEntitiesWithDomainSignatureProperties()
        {
            var comparer = new BaseObjectEqualityComparer<Entity<int>>();

            var obj1 = new ConcreteEntityWithDomainSignatureProperties {Name = "Whatever"};
            var obj2 = new ConcreteEntityWithDomainSignatureProperties {Name = "Whatever"};

            comparer.Equals(obj1, obj2).Should().BeTrue();

            obj2.Name = "Mismatch";
            comparer.Equals(obj1, obj2).Should().BeFalse();

            EntityIdSetter.SetIdOf(obj1, 1);
            EntityIdSetter.SetIdOf(obj2, 1);
            comparer.Equals(obj1, obj2).Should().BeTrue();
        }

        [Fact]
        public void CanCompareEntitiesWithNoDomainSignatureProperties()
        {
            var comparer = new BaseObjectEqualityComparer<BaseObject>();

            var obj1 = new ConcreteEntityWithNoDomainSignatureProperties {Name = "Whatever"};
            var obj2 = new ConcreteEntityWithNoDomainSignatureProperties {Name = @"asdf"};
            comparer.Equals(obj1, obj2).Should().BeFalse();

            EntityIdSetter.SetIdOf(obj1, 1);
            EntityIdSetter.SetIdOf(obj2, 1);
            comparer.Equals(obj1, obj2).Should().BeTrue();
        }

        [Fact]
        public void CanCompareNulls()
        {
            var comparer = new BaseObjectEqualityComparer<BaseObject>();
            comparer.Equals(null, null).Should().BeTrue();
            comparer.Equals(null, new ConcreteBaseObject()).Should().BeFalse();
            comparer.Equals(new ConcreteBaseObject(), null).Should().BeFalse();
        }

        [Fact]
        public void CanCompareValueObjects()
        {
            var comparer = new BaseObjectEqualityComparer<BaseObject>();

            var obj1 = new ConcreteValueObject {Name = "Whatever"};
            var obj2 = new ConcreteValueObject {Name = "Whatever"};
            comparer.Equals(obj1, obj2).Should().BeTrue();

            obj2.Name = "Mismatch";
            comparer.Equals(obj1, obj2).Should().BeFalse();
        }

        [Fact]
        public void CannotSuccessfullyCompareDifferentlyTypedObjectsThatDeriveFromBaseObject()
        {
            var comparer = new BaseObjectEqualityComparer<BaseObject>();

            var obj1 = new ConcreteBaseObject {Name = "Whatever"};
            var obj2 = new ConcreteValueObject {Name = "Whatever"};

            comparer.Equals(obj1, obj2).Should().BeFalse();
        }
    }
}
