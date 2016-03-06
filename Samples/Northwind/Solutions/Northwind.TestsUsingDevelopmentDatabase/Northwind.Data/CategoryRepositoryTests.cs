namespace Tests.Northwind.Data
{
    using global::Northwind.Domain;

    using NUnit.Framework;

    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate;
    using SharpArch.Testing.NUnit.NHibernate;

    [TestFixture]
    [Category("DB Tests")]
    public class CategoryRepositoryTests : DatabaseRepositoryTestsBase
    {
        private readonly IRepository<Category> categoryRepository = new NHibernateRepository<Category>();

        [Test]
        public void CanGetAllCategories()
        {
            var categories = this.categoryRepository.GetAll();

            Assert.That(categories, Is.Not.Null);
            Assert.That(categories, Is.Not.Empty);
        }

        [Test]
        public void CanGetCategoryById()
        {
            var category = this.categoryRepository.Get(1);

            Assert.That(category.CategoryName, Is.EqualTo("Beverages"));
        }
    }
}