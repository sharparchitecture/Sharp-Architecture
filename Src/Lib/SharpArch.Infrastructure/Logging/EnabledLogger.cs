namespace SharpArch.Infrastructure.Logging
{
    using System;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;


    /// <summary>
    ///     Skips call to <see cref="ILogger.Log{TState}" /> to prevent unnecessary memory allocations.
    /// </summary>
    [PublicAPI]
    public readonly struct LogWrapper
    {
        readonly ILogger _logger;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="logger">Logger.</param>
        public LogWrapper(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Logs that contain the most detailed messages. These messages may contain sensitive application data.
        ///     These messages are disabled by default and should never be enabled in a production environment.
        /// </summary>
        /// <returns><see cref="EnabledLogLevel" /> or <c>null</c> if specified log level is not enabled.</returns>
        public EnabledLogLevel? Trace => _logger.IsEnabled(LogLevel.Trace)
            ? new EnabledLogLevel(_logger, LogLevel.Trace)
            : null;

        /// <summary>
        ///     Logs that are used for interactive investigation during development.  These logs should primarily contain
        ///     information useful for debugging and have no long-term value.
        /// </summary>
        /// <returns><see cref="EnabledLogLevel" /> or <c>null</c> if specified log level is not enabled.</returns>
        public EnabledLogLevel? Debug => _logger.IsEnabled(LogLevel.Trace)
            ? new EnabledLogLevel(_logger, LogLevel.Debug)
            : null;

        /// <summary>
        ///     Logs that track the general flow of the application. These logs should have long-term value.
        /// </summary>
        /// <returns><see cref="EnabledLogLevel" /> or <c>null</c> if specified log level is not enabled.</returns>
        public EnabledLogLevel? Information => _logger.IsEnabled(LogLevel.Trace)
            ? new EnabledLogLevel(_logger, LogLevel.Information)
            : null;

        /// <summary>
        ///     Logs that highlight an abnormal or unexpected event in the application flow, but do not otherwise cause the
        ///     application execution to stop.
        /// </summary>
        /// <returns><see cref="EnabledLogLevel" /> or <c>null</c> if specified log level is not enabled.</returns>
        public EnabledLogLevel? Warning => _logger.IsEnabled(LogLevel.Trace)
            ? new EnabledLogLevel(_logger, LogLevel.Warning)
            : null;

        /// <summary>
        ///     Logs that highlight when the current flow of execution is stopped due to a failure. These should indicate a
        ///     failure in the current activity, not an application-wide failure.
        /// </summary>
        /// <returns><see cref="EnabledLogLevel" /> or <c>null</c> if specified log level is not enabled.</returns>
        public EnabledLogLevel? Error => _logger.IsEnabled(LogLevel.Trace)
            ? new EnabledLogLevel(_logger, LogLevel.Error)
            : null;

        /// <summary>
        ///     Logs that describe an unrecoverable application or system crash, or a catastrophic failure that requires
        ///     immediate attention.
        /// </summary>
        /// <returns><see cref="EnabledLogLevel" /> or <c>null</c> if specified log level is not enabled.</returns>
        public EnabledLogLevel? Critical => _logger.IsEnabled(LogLevel.Trace)
            ? new EnabledLogLevel(_logger, LogLevel.Critical)
            : null;
    }
}
