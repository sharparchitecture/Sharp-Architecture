namespace Suteki.TardisBank.Tests.Model;

using Domain;
using Domain.Events;
using MediatR;
using Moq;
using SharpArch.Testing.Xunit;
using Shouldly;
using Xunit;


public class WithdrawlCashTests
{
    readonly Parent _parent;
    readonly Child _child;
    readonly Parent _somebodyElsesParent;
    readonly Mock<IMediator> _mediator;

    public WithdrawlCashTests()
    {
        _mediator = new Mock<IMediator>();

        _parent = new Parent("Dad", "mike@mike.com", "xxx");
        _child = _parent.CreateChild("Leo", "leohadlow", "yyy");
        _parent.MakePaymentTo(_child, 10.00M);

        _somebodyElsesParent = new Parent("Not Dad", "jon@jon.com", "zzz");
    }

    [Fact]
    [SetCulture("en-GB")]
    public void Child_should_be_able_to_withdraw_cash()
    {
        _child.WithdrawCashFromParent(_parent, 2.30M, "For Toys", _mediator.Object);

        _child.Account.Balance.ShouldBe(7.70M);
        _child.Account.Transactions[1].Amount.ShouldBe(-2.30M);
        _child.Account.Transactions[1].Description.ShouldBe("For Toys");

        _parent.Messages.Count.ShouldBe(1);
        _parent.Messages[0].Text.ShouldBe("Leo would like to withdraw £2.30");
    }

    [Fact]
    public void Child_should_not_be_able_to_withdraw_from_some_other_parent()
    {
        Action withdraw = () => _child.WithdrawCashFromParent(_somebodyElsesParent, 2.30M, "for toys", _mediator.Object);
        var exception = withdraw.ShouldThrow<CashWithdrawException>();
        exception.Message.ShouldBe("Not Your Parent");
    }

    [Fact]
    [SetCulture("en-GB")]
    public void Child_should_not_be_able_to_withdraw_more_than_their_balance()
    {
        Action withdraw = () => _child.WithdrawCashFromParent(_parent, 12.11M, "For Toys", _mediator.Object);
        var exception = withdraw.ShouldThrow<CashWithdrawException>();
        exception.Message.ShouldBe("You can not withdraw £12.11 because you only have £10.00 in your account");
    }

    [Fact]
    [SetCulture("en-GB")]
    public void Should_raise_a_SendMessageEvent()
    {
        _child.WithdrawCashFromParent(_parent, 2.30M, "For Toys", _mediator.Object);

        _mediator.Verify(m => m.Publish(
            It.Is((SendMessageEvent ev) =>
                ev.User == _parent && ev.Message == "Leo would like to withdraw £2.30"),
            default(CancellationToken)
        ), Times.Once());
    }
}
