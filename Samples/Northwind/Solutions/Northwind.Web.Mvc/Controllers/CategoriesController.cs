namespace Northwind.Web.Mvc.Controllers
{
    using System.Web.Mvc;

    using Northwind.Domain.Contracts.Tasks;

    [HandleError]
    public class CategoriesController : Controller
    {
        private readonly ICategoryTasks categoryTasks;

        public CategoriesController(ICategoryTasks categoryTasks)
        {
            this.categoryTasks = categoryTasks;
        }

        /// <summary>
        ///   An example of creating an object with an auto incrementing ID.
        /// 
        ///   Because this uses a declarative transaction, everything within this method is wrapped
        ///   within a single transaction.
        /// </summary>
        public ActionResult Create(string categoryName)
        {
            var category = this.categoryTasks.Create(categoryName);
            return View(category);
        }

        /// <summary>
        ///   The transaction on this action is optional, but recommended for performance reasons
        /// </summary>
        public ActionResult Index()
        {
            var categories = this.categoryTasks.GetAllCategories();
            return View(categories);
        }

        /// <summary>
        ///   The transaction on this action is optional, but recommended for performance reasons
        /// </summary>
        public ActionResult Show(int id)
        {
            var category = this.categoryTasks.GetCategoryById(id);
            return View(category);
        }
    }
}