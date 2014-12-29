namespace SharpArch.RavenDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Raven.Abstractions.Commands;
    using Raven.Client;

    using SharpArch.Domain.DomainModel;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.Domain.Specifications;
    using SharpArch.RavenDb.Contracts.Repositories;

    public class RavenDbRepositoryWithTypedId<T, TIdT> : IRavenDbRepositoryWithTypedId<T, TIdT>,
        ILinqRepositoryWithTypedId<T, TIdT>
    {
        private readonly IDocumentSession session;

        private readonly IDbContext context;

        public RavenDbRepositoryWithTypedId(IDocumentSession session)
        {
            this.session = session;
            this.context = new DbContext(session);
        }

        public IDocumentSession Session
        {
            get
            {
                return this.session;
            }
        }

        public virtual IDbContext DbContext
        {
            get
            {
                return this.context;
            }
        }

        public IEnumerable<T> FindAll(Func<T, bool> where)
        {
            return this.FindAll().Where(where);
        }

        public T FindOne(Func<T, bool> where)
        {
            IEnumerable<T> foundList = this.FindAll(where);

            try
            {
                return foundList.SingleOrDefault();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("The query returned more than one result. Please refine your query.", ex);
            }
        }

        public T First(Func<T, bool> where)
        {
            return this.FindAll(where).First(where);
        }

        public void Delete(T entity)
        {
            this.session.Delete(entity);
        }

        public void Delete(TIdT id)
        {
            if (id is ValueType)
            {
                this.Delete(this.Get(id));
            }
            else
            {
                this.session.Advanced.Defer(new DeleteCommandData { Key = id.ToString() });
            }
        }

        public void Save(T entity)
        {
            this.SaveOrUpdate(entity);
        }

        public void SaveAndEvict(T entity)
        {
            this.SaveOrUpdate(entity);
            this.session.Advanced.Evict(entity);
        }

        public T FindOne(TIdT id)
        {
            return this.Get(id);
        }

        public T FindOne(ILinqSpecification<T> specification)
        {
            return specification.SatisfyingElementsFrom(this.FindAll()).SingleOrDefault();
        }

        public IQueryable<T> FindAll()
        {
            return this.Session.Query<T>();
        }

        public IQueryable<T> FindAll(ILinqSpecification<T> specification)
        {
            return specification.SatisfyingElementsFrom(this.FindAll());
        }

        public T Get(TIdT id)
        {
            if (id is ValueType)
            {
                return this.session.Load<T>(id as ValueType);
            }

            return this.session.Load<T>(id.ToString());
        }

        public IList<T> GetAll()
        {
            return this.FindAll().ToList();
        }

        public IList<T> GetAll(IEnumerable<TIdT> ids)
        {
            return this.session.Load<T>(ids.Select(p => p.ToString()));
        }

        public T SaveOrUpdate(T entity)
        {
            this.session.Store(entity);
            return entity;
        }
    }
}