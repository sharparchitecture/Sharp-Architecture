namespace Suteki.TardisBank.Tests.Model;

using Domain;
using Domain.Events;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;


public class UserActivationTests
{
    readonly Mock<IMediator> _mediator;

    public UserActivationTests()
    {
        _mediator = new Mock<IMediator>();
    }

    [Fact]
    public void Child_should_be_active_when_created()
    {
        User child = new Parent("Dad", "Mike@mike.com", "xxx").CreateChild("Leo", "leoahdlow", "bbb");
        child.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Parent_should_raise_an_event_when_created()
    {
        var parent = new Parent("Dad", "mike@mike.com", "xxx");
        parent.Initialise(_mediator.Object);

        _mediator.Verify(m => m.Publish(It.Is((INotification ev) =>
                ev is NewParentCreatedEvent && ((NewParentCreatedEvent)ev).Parent == parent),
            default(CancellationToken)), Times.Once());
    }

    [Fact]
    public void ParentShouldNotBeActiveWhenCreated()
    {
        User parent = new Parent("Dad", "mike@mike.com", "xxx");
        parent.IsActive.Should().BeFalse();
    }
}