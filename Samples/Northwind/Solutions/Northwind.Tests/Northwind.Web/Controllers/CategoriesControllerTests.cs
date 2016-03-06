using NUnit.Framework;
using MvcContrib.TestHelper;
using Northwind.Web.Controllers;
using SharpArch.Core.PersistenceSupport;
using Northwind.Core;
using Rhino.Mocks;
using System.Web.Mvc;
using System.Collections.Generic;
using SharpArch.Testing;
using SharpArch.Testing.NUnit;

namespace Tests.Northwind.Web.Controllers
{
    [TestFixture]
    public class CategoriesControllerTests
    {
        [Test]
        public void CanListCategories() {
            CategoriesController controller = 
                new CategoriesController(CreateMockCategoryRepository());

            ViewResult result = 
                controller.Index().AssertViewRendered().ForView("");

            result.ViewData.ShouldNotBeNull();
            (result.ViewData.Model as List<Category>).ShouldNotBeNull();
            (result.ViewData.Model as List<Category>).Count.ShouldEqual(3);
        }

        [Test]
        public void CanCreateCategory() {
            CategoriesController controller =
                new CategoriesController(CreateMockCategoryRepository());

            ViewResult result =
                controller.Create("Hawaiian").AssertViewRendered().ForView("");

            result.ViewData.ShouldNotBeNull();
            (result.ViewData.Model as Category).ShouldNotBeNull();
            (result.ViewData.Model as Category).Id.ShouldBeGreaterThan(0);
        }

        [Test]
        public void CanDetailCategory() {
            CategoriesController controller = 
                new CategoriesController(CreateMockCategoryRepository());

            ViewResult result = 
                controller.Show(1).AssertViewRendered().ForView("");

            // The builder object acts as a wrapper around the controller, 
            // so be sure to interrogate it instead of the controller
            result.ViewData.ShouldNotBeNull();
            (result.ViewData.Model as Category).ShouldNotBeNull();
            (result.ViewData.Model as Category).Id.ShouldEqual(1);
        }

        private IRepository<Category> CreateMockCategoryRepository() {
            IRepository<Category> repository = MockRepository.GenerateMock<IRepository<Category>>( );
            repository.Expect(r => r.GetAll()).Return(CreateCategories());
            repository.Expect(r => r.Get(1)).IgnoreArguments().Return(CreateCategory());
            repository.Expect(r => r.SaveOrUpdate(null)).IgnoreArguments().Return(CreateCategory());

            return repository;
        }

        private Category CreateCategory() {
            Category category = new Category("Hawaiian");
            EntityIdSetter.SetIdOf(category, 1);
            return category;
        }

        private List<Category> CreateCategories() {
            List<Category> categories = new List<Category>();

            categories.Add(new Category("Blue"));
            categories.Add(new Category("Green"));
            categories.Add(new Category("Yellow"));

            return categories;
        }
    }
}
