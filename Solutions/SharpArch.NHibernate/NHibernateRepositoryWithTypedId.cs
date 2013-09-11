namespace SharpArch.NHibernate
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using global::NHibernate;
    using global::NHibernate.Linq;
    using global::NHibernate.Criterion;

    using SharpArch.Domain;
    using SharpArch.Domain.DomainModel;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.NHibernate.Contracts.Repositories;
    using System.Linq;

    /// <summary>
    ///     Provides a fully loaded DAO which may be created in a few ways including:
    ///     * Direct instantiation; e.g., new GenericDao<Customer, string>
    ///     * Spring configuration; e.g., <object id = "CustomerDao" type = "SharpArch.Data.NHibernateSupport.GenericDao&lt;CustomerAlias, string>, SharpArch.Data" autowire = "byName" />
    /// </summary>
    public class NHibernateRepositoryWithTypedId<T, TId> : IRepositoryWithTypedId<T, TId>
    {

        #region Properties

        protected virtual ISession Session
        {
            get
            {
                string factoryKey = SessionFactoryKeyHelper.GetKeyFrom(this);
                return NHibernateSession.CurrentFor(factoryKey);
            }
        }

        #endregion

        #region IRepository<T> Members

        public T this[TId id]
        {
            get
            {
                var entity = Session.Get<T>(id);
                return entity;
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T entity)
        {
            try
            {
                this.Session.Save(entity);
            }
            catch
            {
                if (this.Session.IsOpen)
                {
                    this.Session.Close();
                }

                throw;
            }

            this.Session.Flush();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T entity)
        {
            var query = Session.Query<T>();

            return query.Contains(entity);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var query = Session.Query<T>();
            var recordset = query.ToArray();

            Array.Copy(recordset, 0, array, arrayIndex, Count);
        }

        public int Count
        {
            get
            {
                var query = Session.Query<T>();
                return query.Count();
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T entity)
        {
            Session.Delete(entity);
            return true;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            var query = Session.Query<T>();
            return query.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IQueryable Members

        public Type ElementType
        {
            get
            {
                var query = Session.Query<T>();
                return query.ElementType;
            }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get
            {
                var query = Session.Query<T>();
                return query.Expression;
            }
        }

        public IQueryProvider Provider
        {
            get
            {
                var query = Session.Query<T>();
                return query.Provider;
            }
        }

        #endregion

    }
}