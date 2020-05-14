namespace SharpArch.Web.AspNetCore.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Domain.PersistenceSupport;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;


    /// <summary>
    ///     An attribute that used to indicate that action must be wrapped in transaction.
    ///     <para>
    ///         Attribute can be applied globally, at controller or at action level.
    ///     </para>
    ///     <para>
    ///         Note: This is marker attribute only, <see cref="AutoTransactionHandler" /> must be added to filter s
    ///         collection in order to enable auto-transactions.
    ///     </para>
    ///     <para>
    ///         Transaction is either committed or rolled back after action execution is completed.
    ///         Note: accessing database from the View is considered as a bad practice.
    ///     </para>
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Transaction will be committed after action execution is completed and no unhandled exception occurred, see
    ///     <see cref="ActionExecutedContext.ExceptionHandled" />.
    ///     Transaction will be rolled back if there was unhandled exception in action or model validation was failed and
    ///     <see cref="RollbackOnModelValidationError" /> is <c>true</c>.
    /// </para>
    /// <para>
    ///     This attribute does initiate distributed transaction.
    ///     When used with multiple databases, committed transactions will not be rolled back.
    /// </para>
    /// </remarks>
    [BaseTypeRequired(typeof(ControllerBase))]
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class TransactionAttribute : Attribute, IFilterMetadata
    {
        static readonly IReadOnlyList<string> _singleDefaultDatabase = new[] {DatabaseIdentifier.Default};

        /// <summary>
        ///     Gets or sets a value indicating whether rollback transaction in case of model validation error.
        /// </summary>
        /// <value>
        ///     <c>true</c> if transaction must be rolled back in case of model validation error; otherwise, <c>false</c>.
        ///     Defaults to <c>true</c>.
        /// </value>
        public bool RollbackOnModelValidationError { get; }

        /// <summary>
        ///     Transaction isolation level.
        /// </summary>
        /// <value>Defaults to <c>ReadCommitted</c>.</value>
        public IsolationLevel IsolationLevel { get; }

        /// <summary>
        ///     Database identifiers.
        /// </summary>
        public IReadOnlyList<string> DatabaseIdentifiers { get; }

        /// <inheritdoc />
        public TransactionAttribute([NotNull] params string[] databaseIdentifiers)
            : this(IsolationLevel.ReadCommitted, true, databaseIdentifiers)
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level.</param>
        /// <param name="rollbackOnModelValidationError">
        ///     indicates that transaction should be rolled back in case of
        ///     model validation error.
        /// </param>

        public TransactionAttribute(
            IsolationLevel isolationLevel, bool rollbackOnModelValidationError)
        {
            IsolationLevel = isolationLevel;
            RollbackOnModelValidationError = rollbackOnModelValidationError;
            DatabaseIdentifiers = _singleDefaultDatabase;
        }

        /// <summary>
        ///     Creates transaction attribute specifying multiple databases.
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level.</param>
        /// <param name="rollbackOnModelValidationError">
        ///     indicates that transaction should be rolled back in case of
        ///     model validation error.
        /// </param>
        /// <param name="databaseIdentifiers">Specifies databases participating in transaction.</param>
        public TransactionAttribute(
            IsolationLevel isolationLevel, bool rollbackOnModelValidationError,
            [NotNull] params string[] databaseIdentifiers
        )
        {
            if (databaseIdentifiers == null) throw new ArgumentNullException(nameof(databaseIdentifiers));
            if (databaseIdentifiers.Length == 0) throw new ArgumentException("Collection can not be empty.", nameof(databaseIdentifiers));

            var dbIds = new Dictionary<string, bool>(databaseIdentifiers.Length);
            foreach (var databaseIdentifier in databaseIdentifiers)
            {
                dbIds[databaseIdentifier] = true;
            }

            if (dbIds.Count == 1 && dbIds.ContainsKey(DatabaseIdentifier.Default))
                DatabaseIdentifiers = _singleDefaultDatabase;
            else
                DatabaseIdentifiers = dbIds.Keys.ToArray();

            IsolationLevel = isolationLevel;
            RollbackOnModelValidationError = rollbackOnModelValidationError;
        }
    }
}
