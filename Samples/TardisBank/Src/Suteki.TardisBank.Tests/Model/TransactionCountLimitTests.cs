namespace Suteki.TardisBank.Tests.Model;

using Domain;
using FluentAssertions;
using Xunit;


public class TransactionCountLimitTests
{
    readonly Child _child;

    public TransactionCountLimitTests()
    {
        _child = new Parent("Mike", "mike@mike.com", "xxx").CreateChild("Leo", "leo2", "yyy");
    }

    [Fact]
    public void When_more_than_max_transactions_created_transactions_should_be_truncated()
    {
        for (int i = 0; i < Account.MaxTransactions; i++)
        {
            _child.ReceivePayment(1M, "payment" + i);
        }

        _child.Account.Balance.Should().Be(100M);
        _child.Account.Transactions.Count.Should().Be(Account.MaxTransactions);

        _child.ReceivePayment(2M, "payment_new");

        _child.Account.Balance.Should().Be(102M);
        _child.Account.Transactions.Count.Should().Be(Account.MaxTransactions);
        _child.Account.OldTransactionsBalance.Should().Be(1M);

        _child.Account.Transactions.First().Description.Should().Be("payment1");
        _child.Account.Transactions.Last().Description.Should().Be("payment_new");

        _child.ReceivePayment(3.55M, "payment_new2");

        _child.Account.Balance.Should().Be(105.55M);
        _child.Account.Transactions.Count.Should().Be(Account.MaxTransactions);
        _child.Account.OldTransactionsBalance.Should().Be(2M);

        _child.Account.Transactions.First().Description.Should().Be("payment2");
        _child.Account.Transactions.Last().Description.Should().Be("payment_new2");
    }
}