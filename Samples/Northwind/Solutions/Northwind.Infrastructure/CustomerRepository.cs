using JetBrains.Annotations;
using NHibernate;
using SharpArch.Domain.PersistenceSupport;

namespace Northwind.Infrastructure
{
    using System.Collections.Generic;

    using NHibernate.Criterion;

    using Northwind.Domain;
    using Northwind.Domain.Contracts;

    using SharpArch.NHibernate;

    public class CustomerRepository : NHibernateRepositoryWithTypedId<Customer, string>, ICustomerRepository
    {
        public List<Customer> FindByCountry(string countryName)
        {
            var criteria = this.Session.CreateCriteria(typeof(Customer)).Add(Restrictions.Eq("Country", countryName));

            return criteria.List<Customer>() as List<Customer>;
        }

        public CustomerRepository([NotNull] ITransactionManager transactionManager, [NotNull] ISession session) : base(transactionManager, session)
        {
        }
    }
}