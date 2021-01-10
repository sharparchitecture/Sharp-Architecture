﻿using SharpArch.NHibernate.MultiDb;

namespace Tests.SharpArch.NHibernate.MultiDb
{
    using System;
    using FluentAssertions;
    using global::NHibernate;
    using global::SharpArch.NHibernate;
    using Moq;
    using Xunit;


    public class SessionFactoryRegistryTests
    {
        readonly SessionFactoryRegistry _sessionFactoryRegistry;

        public SessionFactoryRegistryTests()
        {
            _sessionFactoryRegistry = new SessionFactoryRegistry();
        }

        [Fact]
        public void Should_Defer_Factory_Creation_Until_Get_is_Called()
        {
            int counter = 0;
            var factoryBuilder = new Mock<INHibernateSessionFactoryBuilder>();
            factoryBuilder.Setup(x => x.BuildSessionFactory()).Returns(Mock.Of<ISessionFactory>)
                .Callback(() => counter++);
            _sessionFactoryRegistry.Add("1", factoryBuilder.Object);

            counter.Should().Be(0, "should defer SessionFactory initialization till GetSessionFactory() called");

            var sessionFactory = _sessionFactoryRegistry.GetSessionFactory("1");
            counter.Should().Be(1);

            _sessionFactoryRegistry.GetSessionFactory("1").Should().BeEquivalentTo(sessionFactory);
            counter.Should().Be(1, "SessionFactory should be cached");
        }

        [Fact]
        public void Dispose_Should_Dispose_Initialized_SessionFactory()
        {
            var sessionFactory = new Mock<ISessionFactory>();
            var disposableSessionFactory = sessionFactory.As<IDisposable>();
            var factoryBuilderMock = new Mock<INHibernateSessionFactoryBuilder>();
            factoryBuilderMock.Setup(x => x.BuildSessionFactory()).Returns(sessionFactory.Object);

            _sessionFactoryRegistry.Add("1", factoryBuilderMock.Object);
            _sessionFactoryRegistry.GetSessionFactory("1");

            _sessionFactoryRegistry.Dispose();
            disposableSessionFactory.Verify(d => d.Dispose());
        }
    }
}
