namespace Tests.SharpArch.Web.Mvc.ModelBinder
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Web.Mvc;
    
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using CommonServiceLocator.WindsorAdapter;

    using Moq;

    using global::SharpArch.Domain.DomainModel;
    using global::SharpArch.Domain.PersistenceSupport;
    using global::SharpArch.Web.Mvc.ModelBinder;

    using Microsoft.Practices.ServiceLocation;


    using NUnit.Framework;

    [TestFixture]
    public class SharpModelBinderTests
    {
        [Test]
        public void CanBindModelWithCollection()
        {
            var id = 2;
            var employeeName = "Michael";

            // Arrange
            var formCollection = new NameValueCollection
                {
                    { "Employee.Id", id.ToString() }, 
                    { "Employee.Name", employeeName }, 
                    { "Employee.Reports", "3" }, 
                    { "Employee.Reports", "4" }, 
                    { "Employee.Manager", "12" }
                };

            var valueProvider = new NameValueCollectionValueProvider(formCollection, null);
            var modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Employee));

            var bindingContext = new ModelBindingContext
                {
                   ModelName = "Employee", ValueProvider = valueProvider, ModelMetadata = modelMetadata 
                };

            DefaultModelBinder target = new SharpModelBinder();

            var controllerContext = new ControllerContext();

            // Act
            var result = (Employee)target.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(employeeName, result.Name);
            Assert.AreEqual(2, result.Reports.Count);
        }

        [Test]
        public void CanBindModelWithEntityCollection()
        {
            var id = 2;
            var employeeName = "Michael";

            // Arrange
            var formCollection = new NameValueCollection
                {
                    { "Employee.Id", id.ToString() }, 
                    { "Employee.Name", employeeName }, 
                    { "Employee.Reports[0].Name", "Michael" }, 
                    { "Employee.Reports[1].Name", "Alec" }, 
                    { "Employee.Manager", "12" }
                };

            var valueProvider = new NameValueCollectionValueProvider(formCollection, null);
            var modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Employee));

            var bindingContext = new ModelBindingContext
                {
                   ModelName = "Employee", ValueProvider = valueProvider, ModelMetadata = modelMetadata 
                };

            DefaultModelBinder target = new SharpModelBinder();

            var controllerContext = new ControllerContext();

            // Act
            var result = (Employee)target.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(employeeName, result.Name);
            Assert.GreaterOrEqual(result.Reports.Count, 2);
        }

        [Test]
        public void CanBindModelWithNestedEntities()
        {
            var id = 2;
            var employeeName = "Michael";
            var managerName = "Tobias";
            var managerManagerName = "Scott";

            // Arrange
            var formCollection = new NameValueCollection
                {
                    { "Employee.Id", id.ToString() }, 
                    { "Employee.Name", employeeName }, 
                    { "Employee.Manager.Name", managerName }, 
                    { "Employee.Manager.Manager.Name", managerManagerName }
                };

            var valueProvider = new NameValueCollectionValueProvider(formCollection, null);
            var modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Employee));

            var bindingContext = new ModelBindingContext
                {
                   ModelName = "Employee", ValueProvider = valueProvider, ModelMetadata = modelMetadata 
                };

            DefaultModelBinder target = new SharpModelBinder();

            var controllerContext = new ControllerContext();

            // Act
            var result = (Employee)target.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(employeeName, result.Name);
            Assert.AreEqual(managerName, result.Manager.Name);
            Assert.AreEqual(managerManagerName, result.Manager.Manager.Name);
        }

        [Test]
        public void CanBindSimpleModel()
        {
            var id = 2;
            var employeeName = "Michael";

            // Arrange
            var formCollection = new NameValueCollection
                {
                   { "Employee.Id", id.ToString() }, { "Employee.Name", employeeName }, 
                };

            var valueProvider = new NameValueCollectionValueProvider(formCollection, null);
            var modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Employee));

            var bindingContext = new ModelBindingContext
                {
                   ModelName = "Employee", ValueProvider = valueProvider, ModelMetadata = modelMetadata 
                };

            DefaultModelBinder target = new SharpModelBinder();

            var controllerContext = new ControllerContext();

            // Act
            var result = (Employee)target.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(employeeName, result.Name);
        }

        [Test]
        public void CanBindSimpleModelWithGuidId()
        {
            var id = new Guid();
            var territoryName = "Someplace, USA";

            // Arrange
            var formCollection = new NameValueCollection
                {
                   { "Territory.Id", id.ToString() }, { "Territory.Name", territoryName }, 
                };

            var valueProvider = new NameValueCollectionValueProvider(formCollection, null);
            var modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Territory));

            var bindingContext = new ModelBindingContext
                {
                   ModelName = "Territory", ValueProvider = valueProvider, ModelMetadata = modelMetadata 
                };

            DefaultModelBinder target = new SharpModelBinder();

            var controllerContext = new ControllerContext();

            // Act
            var result = (Territory)target.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(territoryName, result.Name);
        }

        [Test]
        public void CanBindSimpleModelWithGuidIdAndNullValue()
        {
            var territoryName = "Someplace, USA";

            // Arrange
            var formCollection = new NameValueCollection
                {
                   { "Territory.Id", string.Empty }, { "Territory.Name", territoryName }, 
                };

            var valueProvider = new NameValueCollectionValueProvider(formCollection, null);
            var modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Territory));

            var bindingContext = new ModelBindingContext
                {
                   ModelName = "Territory", ValueProvider = valueProvider, ModelMetadata = modelMetadata 
                };

            DefaultModelBinder target = new SharpModelBinder();

            var controllerContext = new ControllerContext();

            // Act
            var result = (Territory)target.BindModel(controllerContext, bindingContext);

            // Assert
            Assert.AreEqual(territoryName, result.Name);
        }

        [TestFixtureSetUp]
        public void SetUp()
        {
            var mockRepository = new Mock<IRepositoryWithTypedId<Employee, int>>();
            var windsorContainer = new WindsorContainer();

            mockRepository.Setup(r => r.Get(It.IsAny<int>())).Returns((int newId) => new Employee(newId));

            windsorContainer.Register(
                Component
                    .For<IRepositoryWithTypedId<Employee, int>>()
                    .Instance(mockRepository.Object));

            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(windsorContainer));
        }

        public class Employee : Entity
        {
            public Employee()
            {
                this.Reports = new List<Employee>();
            }

            public Employee(int id)
            {
                this.Id = id;
            }

            public Employee Manager { get; set; }

            public string Name { get; set; }

            public IList<Employee> Reports { get; protected set; }
        }

        public class Territory : EntityWithTypedId<Guid>
        {
            public string Name { get; set; }
        }
    }
}