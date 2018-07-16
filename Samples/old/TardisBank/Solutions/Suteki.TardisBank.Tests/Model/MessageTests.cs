// ReSharper disable InconsistentNaming

namespace Suteki.TardisBank.Tests.Model
{
    using Domain;
    using FluentAssertions;
    using MediatR;
    using Moq;
    using NUnit.Framework;
    using SharpArch.NHibernate;
    using SharpArch.Testing.NUnit.NHibernate;

    [TestFixture]
    public class MessageTests : RepositoryTestsBase
    {
        protected override void SetUp()
        {
            this.mediator = new Mock<IMediator>();
            base.SetUp();
        }

        int userId;
        Mock<IMediator> mediator;

        protected override void LoadTestData()
        {
            User user = new Parent("Dad", "mike@mike.com", "xxx");
            Session.Save(user);
            this.FlushSessionAndEvict(user);
            userId = user.Id;
        }

        [Test]
        public void Should_be_able_to_add_a_message_to_a_user()
        {
            var parentRepository = new LinqRepository<Parent>(TransactionManager, Session);
            User userToTestWith = parentRepository.Get(userId);

            userToTestWith.SendMessage("some message", this.mediator.Object);

            FlushSessionAndEvict(userToTestWith);


            Parent parent = parentRepository.Get(userId);
            parent.Messages.Count.Should().Be(1);
        }
    }
}

// ReSharper restore InconsistentNaming
