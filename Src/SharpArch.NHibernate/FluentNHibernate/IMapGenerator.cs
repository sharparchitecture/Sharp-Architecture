namespace SharpArch.NHibernate.FluentNHibernate
{
    using System.Xml;
    using JetBrains.Annotations;

    /// <summary>
    ///     Facilitates the visitor pattern for <see cref = "GeneratorHelper" /> to spit out the NHibernate
    ///     XML for the class.
    /// 
    ///     To use, have your mapper implement this interface.  Then, simply include the following line within
    ///     Generate():  return CreateMapping(new MappingVisitor());
    /// 
    ///     Now you can call Generate on your mapper class to view the generated XML.
    /// </summary>
    /// <remarks>
    ///     This is not necessary for Fluent Nhibernate to function properly.
    /// </remarks>
    [PublicAPI]
    public interface IMapGenerator
    {
        /// <summary>
        /// File name.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Generates XML mapping document.
        /// </summary>
        [NotNull]
        XmlDocument Generate();
    }
}