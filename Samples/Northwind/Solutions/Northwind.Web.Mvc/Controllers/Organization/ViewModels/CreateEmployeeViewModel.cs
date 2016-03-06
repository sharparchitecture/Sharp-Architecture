namespace Northwind.Web.Mvc.Controllers.Organization.ViewModels
{
    using Northwind.Domain.Organization;

    public class CreateEmployeeViewModel
    {
        public Employee Employee { get; set; }

        public string TerritoriesString { get; set; }
    }
}

