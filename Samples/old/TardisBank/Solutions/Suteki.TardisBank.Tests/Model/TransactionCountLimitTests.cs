// ReSharper disable InconsistentNaming
using System.Linq;
using NUnit.Framework;

namespace Suteki.TardisBank.Tests.Model
{
    using FluentAssertions;
    using global::Suteki.TardisBank.Domain;

    [TestFixture]
    public class TransactionCountLimitTests
    {
        Child child;

        [SetUp]
        public void SetUp()
        {
            child = new Parent("Mike", "mike@mike.com", "xxx").CreateChild("Leo", "leo2", "yyy");
        }

        [Test]
        public void When_more_than_max_transactions_created_transactions_should_be_truncated()
        {
            for (int i = 0; i < Account.MaxTransactions; i++)
            {
                child.ReceivePayment(1M, "payment" + i);
            }

            child.Account.Balance.Should().Be(100M);
            child.Account.Transactions.Count.Should().Be(Account.MaxTransactions);

            child.ReceivePayment(2M, "payment_new");

            child.Account.Balance.Should().Be(102M);
            child.Account.Transactions.Count.Should().Be(Account.MaxTransactions);
            child.Account.OldTransactionsBalance.Should().Be(1M);

            child.Account.Transactions.First().Description.Should().Be("payment1");
            child.Account.Transactions.Last().Description.Should().Be("payment_new");

            child.ReceivePayment(3.55M, "payment_new2");

            child.Account.Balance.Should().Be(105.55M);
            child.Account.Transactions.Count.Should().Be(Account.MaxTransactions);
            child.Account.OldTransactionsBalance.Should().Be(2M);

            child.Account.Transactions.First().Description.Should().Be("payment2");
            child.Account.Transactions.Last().Description.Should().Be("payment_new2");
        }
    }
}
// ReSharper restore InconsistentNaming