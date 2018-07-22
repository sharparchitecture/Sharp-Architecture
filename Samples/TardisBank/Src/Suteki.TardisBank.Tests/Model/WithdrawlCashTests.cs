namespace Suteki.TardisBank.Tests.Model
{
    using System;
    using System.Threading;
    using Domain;
    using FluentAssertions;
    using MediatR;
    using Moq;
    using SharpArch.Testing.Xunit;
    using Xunit;


    public class WithdrawlCashTests
    {
        public WithdrawlCashTests()
        {
            _mediator = new Mock<IMediator>();

            _parent = new Parent("Dad", "mike@mike.com", "xxx");
            _child = _parent.CreateChild("Leo", "leohadlow", "yyy");
            _parent.MakePaymentTo(_child, 10.00M);

            _somebodyElsesParent = new Parent("Not Dad", "jon@jon.com", "zzz");
        }

        readonly Parent _parent;
        readonly Child _child;
        readonly Parent _somebodyElsesParent;
        readonly Mock<IMediator> _mediator;

        [Fact]
        public void Child_shold_not_be_able_to_withdraw_from_some_other_parent()
        {
            Action withdraw = () => _child.WithdrawCashFromParent(_somebodyElsesParent, 2.30M, "for toys", _mediator.Object);
            withdraw.Should().Throw<CashWithdrawException>().WithMessage("Not Your Parent");
        }


        [Fact]
        [SetCulture("en-GB")]
        public void Child_should_be_able_to_withdraw_cash()
        {


            _child.WithdrawCashFromParent(_parent, 2.30M, "For Toys", _mediator.Object);

            _child.Account.Balance.Should().Be(7.70M);
            _child.Account.Transactions[1].Amount.Should().Be(-2.30M);
            _child.Account.Transactions[1].Description.Should().Be("For Toys");

            _parent.Messages.Count.Should().Be(1);
            _parent.Messages[0].Text.Should().Be("Leo would like to withdraw \u00A32.30");
        }


        [Fact]
        [SetCulture("en-GB")]
        public void Child_should_not_be_able_to_withdraw_more_than_their_balance()
        {
            Action withdraw = () => _child.WithdrawCashFromParent(_parent, 12.11M, "For Toys", _mediator.Object);
            withdraw.Should().Throw<CashWithdrawException>()
                .WithMessage("You can not withdraw \u00A312.11 because you only have \u00A310.00 in your account");
        }


        [Fact]
        [SetCulture("en-GB")]
        public void Should_raise_a_SendMessageEvent()
        {
            _child.WithdrawCashFromParent(_parent, 2.30M, "For Toys", _mediator.Object);

            _mediator.Verify(m => m.Publish(
                It.Is((Domain.Events.SendMessageEvent ev) =>
                    ev.User == _parent && ev.Message == "Leo would like to withdraw \u00A32.30"),
                default(CancellationToken)
            ), Times.Once());
        }
    }
}
