// ReSharper disable InconsistentNaming

namespace Suteki.TardisBank.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using FluentAssertions;
    using NHibernate.Linq;
    using NUnit.Framework;
    using SharpArch.Testing.NUnit.NHibernate;
    using Tasks;

    [TestFixture]
    public class PaymentSchedulingQueryTests : RepositoryTestsBase
    {
        Parent parent;

        DateTime someDate;

        protected override void LoadTestData()
        {
            parent = new Parent("parent", "parent", "xxx");
            Session.Save(parent);
            this.someDate = new DateTime(2010, 4, 5);
            Session.Save(CreateChildWithSchedule("one", 1M, this.someDate.AddDays(-2)));
            Session.Save(CreateChildWithSchedule("two", 2M, this.someDate.AddDays(-1)));
            Session.Save(CreateChildWithSchedule("three", 3M, this.someDate));
            Session.Save(CreateChildWithSchedule("four", 4M, this.someDate.AddDays(1)));
            Session.Save(CreateChildWithSchedule("five", 5M, this.someDate.AddDays(2)));
            Session.Flush();
        }

        Child CreateChildWithSchedule(string name, decimal amount, DateTime startDate)
        {
            Child child = parent.CreateChild(name, name, "xxx");
            child.Account.AddPaymentSchedule(startDate, Interval.Week, amount, "Pocket Money");
            return child;
        }

        [Test]
        public void Should_be_able_to_query_all_pending_scheduled_payments()
        {
            ISchedulerService schedulerService = new SchedulerService(Session);
            schedulerService.ExecuteUpdates(someDate);
            Session.Flush();

            // check results

            List<Child> results = Session.Query<Child>().ToList();

            results.Count().Should().Be(5);

            results.Single(x => x.Name == "one").Account.PaymentSchedules[0].NextRun.Should().Be(someDate.AddDays(5));
            results.Single(x => x.Name == "two").Account.PaymentSchedules[0].NextRun.Should().Be(someDate.AddDays(6));
            results.Single(x => x.Name == "three").Account.PaymentSchedules[0].NextRun.Should().Be(someDate.AddDays(7));
            results.Single(x => x.Name == "four").Account.PaymentSchedules[0].NextRun.Should().Be(someDate.AddDays(1));
            results.Single(x => x.Name == "five").Account.PaymentSchedules[0].NextRun.Should().Be(someDate.AddDays(2));

            results.Single(x => x.Name == "one").Account.Transactions.Count.Should().Be(1);
            results.Single(x => x.Name == "one").Account.Transactions[0].Amount.Should().Be(1M);
        }
    }
}

// ReSharper restore InconsistentNaming
