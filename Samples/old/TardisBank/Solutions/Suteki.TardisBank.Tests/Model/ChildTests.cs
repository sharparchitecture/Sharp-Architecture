// ReSharper disable InconsistentNaming

namespace Suteki.TardisBank.Tests.Model
{
    using System;
    using Domain;
    using FluentAssertions;
    using NUnit.Framework;
    using SharpArch.NHibernate;
    using SharpArch.Testing.NUnit;
    using SharpArch.Testing.NUnit.NHibernate;

    [TestFixture]
    public class ChildTests : RepositoryTestsBase
    {
        int childId;
        int parentId;

        protected override void LoadTestData()
        {
            var parent = new Parent("Mike Hadlow", "mike@yahoo.com", "yyy");
            Session.Save(parent);

            parentId = parent.Id;

            var child = parent.CreateChild("Leo", "leohadlow", "xxx");
            Session.Save(child);
            FlushSessionAndEvict(child);
            FlushSessionAndEvict(parent);
            childId = child.Id;
        }

        [Test]
        public void Should_be_able_to_add_schedule_to_account()
        {
            var childRepository = new LinqRepository<Child>(TransactionManager, Session);
            var childToTestOn = childRepository.Get(childId);
            childToTestOn.Account.AddPaymentSchedule(DateTime.UtcNow, Interval.Week, 10, "Weekly pocket money");
            FlushSessionAndEvict(childToTestOn);

            var child = childRepository.Get(childId);
            child.Account.PaymentSchedules[0].Id.Should().BePositive("schedule was not persisted");
        }

        [Test]
        public void Should_be_able_to_add_transaction_to_account()
        {
            var childRepository = new LinqRepository<Child>(TransactionManager, Session);
            var childToTestOn = childRepository.Get(childId);
            childToTestOn.ReceivePayment(10, "Reward");
            FlushSessionAndEvict(childToTestOn);

            var child = childRepository.Get(childId);
            child.Account.Transactions[0].Id.Should().BePositive();
        }

        [Test]
        public void Should_be_able_to_create_and_retrieve_a_child()
        {
            var child = new LinqRepository<Child>(TransactionManager, Session).Get(childId);
            child.Name.Should().Be("Leo");
            child.UserName.Should().Be("leohadlow");
            child.ParentId.Should().Be(parentId);
            child.Password.Should().Be("xxx");
            child.Account.Should().NotBeNull();
        }
    }
}

// ReSharper restore InconsistentNaming