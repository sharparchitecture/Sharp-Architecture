namespace SharpArch.Domain.PersistenceSupport
{
    /// <summary>
    ///     Default database identifier.
    /// </summary>
    public static class DatabaseIdentifier
    {
        /// <summary>
        ///     Default database identifier.
        /// </summary>
        // ReSharper disable once ConvertToConstant.Global
        public static readonly string Default = "default";

        /// <summary>
        ///     Parameter name for database identifiers.
        ///     Used by exception enrichment, etc...
        /// </summary>
        public static readonly string ParameterName = "DatabaseIdentifier";
    }
}
