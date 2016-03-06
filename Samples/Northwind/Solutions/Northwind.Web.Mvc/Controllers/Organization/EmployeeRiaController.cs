namespace Northwind.Web.Mvc.Controllers.Organization
{
    using Newtonsoft.Json;

    using System.Web.Mvc;

    using Northwind.Domain.Contracts.Tasks;
    using Northwind.Web.Controllers;
    using Northwind.Web.Mvc.Controllers.Organization.ViewModels;

    using SharpArch.NHibernate.Web.Mvc;
    using SharpArch.Web.Mvc.JsonNet;

    [HandleError]
    public class EmployeeRiaController : Controller
    {
        private readonly IEmployeeTasks employeeTasks;
        private readonly ITerritoryTasks territoryTasks;

        public EmployeeRiaController(IEmployeeTasks employeeTasks, ITerritoryTasks territoryTasks)
        {
            this.employeeTasks = employeeTasks;
            this.territoryTasks = territoryTasks;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonNetResult GetEmployeeFormViewModel()
        {
            var employeeViewModel = new ViewEmployeesViewModel
            {
                Employees = this.employeeTasks.GetAllEmployees(),
                AvailableTerritories = this.territoryTasks.GetTerritories()
            };

            var serializer = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new NHibernateContractResolver(),
            };

            return new JsonNetResult
                {
                    Data = employeeViewModel,
                    SerializerSettings = serializer
                };
        }

        [Transaction]
        [HttpPost]
        public JsonNetResult Edit(CreateEmployeeViewModel createEmployeeViewModel)
        {
            this.employeeTasks.RiaCreateOrUpdate(createEmployeeViewModel.Employee, createEmployeeViewModel.TerritoriesString);

            var serializer = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new NHibernateContractResolver(),
            };

            var jsonNetResult = new JsonNetResult
            {
                Data = createEmployeeViewModel.Employee,
                SerializerSettings = serializer
            };

            return jsonNetResult;
        }

        [Transaction]
        [HttpPost]
        public JsonNetResult Delete(int id)
        {
            this.employeeTasks.Delete(id);
            var serializer = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new NHibernateContractResolver(),
            };

            return new JsonNetResult { Data = this.employeeTasks.GetAllEmployees(), SerializerSettings = serializer };
        }
    }
}