namespace SharpArch.NHibernate
{
    using System;

    /// <summary>
    ///     Provides the ability to decorate repositories with an attribute defining the factory key
    ///     for the given repository; accordingly, the respective session factory will be used to 
    ///     communicate with the database.  This allows you to declare different repositories to 
    ///     communicate with different databases.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SessionFactoryAttribute : Attribute
    {
        public readonly string FactoryKey;

        public SessionFactoryAttribute(string factoryKey)
        {
            this.FactoryKey = factoryKey;
        }

        /// <summary>
        ///     Global method to retrieve the factory key from an object, as defined in its 
        ///     SessionFactoryAttribute, if available.  Defaults to the DefaultFactoryKey 
        ///     if not found.
        /// </summary>
        public static string GetKeyFrom(object target)
        {
            if (!NHibernateSession.IsConfiguredForMultipleDatabases())
            {
                return NHibernateSession.DefaultFactoryKey;
            }

            var objectType = target.GetType();

            var attributes = objectType.GetCustomAttributes(typeof(SessionFactoryAttribute), true);

            if (attributes.Length > 0)
            {
                var attribute = (SessionFactoryAttribute)attributes[0];
                return attribute.FactoryKey;
            }

            return NHibernateSession.DefaultFactoryKey;
        }
    }
}