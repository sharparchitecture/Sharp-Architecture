using System;
using MvcContrib.TestHelper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpArch.Core.PersistenceSupport;
using SharpArch.Testing;
using SharpArch.Testing.NUnit;
using System.Collections.Generic;
using System.Web.Mvc;
using Northwind.Core.Organization;
using Northwind.Web.Controllers;
using Northwind.Web.Controllers.Organization;
using Castle.Windsor;
using SharpArch.Core.NHibernateValidator.CommonValidatorAdapter;
using SharpArch.Core.CommonValidator;
using CommonServiceLocator.WindsorAdapter;
using Microsoft.Practices.ServiceLocation;
using SharpArch.Core.DomainModel;
using SharpArch.Core;
using Tests.Northwind.Data.TestDoubles; 

namespace Tests.Northwind.Web.Controllers.Organization
{
    [TestFixture]
    public class EmployeesControllerTests
    {
        [SetUp]
        public void SetUp() {
            // By default, and typically, we'd simply use the CRUD scaffolding generated call to ServiceLocatorInitializer.Init();
            // but since we need a custom duplicate checker, we'll do it locally
            InitServiceLocator();

            controller = new EmployeesController(
                    MockEmployeeRepositoryFactory.CreateMockEmployeeRepository(),
                    MockTerritoryRepositoryFactory.CreateMockTerritoryRepository()
                );
        }

        public void InitServiceLocator() {
            IWindsorContainer container = new WindsorContainer();
            container.AddComponent("duplicateChecker",
                typeof(IEntityDuplicateChecker), typeof(DuplicateCheckerStub));
            container.AddComponent("validator", typeof(IValidator), typeof(Validator));
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
        }

        /// <summary>
        /// Add a couple of objects to the list within CreateEmployees and change the 
        /// "ShouldEqual(0)" within this test to the respective number.
        /// </summary>
        [Test]
        public void CanListEmployees() {
            ViewResult result = controller.Index().AssertViewRendered();

            result.ViewData.Model.ShouldNotBeNull();
            (result.ViewData.Model as List<Employee>).Count.ShouldEqual(0);
        }

        [Test]
        public void CanShowEmployee() {
            ViewResult result = controller.Show(1).AssertViewRendered();

			result.ViewData.ShouldNotBeNull();
			
            (result.ViewData.Model as Employee).Id.ShouldEqual(1);
        }

        [Test]
        public void CanInitEmployeeCreation() {
            ViewResult result = controller.Create().AssertViewRendered();
            
            result.ViewData.Model.ShouldNotBeNull();
            result.ViewData.Model.ShouldBeOfType(typeof(EmployeesController.EmployeeFormViewModel));
            (result.ViewData.Model as EmployeesController.EmployeeFormViewModel).Employee.ShouldBeNull();
        }

        [Test]
        public void CanEnsureEmployeeCreationIsValid() {
            Employee employeeFromForm = new Employee();
            ViewResult result = controller.Create(employeeFromForm).AssertViewRendered();

            result.ViewData.Model.ShouldNotBeNull();
            result.ViewData.Model.ShouldBeOfType(typeof(EmployeesController.EmployeeFormViewModel));
        }

        [Test]
        public void CanCreateEmployee() {
            Employee employeeFromForm = MockEmployeeRepositoryFactory.CreateTransientEmployee();
            RedirectToRouteResult redirectResult = controller.Create(employeeFromForm)
                .AssertActionRedirect().ToAction("Index");
            controller.TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()].ToString()
				.ShouldContain("was successfully created");
        }

        [Test]
        public void CanUpdateEmployee() {
            Employee employeeFromForm = MockEmployeeRepositoryFactory.CreateTransientEmployee();
            EntityIdSetter.SetIdOf<int>(employeeFromForm, 1);
            RedirectToRouteResult redirectResult = controller.Edit(employeeFromForm)
                .AssertActionRedirect().ToAction("Index");
            controller.TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()].ToString()
				.ShouldContain("was successfully updated");
        }

        [Test]
        public void CanInitEmployeeEdit() {
            ViewResult result = controller.Edit(1).AssertViewRendered();

			result.ViewData.Model.ShouldNotBeNull();
            result.ViewData.Model.ShouldBeOfType(typeof(EmployeesController.EmployeeFormViewModel));
            (result.ViewData.Model as EmployeesController.EmployeeFormViewModel).Employee.Id.ShouldEqual(1);
        }

        [Test]
        public void CanDeleteEmployee() {
            RedirectToRouteResult redirectResult = controller.Delete(1)
                .AssertActionRedirect().ToAction("Index");
            
            controller.TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()].ToString()
				.ShouldContain("was successfully deleted");
        }

        private EmployeesController controller;

        private class DuplicateCheckerStub : IEntityDuplicateChecker
        {
            public bool DoesDuplicateExistWithTypedIdOf<IdT>(IEntityWithTypedId<IdT> entity) {
                Check.Require(entity != null);

                if (entity as Employee != null) {
                    Employee employee = entity as Employee;
                    return employee.FirstName == "Billy" && employee.FirstName == "McCafferty";
                }

                // By default, simply return false for no duplicates found
                return false;
            }
        }
    }
}
