namespace Suteki.TardisBank.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using FluentAssertions;
    using NHibernate.Linq;
    using SharpArch.Testing.Xunit.NHibernate;
    using Tasks;
    using Xunit;


    public class PaymentSchedulingQueryTests : TransientDatabaseTests<TransientDatabaseSetup>
    {
        Parent _parent;

        DateTime _someDate;

        public PaymentSchedulingQueryTests(TransientDatabaseSetup dbSetup)
            : base(dbSetup)
        {
        }

        protected override async Task LoadTestData(CancellationToken cancellationToken)
        {
            _parent = new Parent("parent", "parent", "xxx");
            await Session.SaveAsync(_parent, cancellationToken);
            _someDate = new DateTime(2010, 4, 5);
            await Session.SaveAsync(CreateChildWithSchedule("one", 1M, _someDate.AddDays(-2)), cancellationToken).ConfigureAwait(false);
            await Session.SaveAsync(CreateChildWithSchedule("two", 2M, _someDate.AddDays(-1)), cancellationToken).ConfigureAwait(false);
            await Session.SaveAsync(CreateChildWithSchedule("three", 3M, _someDate), cancellationToken).ConfigureAwait(false);
            await Session.SaveAsync(CreateChildWithSchedule("four", 4M, _someDate.AddDays(1)), cancellationToken).ConfigureAwait(false);
            await Session.SaveAsync(CreateChildWithSchedule("five", 5M, _someDate.AddDays(2)), cancellationToken).ConfigureAwait(false);
            await Session.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        Child CreateChildWithSchedule(string name, decimal amount, DateTime startDate)
        {
            Child child = _parent.CreateChild(name, name, "xxx");
            child.Account.AddPaymentSchedule(startDate, Interval.Week, amount, "Pocket Money");
            return child;
        }

        [Fact]
        public async Task Should_be_able_to_query_all_pending_scheduled_payments()
        {
            ISchedulerService schedulerService = new SchedulerService(Session);
            schedulerService.ExecuteUpdates(_someDate);
            await Session.FlushAsync();

            // check results

            List<Child> results = await Session.Query<Child>().ToListAsync();

            results.Should().HaveCount(5);

            results.Single(x => x.Name == "one").Account.PaymentSchedules[0].NextRun.Should().Be(_someDate.AddDays(5));
            results.Single(x => x.Name == "two").Account.PaymentSchedules[0].NextRun.Should().Be(_someDate.AddDays(6));
            results.Single(x => x.Name == "three").Account.PaymentSchedules[0].NextRun.Should().Be(_someDate.AddDays(7));
            results.Single(x => x.Name == "four").Account.PaymentSchedules[0].NextRun.Should().Be(_someDate.AddDays(1));
            results.Single(x => x.Name == "five").Account.PaymentSchedules[0].NextRun.Should().Be(_someDate.AddDays(2));

            results.Single(x => x.Name == "one").Account.Transactions.Count.Should().Be(1);
            results.Single(x => x.Name == "one").Account.Transactions[0].Amount.Should().Be(1M);
        }
    }
}
