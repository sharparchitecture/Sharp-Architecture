// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
namespace Tests.SharpArch.Web.Http.Castle
{
    using System.Linq;
    using FluentAssertions;
    using global::Castle.MicroKernel.Registration;
    using global::Castle.Windsor;
    using global::SharpArch.Web.Http.Castle;
    using NUnit.Framework;
    using Tests.Helpers;

    [TestFixture]
    class WindsorDependencyResolver_RegisteredServicesResolutionTests : WindsorTestsBase
    {
        private WindsorDependencyResolver dependencyResolver;

        protected override void Initialize(WindsorContainer container)
        {
            container.Register(Component.For<ITestService>().ImplementedBy<TestService>().Named("1"));
            container.Register(Component.For<ITestService>().ImplementedBy<TestService>().Named("2"));

            this.dependencyResolver = new WindsorDependencyResolver(container);
        }

        [Test]
        public void GetService_Should_ReturnService()
        {
            this.dependencyResolver.GetService(typeof(ITestService)).Should().BeOfType<TestService>();
        }

        [Test]
        public void GetServices_Should_ReturnAllRegisteredServices()
        {
            var services = this.dependencyResolver.GetServices(typeof(ITestService)).ToArray();
            services.Should().HaveCount(2);
            services.Should().OnlyContain(s => s.GetType() == typeof(TestService));
        }
    }
}