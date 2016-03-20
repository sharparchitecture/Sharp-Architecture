namespace Northwind.Web.Mvc.Controllers.Organization.ViewModels
{
    using System.Collections.Generic;

    using Northwind.Domain;
    using Northwind.Domain.Organization;

    public class ViewEmployeesViewModel
    {
        public IList<Territory> AvailableTerritories { get; internal set; }

        public IList<Employee> Employees { get; internal set; }
    }
}