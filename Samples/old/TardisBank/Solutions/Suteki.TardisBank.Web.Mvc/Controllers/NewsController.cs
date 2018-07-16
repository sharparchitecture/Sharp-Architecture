namespace Suteki.TardisBank.Web.Mvc.Controllers
{
    using System;
    using System.Web.Mvc;
    using Suteki.TardisBank.Domain;
    using Suteki.TardisBank.Tasks;
    using Suteki.TardisBank.Web.Mvc.Models.News;

    public class NewsController : Controller
    {
        readonly IHttpContextService context;


        /// <summary>
        /// Initializes a new instance of the <see cref="NewsController"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name=""/> is <see langword="null" />.</exception>
        public NewsController(IHttpContextService context)
        {
            if (context == null) throw new ArgumentNullException("context");
            this.context = context;
        }


        // GET: News
        public ActionResult Index()
        {
            var vm = new NewsListModel
            {
                IsParent = context.UserIsInRole(UserRoles.Parent),
            };

            return View(vm);
        }

        [Authorize(Roles = UserRoles.Parent)]
        public ActionResult Edit()
        {
            return View();
        }
    }
}