namespace Suteki.TardisBank.Tests.Model;

using Domain;
using FluentAssertions;
using Humanizer;
using Xunit;


public class PaymentSchedulingTests
{
    const Interval Interval = Domain.Interval.Week;
    const decimal Amount = 2.00M;
    const string Description = "Weekly Pocket Money";
    readonly DateTime _startDate = new DateTime(2010, 10, 28);

    readonly Child _child;

    public PaymentSchedulingTests()
    {
        _child = new Parent("Dad", "mike@mike.com", "xxx")
            .CreateChild("Leo", @"leohadlow", "yyy");
    }

    [Fact]
    public void Should_be_able_to_add_a_schedule_to_an_account()
    {
        _child.Account.AddPaymentSchedule(_startDate, Interval, Amount, Description);

        _child.Account.PaymentSchedules.Count.Should().Be(1);
        _child.Account.PaymentSchedules[0].NextRun.Should().Be(_startDate);
        _child.Account.PaymentSchedules[0].Interval.Should().Be(Interval);
        _child.Account.PaymentSchedules[0].Amount.Should().Be(Amount);
        _child.Account.PaymentSchedules[0].Description.Should().Be(Description);
        _child.Account.PaymentSchedules[0].Id.Should().Be(0);
    }

    [Fact]
    public void Should_be_able_to_remove_a_payment_schedule()
    {
        _child.Account.AddPaymentSchedule(_startDate, Interval, Amount, Description);

        var id = _child.Account.PaymentSchedules[0].Id;
        _child.Account.RemovePaymentSchedule(id);

        _child.Account.PaymentSchedules.Count.Should().Be(0);
    }

    [Fact]
    public void Should_be_able_to_trigger_the_payment()
    {
        _child.Account.AddPaymentSchedule(_startDate, Interval, Amount, Description);

        _child.Account.TriggerScheduledPayments(_startDate);

        _child.Account.Transactions.Count.Should().Be(1);
        _child.Account.Transactions[0].Amount.Should().Be(Amount);
        _child.Account.Transactions[0].Description.Should().Be(Description);
        _child.Account.Transactions[0].Date.Should().Be(DateTime.Now.Date);

        var expectedNextRun = _startDate + 1.Weeks();
        _child.Account.PaymentSchedules[0].NextRun.Should().Be(expectedNextRun);
    }

    [Fact]
    public void Triggering_payment_before_next_run_causes_nothing_to_happen()
    {
        _child.Account.AddPaymentSchedule(_startDate, Interval, Amount, Description);
        _child.Account.TriggerScheduledPayments(_startDate.AddMinutes(-1));

        _child.Account.Transactions.Count.Should().Be(0);
        _child.Account.PaymentSchedules[0].NextRun.Should().Be(_startDate);
    }
}