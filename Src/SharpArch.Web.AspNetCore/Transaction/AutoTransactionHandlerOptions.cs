namespace SharpArch.Web.AspNetCore.Transaction
{
    /// <summary>
    ///     Configuration options for <see cref="AutoTransactionHandler" />.
    /// </summary>
    public class AutoTransactionHandlerOptions
    {
        /// <summary>
        ///     Ensure only one session registry contains database with given identifier.
        /// </summary>
        public bool EnsureSingleSessionForDatabase { get; set; }
    }
}
