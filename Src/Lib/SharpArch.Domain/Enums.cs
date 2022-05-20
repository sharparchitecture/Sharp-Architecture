namespace SharpArch.Domain
{
    using JetBrains.Annotations;


    /// <summary>
    ///     Contains global enumerations.
    /// </summary>
    [PublicAPI]
    public static class Enums
    {
        /// <summary>
        ///     Provides an NHibernate.LockMode facade so as to avoid a direct dependency on the NHibernate DLL.
        /// </summary>
        /// <remarks>
        ///     Further information concerning lock modes may be found at:
        ///     http://docs.jboss.org/hibernate/orm/4.1/manual/en-US/html/ch13.html#transactions-locking
        /// </remarks>
        public enum LockMode : byte
        {
            /// <summary>
            ///     Represents the absence of a lock. All objects switch to this lock mode at the
            ///     end of a Transaction. Objects associated with the session via a call to
            ///     update() or saveOrUpdate() also start out in this lock mode.
            /// </summary>
            None = 0,

            /// <summary>
            ///     This lock mode is acquired automatically when data is read under Repeatable Read
            ///     or Serializable isolation level. It can be re-acquired by explicit user request.
            /// </summary>
            Read = 1,

            /// <summary>
            ///     This lock mode can be acquired upon explicit user request using
            ///     <c>SELECT ... FOR UPDATE</c> on databases which support that syntax.
            /// </summary>
            Upgrade = 2,

            /// <summary>
            ///     This lock mode can be acquired upon explicit user request using a
            ///     <c>SELECT ... FOR UPDATE NOWAIT</c> under Oracle.
            /// </summary>
            UpgradeNoWait = 3,

            /// <summary>
            ///     This lock mode is acquired automatically when a row is updated or inserted.
            /// </summary>
            Write = 4
        }
    }
}
