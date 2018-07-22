namespace Suteki.TardisBank.Tests.Model
{
    using System;
    using Domain;
    using FluentAssertions;
    using Xunit;


    public class MakePaymentTests
    {
        readonly Parent _parent;
        readonly Child _child;

        readonly Parent _somebodyElse;
        readonly Child _somebodyElsesChild;

        public MakePaymentTests()
        {
            _parent = new Parent("Mike Hadlow", "mike@yahoo.com", "pwd");
            _child = _parent.CreateChild("Leo", @"leohadlow", "xxx");

            _somebodyElse = new Parent("John Robinson", "john@gmail.com", "pwd");
            _somebodyElsesChild = _somebodyElse.CreateChild("Jim", "jimrobinson", "yyy");
        }

        [Fact]
        public void Should_be_able_to_make_a_payment()
        {
            _parent.MakePaymentTo(_child, 2.30M);

            _child.Account.Transactions.Count.Should().Be(1);
            _child.Account.Transactions[0].Amount.Should().Be(2.30M);
            _child.Account.Transactions[0].Description.Should().Be("Payment from Mike Hadlow");
            _child.Account.Transactions[0].Date.Should().Be(DateTime.Now.Date);
            _child.Account.Balance.Should().Be(2.30M);
        }

        [Fact]
        public void Should_not_be_able_to_make_a_payment_to_somebody_elses_child()
        {
            Action makePayment = () => _parent.MakePaymentTo(_somebodyElsesChild, 4.50M);
            makePayment.Should().Throw<TardisBankException>().WithMessage("Jim is not a child of Mike Hadlow");
        }
    }
}
