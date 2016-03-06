using Northwind.ApplicationServices;
using NUnit.Framework;
using Northwind.Web.Controllers;
using System.Web.Mvc;
using Rhino.Mocks;
using MvcContrib.TestHelper;

namespace Tests.Northwind.Web.Controllers
{
    [TestFixture]
    public class DashboardControllerTests
    {
        [Test]
        public void CanShowSummaryData() {
            DashboardController controller =
                new DashboardController(CreateMockDashboardServiceRepository());

            ViewResult result =
                controller.Index().AssertViewRendered().ForView("");

            Assert.That(result.ViewData, Is.Not.Null);
            Assert.That(result.ViewData.Model as DashboardService.DashboardSummaryDto, Is.Not.Null);

            // I could go on to test the internals of the summary DTO, but I don't really care
            // since here I'm mostly concerned the the controller is getting a response from the
            // application service and giving the summary DTO to the view.  The application service
            // unit tests verify that the service itself is returning valid data.
        }

        public IDashboardService CreateMockDashboardServiceRepository() {
            IDashboardService dashboardService = MockRepository.GenerateMock<IDashboardService>( );
            dashboardService.Expect(ds => ds.GetDashboardSummary()).Return(new DashboardService.DashboardSummaryDto());
            return dashboardService;
        }
    }
}
