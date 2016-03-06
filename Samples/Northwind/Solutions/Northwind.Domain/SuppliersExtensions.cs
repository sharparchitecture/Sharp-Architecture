namespace Northwind.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///   Extends IList&lt;Supplier> with other, customer-specific collection methods.
    /// </summary>
    public static class SuppliersExtensions
    {
        public static List<Supplier> FindSuppliersCarryingFewestProducts(this IList<Supplier> suppliers)
        {
            var minProductsCount = suppliers.Min(supplier => supplier.Products.Count);
            return GetSuppliersWithProductCountOf(minProductsCount, suppliers);
        }

        public static List<Supplier> FindSuppliersCarryingMostProducts(this IList<Supplier> suppliers)
        {
            var maxProductsCount = suppliers.Max(supplier => supplier.Products.Count);
            return GetSuppliersWithProductCountOf(maxProductsCount, suppliers);
        }

        private static List<Supplier> GetSuppliersWithProductCountOf(int productsCount, IList<Supplier> suppliers)
        {
            return (from supplier in suppliers where supplier.Products.Count == productsCount select supplier).ToList();
        }
    }
}