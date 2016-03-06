namespace Northwind.Domain.Contracts
{
    using System.Collections.Generic;

    using SharpArch.Domain.PersistenceSupport;

    public interface ISupplierRepository : IRepository<Supplier>
    {
        List<Supplier> GetSuppliersBy(string productCategoryName);
    }
}