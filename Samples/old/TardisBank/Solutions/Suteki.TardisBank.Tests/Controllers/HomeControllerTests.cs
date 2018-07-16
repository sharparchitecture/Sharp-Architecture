// ReSharper disable InconsistentNaming
using NUnit.Framework;

using Suteki.TardisBank.Tests.Helpers;

namespace Suteki.TardisBank.Tests.Controllers
{
    using global::Suteki.TardisBank.Web.Mvc.Controllers;

    [TestFixture]
    public class HomeControllerTests
    {
        HomeController homeController;

        [SetUp]
        public void SetUp()
        {
            homeController = new HomeController();
        }

        [Test]
        public void Should_show_index_view()
        {
            homeController.Index()
                .ReturnsViewResult()
                .ForView("Index");
        }
    }
}
// ReSharper restore InconsistentNaming