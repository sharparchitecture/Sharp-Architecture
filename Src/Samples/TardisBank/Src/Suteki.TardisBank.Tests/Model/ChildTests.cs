namespace Suteki.TardisBank.Tests.Model;

using Domain;
using FluentAssertions;
using SharpArch.NHibernate;
using SharpArch.Testing.Xunit.NHibernate;
using Xunit;


public class ChildTests : TransientDatabaseTests<TransientDatabaseSetup>
{
    readonly LinqRepository<Child, int> _childRepository;
    int _childId;
    int _parentId;

    /// <inheritdoc />
    public ChildTests(TransientDatabaseSetup dbSetup)
        : base(dbSetup)
    {
        _childRepository = new LinqRepository<Child, int>(TransactionManager);
    }

    protected override async Task LoadTestData(CancellationToken cancellationToken)
    {
        var parent = new Parent("Mike Hadlow", "mike@yahoo.com", "yyy");
        await Session.SaveAsync(parent, cancellationToken);

        _parentId = parent.Id;

        var child = parent.CreateChild("Leo", "leohadlow", "xxx");
        await Session.SaveAsync(child, cancellationToken);
        await Session.FlushAndEvictAsync(cancellationToken, child, parent);
        _childId = child.Id;
    }

    [Fact]
    public async Task Should_be_able_to_add_schedule_to_account()
    {
        var childToTestOn = (await _childRepository.GetAsync(_childId))!;
        childToTestOn.Should().NotBeNull();

        childToTestOn.Account.AddPaymentSchedule(DateTime.UtcNow, Interval.Week, 10, "Weekly pocket money");
        await FlushSessionAndEvict(childToTestOn);

        var child = (await _childRepository.GetAsync(_childId))!;
        child.Should().NotBeNull();
        child.Account.PaymentSchedules[0].Id.Should().BePositive("schedule was not persisted");
    }

    [Fact]
    public async Task Should_be_able_to_add_transaction_to_account()
    {
        var childToTestOn = (await _childRepository.GetAsync(_childId))!;
        childToTestOn.ReceivePayment(10, "Reward");
        await FlushSessionAndEvict(childToTestOn);

        var child = (await _childRepository.GetAsync(_childId))!;
        child.Account.Transactions[0].Id.Should().BePositive();
    }

    [Fact]
    public async Task Should_be_able_to_create_and_retrieve_a_child()
    {
        var child = (await _childRepository.GetAsync(_childId))!;
        child.Name.Should().Be("Leo");
        child.UserName.Should().Be(@"leohadlow");
        child.ParentId.Should().Be(_parentId);
        child.Password.Should().Be("xxx");
        child.Account.Should().NotBeNull();
    }
}