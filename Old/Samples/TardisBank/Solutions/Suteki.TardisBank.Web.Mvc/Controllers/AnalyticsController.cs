namespace Suteki.TardisBank.Web.Mvc.Controllers
{
    using System.Web.Mvc;

    using Suteki.TardisBank.Tasks;

    public class AnalyticsController : Controller
    {
        readonly TardisConfiguration configuration;

        public AnalyticsController(TardisConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [ChildActionOnly]
        public PartialViewResult Index()
        {
            return this.PartialView(this.configuration);
        }
    }
}