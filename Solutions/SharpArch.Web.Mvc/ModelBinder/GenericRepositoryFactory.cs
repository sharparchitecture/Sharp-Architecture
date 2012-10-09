namespace SharpArch.Web.Mvc.ModelBinder
{
    using System;
    using System.Web.Mvc;

    using SharpArch.Domain.PersistenceSupport;

    internal class GenericRepositoryFactory
    {
        public static object CreateEntityRepositoryFor(Type entityType, Type idType)
        {
            var genericRepositoryType = typeof(IRepositoryWithTypedId<,>);
            var concreteRepositoryType = genericRepositoryType.MakeGenericType(new[] { entityType, idType });

            object repository;

            try
            {
                repository = DependencyResolver.Current.GetService(concreteRepositoryType);
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException(
                    "ServiceLocator has not been initialized; " + "I was trying to retrieve " + concreteRepositoryType);
            }

            return repository;
        }
    }
}