namespace Northwind.Web.Mvc.Controllers
{
    using System.Web.Mvc;

    using Northwind.Domain.Contracts.Tasks;

    [HandleError]
    public class CustomersController : Controller
    {
        private readonly ICustomerTasks customerTasks;

        public CustomersController(ICustomerTasks customerTasks)
        {
            this.customerTasks = customerTasks;
        }

        /// <summary>
        ///   An example of creating an object with an assigned ID.  Because this uses a declarative 
        ///   transaction, everything within this method is wrapped within a single transaction.
        /// 
        ///   I'd like to be perfectly clear that I think assigned IDs are almost always a terrible
        ///   idea; this is a major complaint I have with the Northwind database.  With that said, 
        ///   some legacy databases require such techniques.
        /// </summary>
        public ActionResult Create(string customerName, string assignedId)
        {
            var customerToCreate = this.customerTasks.Create(customerName, assignedId);
            return View(customerToCreate);
        }

        /// <summary>
        ///   The transaction on this action is optional, but recommended for performance reasons
        /// </summary>
        public ActionResult Index()
        {
            var customers = this.customerTasks.GetCustomersByCountry("Venezuela");
            return View(customers);
        }
    }
}