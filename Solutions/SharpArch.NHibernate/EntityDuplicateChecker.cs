namespace SharpArch.NHibernate
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Domain;
    using Domain.DomainModel;
    using Domain.PersistenceSupport;

    using global::NHibernate;
    using global::NHibernate.Criterion;

    public class EntityDuplicateChecker : IEntityDuplicateChecker
    {
        private static readonly DateTime UninitializedDatetime = default(DateTime);

        /// <summary>
        ///     Provides a behavior specific repository for checking if a duplicate exists of an existing entity.
        /// </summary>
        public bool DoesDuplicateExistWithTypedIdOf<TId>(IEntityWithTypedId<TId> entity)
        {
            Check.Require(entity != null, "Entity may not be null when checking for duplicates");

            var session = GetSessionFor(entity);

            var previousFlushMode = session.FlushMode;

            // We do NOT want this to flush pending changes as checking for a duplicate should 
            // only compare the object against data that's already in the database
            session.FlushMode = FlushMode.Never;

            var criteria =
                session.CreateCriteria(entity.GetType()).Add(Restrictions.Not(Restrictions.Eq("Id", entity.Id))).SetMaxResults(1);

            AppendSignaturePropertyCriteriaTo(criteria, entity);
            var doesDuplicateExist = criteria.List().Count > 0;
            session.FlushMode = previousFlushMode;
            return doesDuplicateExist;
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

        private static ISession GetSessionFor(object entity)
        {
            var factoryKey = SessionFactoryKeyHelper.GetKeyFrom(entity);
            return NHibernateSession.CurrentFor(factoryKey);
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

            return string.Format("{0}{1}", parentPropertyName, signatureProperty.Name);
        }

        private static void AppendDateTimePropertyCriteriaTo(ICriteria criteria, string propertyName, object propertyValue)
        {
            criteria.Add(
                (DateTime)propertyValue > UninitializedDatetime
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