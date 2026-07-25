namespace Suteki.TardisBank.Tests.Model;

using Domain;
using SharpArch.NHibernate;
using SharpArch.Testing.Xunit.NHibernate;
using Shouldly;
using Xunit;


public class ParentTests : TransientDatabaseTests<TransientDatabaseSetup>
{
    int _parentId;

    public ParentTests(TransientDatabaseSetup dbSetup)
        : base(dbSetup)
    {
    }

    protected override async Task LoadTestData(CancellationToken cancellationToken)
    {
        var parent = new Parent("Mike Hadlow", string.Format("{0}@yahoo.com", "mike"), "yyy");
        await Session.SaveAsync(parent, cancellationToken);
        await FlushSessionAndEvict(parent, cancellationToken);
        _parentId = parent.Id;
    }

    [Fact]
    public async Task Should_be_able_to_add_a_child_to_a_parent()
    {
        var linqRepository = new LinqRepository<Parent, int>(TransactionManager);
        Parent savedParent = (await linqRepository.GetAsync(_parentId))!;
        savedParent.ShouldNotBeNull();

        savedParent.CreateChild("jim", "jim123", "passw0rd1");
        savedParent.CreateChild("jenny", "jenny123", "passw0rd2");
        savedParent.CreateChild("jez", "jez123", "passw0rd3");
        await FlushSessionAndEvict(savedParent);

        Parent parent = (await linqRepository.GetAsync(_parentId))!;
        parent.Children.Count.ShouldBe(3);

        parent.Children[0].Name.ShouldBe("jim");
        parent.Children[1].Name.ShouldBe("jenny");
        parent.Children[2].Name.ShouldBe("jez");
    }

    [Fact]
    public async Task Should_be_able_to_create_and_retrieve_Parent()
    {
        Parent parent = (await new LinqRepository<Parent, int>(TransactionManager).GetAsync(_parentId))!;
        parent.ShouldNotBeNull();
        parent.Name.ShouldBe("Mike Hadlow");
        parent.UserName.ShouldBe("mike@yahoo.com");
        parent.Children.ShouldNotBeNull();
    }
}
