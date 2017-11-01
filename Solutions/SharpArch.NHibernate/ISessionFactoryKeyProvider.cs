namespace SharpArch.NHibernate
{
    using JetBrains.Annotations;


    /// <summary>
    /// Provides the key to retrieve session factory from IoC.
    /// </summary>
    [PublicAPI]
    public interface ISessionFactoryKeyProvider
  {
        /// <summary>
        /// Gets the session factory key.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        string GetKey();

        /// <summary>
        /// Gets the session factory key.
        /// </summary>
        /// <param name="anObject">An optional object that may have an attribute used to determine the session factory key.</param>
        /// <returns></returns>
        [NotNull]
        string GetKeyFrom([NotNull] object anObject);
  }
}