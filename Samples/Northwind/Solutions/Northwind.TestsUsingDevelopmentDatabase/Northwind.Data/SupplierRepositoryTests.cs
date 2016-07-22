namespace Tests.Northwind.Data
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using global::Northwind.Domain;
    using global::Northwind.Domain.Contracts;
    using global::Northwind.Infrastructure;
    using NHibernate;
    using NUnit.Framework;

    using SharpArch.Testing.NUnit.NHibernate;

    [TestFixture]
    [Category("DB Tests")]
    public class SupplierRepositoryTests : DatabaseRepositoryTestsBase
    {
        ISupplierRepository supplierRepository;

        /// <summary>
        /// Creates new <see cref="ISession"/>.
        /// </summary>
        public override void SetUp()
        {
            base.SetUp();
            this.supplierRepository = new SupplierRepository(TransactionManager, Session);
        }

        [Test]
        public void CanLoadSuppliersByProductCategoryName()
        {
            var matchingSuppliers = this.supplierRepository.GetSuppliersBy("Seafood");

            Assert.That(matchingSuppliers.Count, Is.EqualTo(8));

            OutputSearchResults(matchingSuppliers);
        }

        private static void OutputSearchResults(List<Supplier> matchingSuppliers)
        {
            Debug.WriteLine("SupplierRepositoryTests.CanLoadSuppliersByProductCategoryName Results:");

            foreach (var supplier in matchingSuppliers)
            {
                Debug.WriteLine("Company name: " + supplier.CompanyName);

                foreach (var product in supplier.Products)
                {
                    Debug.WriteLine(" * Product name: " + product.ProductName);
                    Debug.WriteLine(" * Category name: " + product.Category.CategoryName);
                }
            }
        }
    }
}