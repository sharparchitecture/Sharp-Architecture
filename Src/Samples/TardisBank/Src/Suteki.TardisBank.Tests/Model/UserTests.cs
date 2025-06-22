namespace Suteki.TardisBank.Tests.Model;

using Domain;
using Shouldly;
using SharpArch.Testing.Xunit.NHibernate;
using Xunit;


public class UserTests : TransientDatabaseTests<TransientDatabaseSetup>
{
    public UserTests(TransientDatabaseSetup dbSetup)
        : base(dbSetup)
    {
    }

    protected override async Task LoadTestData(CancellationToken cancellationToken)
    {
        var mike = new Parent("Mike Hadlow", "mike@yahoo.com", "yyy");
        await Session.SaveAsync(mike, cancellationToken);

        Child leo = mike.CreateChild("Leo", "leohadlow", "xxx");
        Child yuna = mike.CreateChild("Yuna", "yunahadlow", "xxx");
        await Session.SaveAsync(leo, cancellationToken);
        await Session.SaveAsync(yuna, cancellationToken);

        var john = new Parent("John Robinson", "john@gmail.com", "yyy");
        await Session.SaveAsync(john, cancellationToken);

        Child jim = john.CreateChild("Jim", "jimrobinson", "xxx");
        await Session.SaveAsync(jim, cancellationToken);

        await Session.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    [Fact]
    public void Should_be_able_to_treat_Parents_and_Children_Polymorphically()
    {
        User[] users = Session.Query<User>().ToArray();

        users.Length.ShouldBe(5);

        users[0].ShouldBeOfType<Parent>();
        users[1].GetType().Name.ShouldBe("Child");
        users[2].GetType().Name.ShouldBe("Child");
        users[3].GetType().Name.ShouldBe("Parent");
        users[4].GetType().Name.ShouldBe("Child");
    }
}