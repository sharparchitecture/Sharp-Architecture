namespace Northwind.Web.Mvc.Controllers
{
    using Northwind.Domain.Contracts.Tasks;

    using System.Web.Mvc;

    using SharpArch.Domain;

    public class DashboardController : Controller
    {
        private readonly IDashboardTasks dashboardService;

        /// <summary>
        ///   Note that the application service gets injected into the controller.  Since it's not a 
        ///   repository (which gets automatically wired up for dependency injection), the service 
        ///   needs to be manually registered within Northwind.Web.CastleWindsor.ComponentRegistrar
        /// </summary>
        public DashboardController(IDashboardTasks dashboardTasks)
        {
            Check.Require(dashboardTasks != null, "dashboardService may not be null");

            this.dashboardService = dashboardTasks;
        }

        /// <summary>
        ///   Uses the application summary to collate the dashboard summary information
        /// </summary>
        public ActionResult Index()
        {
            var summary = this.dashboardService.GetDashboardSummary();

            return View(summary);
        }
    }
}