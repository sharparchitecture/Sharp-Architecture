using Northwind.Web.Controllers;
using System.Web.Mvc;
using System.Collections.Generic;
using NUnit.Framework;
using MvcContrib.TestHelper;
using Northwind.Core;
using Rhino.Mocks;
using Northwind.Core.DataInterfaces;
using SharpArch.Testing;
using SharpArch.Testing.NUnit;

namespace Tests.Northwind.Web.Controllers
{
    [TestFixture]
    public class CustomersControllerTests
    {
        [Test]
        public void CanListCustomers() {
            CustomersController controller = 
                new CustomersController(CreateMockCustomerRepository());

            ViewResult result = 
                controller.Index().AssertViewRendered().ForView("");

            result.ViewData.ShouldNotBeNull();
            (result.ViewData.Model as List<Customer>).ShouldNotBeNull();
            (result.ViewData.Model as List<Customer>).Count.ShouldEqual(2);
        }

        public ICustomerRepository CreateMockCustomerRepository() {
            ICustomerRepository repository = MockRepository.GenerateMock<ICustomerRepository>( );
            repository.Expect(r => r.FindByCountry(null)).IgnoreArguments().Return(CreateCustomers());
            return repository;
        }

        private List<Customer> CreateCustomers() {
            List<Customer> customers = new List<Customer>();

            Customer customer = new Customer("Acme Anvil");
            EntityIdSetter.SetIdOf(customer, "abcde");
            customers.Add(new Customer("Acme Anvil"));
            customers.Add(new Customer("Road Runner Industries"));

            return customers;
        }
    }
}
