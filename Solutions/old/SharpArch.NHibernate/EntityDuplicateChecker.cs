namespace SharpArch.NHibernate
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Domain.DomainModel;
    using Domain.PersistenceSupport;

    using global::NHibernate;
    using global::NHibernate.Criterion;
    using global::NHibernate.Util;
    using JetBrains.Annotations;

    /// <summary>
    /// Checks if entity with the same domain signature already exists in the database.
    /// </summary>
    /// <seealso cref="SharpArch.Domain.PersistenceSupport.IEntityDuplicateChecker" />
    /// <seealso cref="DomainSignatureAttribute"/>.
    [PublicAPI]
    public class EntityDuplicateChecker : IEntityDuplicateChecker
    {
        private readonly ISession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDuplicateChecker"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public EntityDuplicateChecker([NotNull] ISession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            this.session = session;
        }

        private static readonly DateTime uninitializedDatetime = default(DateTime);

        /// <summary>
        /// Provides a behavior specific repository for checking if a duplicate exists of an existing entity.
        /// </summary>
        /// <typeparam name="TId">Entity Id type.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if a duplicate exists, <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">entity is null. </exception>
        public bool DoesDuplicateExistWithTypedIdOf<TId>(IEntityWithTypedId<TId> entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var sessionForEntity = GetSessionFor(entity);

            var previousFlushMode = sessionForEntity.FlushMode;

            // We do NOT want this to flush pending changes as checking for a duplicate should 
            // only compare the object against data that's already in the database
            sessionForEntity.FlushMode = FlushMode.Never;
            try
            {
                var criteria =
                    sessionForEntity.CreateCriteria(entity.GetType())
                        .Add(Restrictions.Not(Restrictions.Eq("Id", entity.Id)))
                        .SetMaxResults(1);

                AppendSignaturePropertyCriteriaTo(criteria, entity);
                var doesDuplicateExist = criteria.List().Any();
                return doesDuplicateExist;
            }
            finally
            {
                sessionForEntity.FlushMode = previousFlushMode;    
            }
        }

        private static void AppendEntityCriteriaTo<TId>(
            ICriteria criteria, string propertyName, object propertyValue)
        {
            criteria.Add(
                propertyValue != null
                    ? Restrictions.Eq(propertyName + ".Id", ((IEntityWithTypedId<TId>)propertyValue).Id)
                    : Restrictions.IsNull(propertyName + ".Id"));
        }

        private static void AppendStringPropertyCriteriaTo(
            ICriteria criteria, string propertyName, object propertyValue)
        {
            criteria.Add(
                propertyValue != null
                    ? Restrictions.InsensitiveLike(propertyName, propertyValue.ToString(), MatchMode.Exact)
                    : Restrictions.IsNull(propertyName));
        }

        private static void AppendValuePropertyCriteriaTo(
            ICriteria criteria, string propertyName, object propertyValue)
        {
            criteria.Add(
                propertyValue != null
                    ? Restrictions.Eq(propertyName, propertyValue)
                    : Restrictions.IsNull(propertyName));
        }

        [NotNull]
        private ISession GetSessionFor(object entity)
        {
            return this.session;
        }

        private static string GetPropertyName(string parentPropertyName, PropertyInfo signatureProperty)
        {
            if (string.IsNullOrEmpty(parentPropertyName))
            {
                return signatureProperty.Name;
            }

            if (parentPropertyName.EndsWith(".") == false)
            {
                parentPropertyName += ".";
            }

            return string.Concat(parentPropertyName, signatureProperty.Name);
        }

        private static void AppendDateTimePropertyCriteriaTo(ICriteria criteria, string propertyName, object propertyValue)
        {
            criteria.Add(
                (DateTime)propertyValue > uninitializedDatetime
                    ? Restrictions.Eq(propertyName, propertyValue)
                    : Restrictions.IsNull(propertyName));
        }

        private static void AppendSignaturePropertyCriteriaTo<TId>(ICriteria criteria, IEntityWithTypedId<TId> entity)
        {
            foreach (var signatureProperty in entity.GetSignatureProperties())
            {
                var propertyType = signatureProperty.PropertyType;
                var propertyValue = signatureProperty.GetValue(entity, null);
                var propertyName = signatureProperty.Name;
                
                if (propertyType.GetInterfaces().Any(
                       x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntityWithTypedId<>)))
                {
                    AppendEntityCriteriaTo<TId>(criteria, propertyName, propertyValue);
                }
                else if (typeof(ValueObject).IsAssignableFrom(propertyType))
                {
                    AppendValueObjectSignaturePropertyCriteriaTo(criteria, entity.GetType(), propertyName, propertyValue as ValueObject);
                }
                else
                {
                    AppendSimplePropertyCriteriaTo(criteria, entity.GetType(), propertyValue, propertyType, propertyName);
                }
            }
        }

        private static void AppendValueObjectSignaturePropertyCriteriaTo(ICriteria criteria, Type entityType, string valueObjectPropertyName, ValueObject valueObject)
        {
            if (valueObject == null)
            {
                return;
            }

            foreach (PropertyInfo signatureProperty in valueObject.GetSignatureProperties())
            {
                Type propertyType = signatureProperty.PropertyType;
                object propertyValue = signatureProperty.GetValue(valueObject, null);
                string propertyName = GetPropertyName(valueObjectPropertyName, signatureProperty);

                AppendSimplePropertyCriteriaTo(criteria, entityType, propertyValue, propertyType, propertyName);
            }
        }

        private static void AppendSimplePropertyCriteriaTo(
            ICriteria criteria, Type entityType, object propertyValue, Type propertyType, string propertyName)
        {
            if (propertyType.IsEnum)
            {
                criteria.Add(Restrictions.Eq(propertyName, (int)propertyValue));
            }
            else if (propertyType == typeof(DateTime))
            {
                AppendDateTimePropertyCriteriaTo(criteria, propertyName, propertyValue);
            }
            else if (propertyType == typeof(string))
            {
                AppendStringPropertyCriteriaTo(criteria, propertyName, propertyValue);
            }
            else if (propertyType.IsValueType)
            {
                AppendValuePropertyCriteriaTo(criteria, propertyName, propertyValue);
            }
            else
            {
                throw new ApplicationException(
                    "Can't determine how to use " + entityType + "." + propertyName
                    + " when looking for duplicate entries. To remedy this, "
                    + "you can create a custom validator or report an issue to the S#arp Architecture "
                    + "project, detailing the type that you'd like to be accommodated.");
            }
        }
    }
}