namespace Northwind.Domain.Contracts
{
    using System.Collections.Generic;

    using SharpArch.Domain.PersistenceSupport;

    /// <summary>
    ///   Needs to implement INHibernateRepositoryWithTypedId because it has an assigned Id
    ///   and will need to be explicit about called Save or Update appropriately.  Assigned
    ///   Ids are EVil with a capital E and V...yes, they're just that evil.
    /// </summary>
    public interface ICustomerRepository : IRepositoryWithTypedId<Customer, string>
    {
        List<Customer> FindByCountry(string countryName);
    }
}