namespace Northwind.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using NHibernate;
    using NHibernate.Criterion;

    using Northwind.Domain;
    using Northwind.Domain.Contracts;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate;

    public class CustomerRepository : NHibernateRepositoryWithTypedId<Customer, string>, ICustomerRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateRepositoryWithTypedId{T, TId}"/> class.
        /// </summary>
        /// <param name="transactionManager">The transaction manager.</param>
        /// <param name="session">The session.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public CustomerRepository(ITransactionManager transactionManager, ISession session) : base(transactionManager, session)
        {
        }

        public List<Customer> FindByCountry(string countryName)
        {
            var criteria = this.Session.CreateCriteria(typeof(Customer)).Add(Restrictions.Eq("Country", countryName));

            return criteria.List<Customer>() as List<Customer>;
        }
    }
}