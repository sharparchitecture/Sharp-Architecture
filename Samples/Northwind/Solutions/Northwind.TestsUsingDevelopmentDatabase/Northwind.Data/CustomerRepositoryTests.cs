namespace Tests.Northwind.Data
{
    using System;
    using System.Collections.Generic;

    using global::Northwind.Domain;
    using global::Northwind.Domain.Contracts;
    using global::Northwind.Infrastructure;

    using NUnit.Framework;

    using SharpArch.Domain;
    using SharpArch.Testing.NUnit.NHibernate;

    [TestFixture]
    [Category("DB Tests")]
    public class CustomerRepositoryTests : DatabaseRepositoryTestsBase
    {
        private readonly CustomerRepository customerRepository = new CustomerRepository();

        [Test]
        public void CanFindCustomerOrdersViaCustomFilter()
        {
            var customer = this.GetCustomerById();

            Assert.That(customer.Orders.FindOrdersPlacedOn(new DateTime(1998, 1, 13)).Count, Is.EqualTo(1));
            Assert.That(customer.Orders.FindOrdersPlacedOn(new DateTime(1992, 10, 13)), Is.Empty);
        }

        [Test]
        public void CanGetAllCustomers()
        {
            var customers = this.customerRepository.GetAll();

            Assert.That(customers, Is.Not.Null);
            Assert.That(customers, Is.Not.Empty);
        }

        [Test]
        public void CanGetCustomerById()
        {
            var customer = this.GetCustomerById();

            Assert.That(customer.CompanyName, Is.EqualTo("Rancho grande"));
        }

        [Test]
        public void CanGetCustomerByProperties()
        {
            IDictionary<string, object> propertyValues = new Dictionary<string, object>();
            propertyValues.Add("CompanyName", "Rancho grande");
            var customer = this.customerRepository.FindOne(propertyValues);

            Assert.That(customer, Is.Not.Null);
            Assert.That(customer.CompanyName, Is.EqualTo("Rancho grande"));

            propertyValues.Add("ContactName", "Won't Match");
            customer = this.customerRepository.FindOne(propertyValues);

            Assert.That(customer, Is.Null);
        }

        /// <summary>
        ///   It would be more effecient to use an example object or a more direct means to find the customers
        ///   within a particular country, but this is a good demonstration of how the Repository extensions work.
        /// </summary>
        [Test]
        public void CanGetCustomersByCountry()
        {
            var customers = this.customerRepository.FindByCountry("Brazil");

            Assert.That(customers, Is.Not.Null);
            Assert.That(customers.Count, Is.EqualTo(9));
        }

        /// <summary>
        ///   This test demonstrates that the orders collection is lazily loaded.  Since lazilly loaded
        ///   collections depend on a persisted session, this method is wrapped in a rolled-back transaction 
        ///   managed via the superclass.
        /// </summary>
        [Test]
        public void CanLoadCustomerOrders()
        {
            var customer = this.GetCustomerById();

            Assert.That(customer.Orders.Count, Is.EqualTo(5));
        }

        [Test]
        public void CanNotSaveOrUpdateEntityWithAssignedId()
        {
            var customer = this.GetCustomerById();

            Assert.Throws<PreconditionException>(() => this.customerRepository.SaveOrUpdate(customer));
        }

        private Customer GetCustomerById()
        {
            var customer = this.customerRepository.Load("RANCH", Enums.LockMode.Read);
            Assert.That(customer, Is.Not.Null);
            return customer;
        }
    }
}