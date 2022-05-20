namespace Suteki.TardisBank.Tests.Model;

using Domain;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;


public class MessageCountLimitTests
{
    readonly User _user;
    readonly Mock<IMediator> _mediator;

    public MessageCountLimitTests()
    {
        _mediator = new Mock<IMediator>();
        _user = new Parent("Mike", "mike@mike.com", "xxx");
    }

    [Fact]
    public void Number_of_messages_should_be_limited()
    {
        for (var i = 0; i < User.MaxMessages; i++)
        {
            _user.SendMessage("Message" + i, _mediator.Object);
        }

        _user.Messages.Count.Should().Be(User.MaxMessages);

        _user.SendMessage("New", _mediator.Object);
        _user.Messages.Count.Should().Be(User.MaxMessages);
        _user.Messages.First().Text.Should().Be("Message1");
        _user.Messages.Last().Text.Should().Be("New");

        _user.SendMessage("New2", _mediator.Object);
        _user.Messages.Count.Should().Be(User.MaxMessages);
        _user.Messages.First().Text.Should().Be("Message2");
        _user.Messages.Last().Text.Should().Be("New2");
    }
}