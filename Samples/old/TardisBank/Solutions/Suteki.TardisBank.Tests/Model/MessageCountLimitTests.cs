namespace Suteki.TardisBank.Tests.Model
{
    using System.Linq;
    using Domain;
    using FluentAssertions;
    using MediatR;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MessageCountLimitTests
    {
        [SetUp]
        public void SetUp()
        {
            mediator = new Mock<IMediator>();
            user = new Parent("Mike", "mike@mike.com", "xxx");
        }

        [TearDown]
        public void TearDown()
        {
            //DomainEvents.Reset();
        }

        User user;
        Mock<IMediator> mediator;


        [Test]
        public void Number_of_messages_should_be_limited()
        {
            for (var i = 0; i < User.MaxMessages; i++)
            {
                user.SendMessage("Message" + i, mediator.Object);
            }

            user.Messages.Count.Should().Be(User.MaxMessages);

            user.SendMessage("New", mediator.Object);
            user.Messages.Count.Should().Be(User.MaxMessages);
            user.Messages.First().Text.Should().Be("Message1");
            user.Messages.Last().Text.Should().Be("New");

            user.SendMessage("New2", mediator.Object);
            user.Messages.Count.Should().Be(User.MaxMessages);
            user.Messages.First().Text.Should().Be("Message2");
            user.Messages.Last().Text.Should().Be("New2");
        }
    }
}
