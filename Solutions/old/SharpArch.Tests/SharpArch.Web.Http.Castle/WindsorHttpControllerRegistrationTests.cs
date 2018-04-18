// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable PublicMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.BoxingAllocation
namespace Tests.SharpArch.Web.Http.Castle
{
    using System.Web.Http;
    using FluentAssertions;
    using global::Castle.Core;
    using global::Castle.MicroKernel;
    using global::Castle.Windsor;
    using global::SharpArch.Web.Http.Castle;
    using NUnit.Framework;
    using Tests.Helpers;

    [TestFixture]
    internal class WindsorHttpControllerRegistrationTests: WindsorTestsBase
    {

        [Test]
        public void Should_Ignore_AbstractControllerClasses()
        {
            Container.Kernel.HasComponent(typeof(AbstractController)).Should().BeFalse();
        }

        [Test]
        public void Should_Ignore_Controllers_Without_Controller_Suffix()
        {
            Container.Kernel.HasComponent(typeof(WebApiControllerBase)).Should().BeFalse();
        }

        [Test]
        public void Should_Register_Controllers_With_Controller_Suffix()
        {
            Container.Kernel.HasComponent(typeof(TestController)).Should().BeTrue();
        }

        [Test]
        public void Should_Register_WithScopedLifetime()
        {
            IHandler controllerHandler = Container.Kernel.GetHandler(typeof(TestController));
            controllerHandler.Should().NotBeNull();
            controllerHandler.ComponentModel.LifestyleType.Should().Be(LifestyleType.Scoped);
        }

        protected override void Initialize(WindsorContainer container)
        {
            container.RegisterHttpControllers(typeof(TestController).Assembly);
        }
    }

    #region Controllers

    public class TestController : ApiController
    {
    }

    public abstract class AbstractController : ApiController
    {
    }

    public class WebApiControllerBase : ApiController
    {
    }

    #endregion
}
