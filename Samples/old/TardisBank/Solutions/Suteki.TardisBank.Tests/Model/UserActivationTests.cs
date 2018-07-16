namespace Suteki.TardisBank.Tests.Model
{
    using Domain;
    using FluentAssertions;
    using MediatR;
    using Moq;
    using NUnit.Framework;
    using SharpArch.Testing.NUnit;

    [TestFixture]
    public class UserActivationTests
    {
        [SetUp]
        public void SetUp()
        {
            mediator = new Mock<IMediator>();
        }

        Mock<IMediator> mediator;

        [Test]
        public void Child_should_be_active_when_created()
        {
            User child = new Parent("Dad", "Mike@mike.com", "xxx").CreateChild("Leo", "leoahdlow", "bbb");
            child.IsActive.Should().BeTrue();
        }

        [Test]
        public void Parent_should_raise_an_event_when_created()
        {
            var parent = new Parent("Dad", "mike@mike.com", "xxx");
            parent.Initialise(mediator.Object);

            mediator.Verify(m => m.Publish(It.Is((INotification ev) =>
                ev is Domain.Events.NewParentCreatedEvent && ((Domain.Events.NewParentCreatedEvent) ev).Parent == parent)
                ));
        }

        [Test]
        public void ParentShouldNotBeActiveWhenCreated()
        {
            User parent = new Parent("Dad", "mike@mike.com", "xxx");
            parent.IsActive.Should().BeFalse();
        }
    }
}
