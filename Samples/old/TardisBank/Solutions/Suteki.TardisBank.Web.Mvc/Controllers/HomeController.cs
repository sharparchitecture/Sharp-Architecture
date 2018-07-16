namespace Suteki.TardisBank.Web.Mvc.Controllers
{
    using System.Web.Mvc;

    using Suteki.TardisBank.Domain;

    public class HomeController : Controller
    {
        public ViewResult Index()
        {
            return this.View("Index");
        }

        public ViewResult Error()
        {
            // throw an error for testing
            throw new TardisBankException("Something really bad happened!");
        }

        public ActionResult NotFound()
        {
            return new HttpStatusCodeResult(404);
        }

        public ViewResult About()
        {
            return this.View();
        }

        public ViewResult Legal()
        {
            return this.View();
        }
    }
}