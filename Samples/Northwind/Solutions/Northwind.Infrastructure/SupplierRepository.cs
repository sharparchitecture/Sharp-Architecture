namespace Northwind.Infrastructure
{
    using System.Collections.Generic;

    using NHibernate.Criterion;
    using NHibernate.Transform;

    using Northwind.Domain;
    using Northwind.Domain.Contracts;

    using SharpArch.NHibernate;

    public class SupplierRepository : NHibernateRepository<Supplier>, ISupplierRepository
    {
        /// <summary>
        ///   Uses NHibernate's CreateAlias to create a join query from the <see cref = "Supplier" />
        ///   to its collection of <see cref = "Product" /> items to the category in which each
        ///   product belongs.  This
        /// </summary>
        /// <remarks>
        ///   Note that a category alias would not be necessary if we were trying to match the category ID.
        /// </remarks>
        public List<Supplier> GetSuppliersBy(string productCategoryName)
        {
            var criteria =
                this.Session.CreateCriteria(typeof(Supplier)).CreateAlias("Products", "product").CreateAlias(
                    "product.Category", "productCategory").Add(
                        Restrictions.Eq("productCategory.CategoryName", productCategoryName)).SetResultTransformer(
                            new DistinctRootEntityResultTransformer());

            return criteria.List<Supplier>() as List<Supplier>;
        }
    }
}