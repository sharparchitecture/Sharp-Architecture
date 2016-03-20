namespace Northwind.Tasks
{
    using System.Collections.Generic;

    using Northwind.Domain;
    using Northwind.Domain.Contracts;
    using Northwind.Domain.Contracts.Tasks;

    using SharpArch.Domain;

    public class CustomerTasks : ICustomerTasks
    {
        private readonly ICustomerRepository customerRepository;

        public CustomerTasks(ICustomerRepository customerRepository)
        {
            this.customerRepository = customerRepository;
        }

        public Customer Create(string companyName, string assignedId)
        {
            var customerToCreate = new Customer(companyName);
            customerToCreate.SetAssignedIdTo(assignedId);

            this.customerRepository.DbContext.BeginTransaction();
            this.customerRepository.SaveOrUpdate(customerToCreate);
            this.customerRepository.DbContext.CommitTransaction();

            return customerToCreate;
        }

        public List<Customer> GetCustomersByCountry(string country)
        {
            Check.Require(country != null, "country must be provided.");
            var customer = this.customerRepository.FindByCountry(country);
            return customer;
        }
    }
}