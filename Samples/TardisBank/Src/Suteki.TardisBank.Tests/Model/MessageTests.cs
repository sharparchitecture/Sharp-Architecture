namespace Suteki.TardisBank.Tests.Model
{
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using FluentAssertions;
    using MediatR;
    using Moq;
    using SharpArch.NHibernate;
    using SharpArch.NHibernate.Impl;
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

        protected override async Task LoadTestData(CancellationToken cancellationToken)
        {
            User user = new Parent("Dad", "mike@mike.com", "xxx");
            await Session.SaveAsync(user, cancellationToken);

            await FlushSessionAndEvict(user, cancellationToken);
            _userId = user.Id;
        }

        [Fact]
        public async Task Should_be_able_to_add_a_message_to_a_user()
        {
            var parentRepository = new LinqRepository<Parent, int>(TransactionManager);
            User userToTestWith = await parentRepository.GetAsync(_userId);

            userToTestWith.SendMessage("some message", _mediator.Object);

            await FlushSessionAndEvict(userToTestWith);

            Parent parent = await parentRepository.GetAsync(_userId);
            parent.Messages.Count.Should().Be(1);
        }
    }
}
