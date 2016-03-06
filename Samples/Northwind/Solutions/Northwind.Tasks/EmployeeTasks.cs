using System.Linq;
using Northwind.Domain;

namespace Northwind.Tasks
{
    using System.Collections.Generic;

    using Northwind.Domain.Contracts.Tasks;
    using Northwind.Domain.Organization;

    using SharpArch.Domain;
    using SharpArch.Domain.PersistenceSupport;

    public class EmployeeTasks : IEmployeeTasks
    {
        private readonly IRepository<Employee> employeeRepository;
        private readonly IRepository<Territory> territoryRepository;

        public EmployeeTasks(IRepository<Employee> employeeRepository, IRepository<Territory> territoryRepository)
        {
            this.employeeRepository = employeeRepository;
            this.territoryRepository = territoryRepository;
        }

        public void CreateOrUpdate(Employee employee)
        {
            var employeeToUpdate = employee.Clone();
            employee.Territories.Clear();

            foreach (var territory in employeeToUpdate.Territories)
            {
                employee.Territories.Add(territory);
            }

            if (employee.IsValid())
            {
                this.employeeRepository.DbContext.BeginTransaction();
                this.employeeRepository.SaveOrUpdate(employee);
                this.employeeRepository.DbContext.CommitChanges();
            }
            else
            {
                this.employeeRepository.DbContext.RollbackTransaction();
            }
        }

        public void RiaCreateOrUpdate(Employee employee, string availableTerritories)
        {
            employee.Territories.Clear();

            foreach (var territory in availableTerritories.Split(',')) 
            {
                // Depending on how you're accepting user input, better to use NHSearch
                var hydratedTerritory =
                    this.territoryRepository.GetAll().Where(x => x.Description.Trim() == territory.Trim()).FirstOrDefault();

                if (hydratedTerritory != null) 
                {
                    employee.Territories.Add(hydratedTerritory);
                }
            }

            // Currently crashes on duplicates, Territories probably should be a hash set and not a list
            if (employee.IsValid())
            {
                this.employeeRepository.DbContext.BeginTransaction();
                this.employeeRepository.SaveOrUpdate(employee);
                this.employeeRepository.DbContext.CommitChanges();
            }
            else
            {
                this.employeeRepository.DbContext.RollbackTransaction();
            }
        }

        public void Delete(int id)
        {
            var employeeToRemove = this.employeeRepository.Get(id);

            Check.Require(employeeToRemove != null, "employee must exist.");

            if (employeeToRemove != null)
            {
                this.employeeRepository.DbContext.BeginTransaction();
                this.employeeRepository.Delete(employeeToRemove);
                this.employeeRepository.DbContext.CommitTransaction();
            }
        }

        public IList<Employee> GetAllEmployees()
        {
            this.employeeRepository.DbContext.BeginTransaction();
            var employees = this.employeeRepository.GetAll();
            this.employeeRepository.DbContext.CommitTransaction();
            return employees;
        }

        public Employee GetEmployeeById(int id)
        {
            if (id == 0)
            {
                return new Employee();
            }

            this.employeeRepository.DbContext.BeginTransaction();
            var employee = this.employeeRepository.Get(id);
            this.employeeRepository.DbContext.CommitTransaction();

            Check.Require(employee != null, "employee must exist.");
            return employee;
        }
    }
}