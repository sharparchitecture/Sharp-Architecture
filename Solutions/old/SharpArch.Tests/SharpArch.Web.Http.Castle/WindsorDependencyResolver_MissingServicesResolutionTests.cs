// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident
namespace Tests.SharpArch.Web.Http.Castle
{
    using FluentAssertions;
    using global::Castle.Windsor;
    using global::SharpArch.Web.Http.Castle;
    using NUnit.Framework;
    using Tests.Helpers;

    [TestFixture]
    class WindsorDependencyResolver_MissingServicesResolutionTests : WindsorTestsBase
    {
        WindsorDependencyResolver dependencyResolver;

        protected override void Initialize(WindsorContainer container)
        {
            this.dependencyResolver = new WindsorDependencyResolver(container);
        }


        [Test]
        public void GetService_ShouldReturn_Null()
        {
            this.dependencyResolver.GetService(typeof(ITestService)).Should().BeNull();
        }

        [Test]
        public void GetServices_ShouldReturn_EmptyEnumeration()
        {
            this.dependencyResolver.GetServices(typeof(ITestService)).Should().BeEmpty();
        }
    }
}
