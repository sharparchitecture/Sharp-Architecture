namespace Northwind.Domain.Contracts.Tasks
{
    using System.Collections.Generic;

    using Northwind.Domain.Organization;

    public interface IEmployeeTasks
    {
        void CreateOrUpdate(Employee employee);

        void RiaCreateOrUpdate(Employee employee, string availableTerritories);

        void Delete(int id);

        IList<Employee> GetAllEmployees();

        Employee GetEmployeeById(int id);
    }
}