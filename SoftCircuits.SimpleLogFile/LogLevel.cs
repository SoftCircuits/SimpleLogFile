// Copyright (c) 2020-2024 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

namespace SoftCircuits.SimpleLogFile
{
    /// <summary>
    /// Specifies the type, or level, of a log entry.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Specifies that no log entries are written to the log.
        /// </summary>
        None = -1,

        /// <summary>
        /// Specifies that only log entries with critical error level or lower are written to the log.
        /// </summary>
        Critical = 0,

        /// <summary>
        /// Specifies that only log entries with error level or lower are written to the log.
        /// </summary>
        Error = 1,

        /// <summary>
        /// Specifies that only log entries with warning level or lower are written to the log.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Specifies that log entries with information level or lower are written to the log.
        /// Same effect as using <see cref="LogLevel.All"/>.
        /// </summary>
        Info = 3,

        /// <summary>
        /// Specifies that all log entries are written to the log.
        /// </summary>
        All = 100,
    }
}
