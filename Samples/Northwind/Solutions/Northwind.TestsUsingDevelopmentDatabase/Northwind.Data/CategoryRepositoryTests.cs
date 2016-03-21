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
        [Test]
        public void CanGetAllCategories()
        {
            IRepository<Category> categoryRepository = new NHibernateRepository<Category>(new TransactionManager(Session), Session);

            var categories = categoryRepository.GetAll();

            Assert.That(categories, Is.Not.Null);
            Assert.That(categories, Is.Not.Empty);
        }

        [Test]
        public void CanGetCategoryById()
        {
            IRepository<Category> categoryRepository = new NHibernateRepository<Category>(new TransactionManager(Session), Session);
            
            var category = categoryRepository.Get(1);

            Assert.That(category.CategoryName, Is.EqualTo("Beverages"));
        }
    }
}