namespace SharpArch.Web.Mvc.ModelBinder
{
    using System;
    using System.Web.Mvc;

    using Domain.PersistenceSupport;
    using JetBrains.Annotations;

    internal class GenericRepositoryFactory
    {
        /// <summary>
        /// Resolve repository for given entity type.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="idType">Type of the identifier.</param>
        /// <returns>Repository instance.</returns>
        /// <exception cref="InvalidOperationException">If repository can not be resolved by <see cref="DependencyResolver"/>.</exception>
        public static object CreateEntityRepositoryFor([NotNull] Type entityType, [NotNull] Type idType)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));
            if (idType == null) throw new ArgumentNullException(nameof(idType));

            var genericRepositoryType = typeof (IRepositoryWithTypedId<,>);
            var concreteRepositoryType = genericRepositoryType.MakeGenericType(entityType, idType);

            var repository = DependencyResolver.Current.GetService(concreteRepositoryType);
            if (repository == null)
                throw new InvalidOperationException("Can not resolve " + concreteRepositoryType);

            return repository;
        }
    }
}