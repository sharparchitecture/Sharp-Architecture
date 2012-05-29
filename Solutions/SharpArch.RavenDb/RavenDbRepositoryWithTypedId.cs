namespace SharpArch.RavenDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Raven.Abstractions.Commands;
    using Raven.Client;

    using SharpArch.Domain.DomainModel;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.RavenDb.Contracts.Repositories;

    public class RavenDbRepositoryWithTypedId<T, TIdT> : IRavenDbRepositoryWithTypedId<T, TIdT> where T : EntityWithTypedId<TIdT>
    {
        #region Constants and Fields

        private readonly IDocumentSession context;

        #endregion

        #region Constructors and Destructors

        public RavenDbRepositoryWithTypedId(IDocumentSession context)
        {
            this.context = context;
        }

        #endregion

        #region Implemented Interfaces

        #region IRavenDbRepositoryWithTypedId<T,TIdT>

        public IEnumerable<T> FindAll(Func<T, bool> where, bool waitForNonStaleResults = false)
        {
            return this.context.Query<T>().Customize(q => CustomizeQuery(q, waitForNonStaleResults)).Where(where);
        }

        public T FindOne(Func<T, bool> where, bool waitForNonStaleResults = false)
        {
            IEnumerable<T> foundList = this.FindAll(where, waitForNonStaleResults);
            try
            {
                return foundList.SingleOrDefault();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("The query returned more than one result. Please refine your query.");
            }
        }

        public T First(Func<T, bool> where, bool waitForNonStaleResults = false)
        {
            return this.FindAll(where, waitForNonStaleResults).First(where);
        }

        #endregion

        #region IRepositoryWithTypedId<T,TIdT>

        protected IDocumentSession Session
        {
            get
            {
                return this.context;
            }
        }

        public virtual IDbContext DbContext 
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Delete(T entity)
        {
            this.context.Delete(entity);
            this.context.SaveChanges();
        }

        public void Delete(TIdT id)
        {
            this.context.Advanced.Defer(new DeleteCommandData { Key = id.ToString() });
        }

        public T Get(TIdT id)
        {
            return this.context.Load<T>(id.ToString());
        }

        public IList<T> GetAll()
        {
            return this.context.Query<T>().ToList();
        }

        public IList<T> GetAll(IEnumerable<TIdT> ids)
        {
            return this.context.Load<T>(ids.Select(p => p.ToString()));
        }

        public T SaveOrUpdate(T entity)
        {
            this.context.Store(entity);
            this.context.SaveChanges();

            return entity;
        }

        #endregion

        #endregion

        private static void CustomizeQuery(IDocumentQueryCustomization p, bool waitForNonStaleResults)
        {
            if (waitForNonStaleResults)
            {
                p.WaitForNonStaleResults();
            }
        }
    }
}