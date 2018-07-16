// ReSharper disable InconsistentNaming

namespace Suteki.TardisBank.Tests.Model
{
    using Domain;
    using FluentAssertions;
    using NUnit.Framework;
    using SharpArch.NHibernate;
    using SharpArch.Testing.NUnit;
    using SharpArch.Testing.NUnit.NHibernate;

    [TestFixture]
    public class ParentTests : RepositoryTestsBase
    {
        int parentId;

        protected override void LoadTestData()
        {
            var parent = new Parent("Mike Hadlow", string.Format("{0}@yahoo.com", "mike"), "yyy");
            Session.Save(parent);
            this.FlushSessionAndEvict(parent);
            parentId = parent.Id;
        }

        [Test]
        public void Should_be_able_to_add_a_child_to_a_parent()
        {
            var linqRepository = new LinqRepository<Parent>(TransactionManager, Session);
            Parent savedParent = linqRepository.Get(parentId);
            savedParent.CreateChild("jim", "jim123", "passw0rd1");
            savedParent.CreateChild("jenny", "jenny123", "passw0rd2");
            savedParent.CreateChild("jez", "jez123", "passw0rd3");
            this.FlushSessionAndEvict(savedParent);


            Parent parent = linqRepository.Get(parentId);
            parent.Children.Count.Should().Be(3);

            parent.Children[0].Name.Should().Be("jim");
            parent.Children[1].Name.Should().Be("jenny");
            parent.Children[2].Name.Should().Be("jez");
        }

        [Test]
        public void Should_be_able_to_create_and_retrieve_Parent()
        {
            Parent parent = new LinqRepository<Parent>(TransactionManager, Session).Get(parentId);
            parent.Should().NotBeNull();
            parent.Name.Should().Be("Mike Hadlow");
            parent.UserName.Should().Be("mike@yahoo.com");
            parent.Children.Should().NotBeNull();
        }
    }
}

// ReSharper restore InconsistentNaming
