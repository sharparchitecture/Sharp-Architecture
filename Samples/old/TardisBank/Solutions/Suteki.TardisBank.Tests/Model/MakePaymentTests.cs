// ReSharper disable InconsistentNaming
using System;
using NUnit.Framework;

namespace Suteki.TardisBank.Tests.Model
{
    using FluentAssertions;
    using global::Suteki.TardisBank.Domain;

    [TestFixture]
    public class MakePaymentTests
    {
        Parent parent;
        Child child;

        Parent somebodyElse;
        Child somebodyElsesChild;

        [SetUp]
        public void SetUp()
        {
            parent = new Parent("Mike Hadlow", "mike@yahoo.com", "pwd");
            child = parent.CreateChild("Leo", "leohadlow", "xxx");

            somebodyElse = new Parent("John Robinson", "john@gmail.com", "pwd");
            somebodyElsesChild = somebodyElse.CreateChild("Jim", "jimrobinson", "yyy");
        }

        [Test]
        public void Should_be_able_to_make_a_payment()
        {
            parent.MakePaymentTo(child, 2.30M);

            child.Account.Transactions.Count.Should().Be(1);
            child.Account.Transactions[0].Amount.Should().Be(2.30M);
            child.Account.Transactions[0].Description.Should().Be("Payment from Mike Hadlow");
            child.Account.Transactions[0].Date.Should().Be(DateTime.Now.Date);
            child.Account.Balance.Should().Be(2.30M);
        }

        [Test]
        public void Should_not_be_able_to_make_a_payment_to_somebody_elses_child()
        {
            Action makePayment = () => parent.MakePaymentTo(somebodyElsesChild, 4.50M);
            makePayment.ShouldThrow<TardisBankException>().WithMessage("Jim is not a child of Mike Hadlow");
        }
    }
}
// ReSharper restore InconsistentNaming