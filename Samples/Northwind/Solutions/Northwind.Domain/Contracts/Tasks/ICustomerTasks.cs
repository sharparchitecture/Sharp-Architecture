namespace Northwind.Domain.Contracts.Tasks
{
    using System.Collections.Generic;

    public interface ICustomerTasks
    {
        Customer Create(string customerName, string assignedId);

        List<Customer> GetCustomersByCountry(string country);
    }
}