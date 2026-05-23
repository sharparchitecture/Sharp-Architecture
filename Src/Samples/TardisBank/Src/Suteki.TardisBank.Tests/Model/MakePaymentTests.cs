namespace Suteki.TardisBank.Tests.Model;

using Domain;
using Shouldly;
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

        _child.Account.Transactions.Count.ShouldBe(1);
        _child.Account.Transactions[0].Amount.ShouldBe(2.30M);
        _child.Account.Transactions[0].Description.ShouldBe("Payment from Mike Hadlow");
        _child.Account.Transactions[0].Date.ShouldBe(DateTime.Now.Date);
        _child.Account.Balance.ShouldBe(2.30M);
    }

    [Fact]
    public void Should_not_be_able_to_make_a_payment_to_somebody_elses_child()
    {
        Action makePayment = () => _parent.MakePaymentTo(_somebodyElsesChild, 4.50M);
        var ex = makePayment.ShouldThrow<TardisBankException>();
        ex.Message.ShouldBe("Jim is not a child of Mike Hadlow");
    }
}
