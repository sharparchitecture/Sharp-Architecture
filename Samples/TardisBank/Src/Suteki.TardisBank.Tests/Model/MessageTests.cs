namespace Suteki.TardisBank.Tests.Model
{
    using Domain;
    using FluentAssertions;
    using MediatR;
    using Moq;
    using SharpArch.NHibernate;
    using SharpArch.Testing.NHibernate;
    using SharpArch.Testing.Xunit.NHibernate;
    using Xunit;


    public class MessageTests : TransientDatabaseTests<TransientDatabaseSetup>
    {
        int _userId;
        Mock<IMediator> _mediator;

        public MessageTests(TransientDatabaseSetup dbSetup) : base(dbSetup)
        {
            _mediator = new Mock<IMediator>();
        }

        protected override void LoadTestData()
        {
            User user = new Parent("Dad", "mike@mike.com", "xxx");
            Session.Save(user);

            FlushSessionAndEvict(user);
            _userId = user.Id;
        }

        [Fact]
        public void Should_be_able_to_add_a_message_to_a_user()
        {
            var parentRepository = new LinqRepository<Parent>(TransactionManager);
            User userToTestWith = parentRepository.Get(_userId);

            userToTestWith.SendMessage("some message", _mediator.Object);

            FlushSessionAndEvict(userToTestWith);

            Parent parent = parentRepository.Get(_userId);
            parent.Messages.Count.Should().Be(1);
        }
    }
}
