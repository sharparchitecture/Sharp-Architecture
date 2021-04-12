namespace SharpArch.Infrastructure.Logging
{
    using System;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;


    /// <summary>
    ///     Logs with specific log level.
    /// </summary>
    public readonly struct EnabledLogLevel
    {
        readonly ILogger _logger;
        readonly LogLevel _level;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="level">Log level to use.</param>
        internal EnabledLogLevel(ILogger logger, LogLevel level)
        {
            _logger = logger;
            _level = level;
        }

        /// <summary>
        ///     Logs message.
        /// </summary>
        /// <param name="message">Message template.</param>
        /// <param name="args">Template parameters.</param>
        public void Log(string message, params object[]? args)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.Log(_level, message, args);
        }

        /// <summary>
        ///     Logs message.
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        /// <param name="message">Message template.</param>
        /// <param name="args">Template parameters.</param>
        public void Log(Exception? ex, string message, params object[]? args)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.Log(_level, ex, message, args);
        }
    }
}
