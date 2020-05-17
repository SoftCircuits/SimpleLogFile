// Copyright (c) 2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System;
using System.IO;
using System.Text;

namespace SoftCircuits.SimpleLogFile
{
    /// <summary>
    /// - Can log text, exceptions or both.
    /// - Can optionally log all inner exceptions.
    /// - Can override OnWrite() to customize where log entry is written to.
    /// - Can override OnDelete() to customize action.
    /// - Can override OnFormat() to change how entries are formatted.
    /// - Several different levels of logging, can limit what levels of entries are
    ///   logged, or disable logging entirely.
    /// - Correctly handles log filename being null. Effectively disables logging.
    /// </summary>
    public class LogFile
    {
        private static readonly string NullString = "(null)";
        private static readonly string NullException = "(null exception)";

        /// <summary>
        /// Gets or sets the name of the file that log entries will be written to. May be set
        /// to <c>null</c>, which effectively disables logging. Note that derived classes can
        /// also override where log entries are written.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets which log entries are written to the log file. Log entries with the
        /// specified level or higher will be written.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets whether the log methods that accept an <see cref="Exception"/> argument
        /// also log all the inner exceptions.
        /// </summary>
        public bool LogInnerExceptions { get; set; }

        /// <summary>
        /// Constructs a new <see cref="LogFile"/> instance.
        /// </summary>
        /// <param name="filename">Name of the file log entries will be written to. May
        /// be <c>null</c>, which effectively disables logging.</param>
        /// <param name="logLevel">Determines which log entries are written to the log file.
        /// Log entries with the specified level or higher will be written.
        /// </param>
        /// <param name="logInnerExceptions">Determins whether the log methods that accept
        /// an <see cref="Exception"/> argument also log all the inner exceptions.</param>
        public LogFile(string filename, LogLevel logLevel = LogLevel.All, bool logInnerExceptions = false)
        {
            Filename = filename;
            LogLevel = logLevel;
            LogInnerExceptions = logInnerExceptions;
        }

        /// <summary>
        /// Logs an information-level entry.
        /// </summary>
        /// <param name="args">Any number of values for this entry.</param>
        public void LogInfo(params object[] args) => Log(LogLevel.Info, args);

        /// <summary>
        /// Logs a formatted information-level entry.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Any number of objects to format.</param>
        public void LogInfoFormat(string format, params object[] args) => LogFormat(LogLevel.Info, format, args);

        /// <summary>
        /// Logs an warning-level entry.
        /// </summary>
        /// <param name="args">Any number of values for this entry.</param>
        public void LogWarning(params object[] args) => Log(LogLevel.Warning, args);

        /// <summary>
        /// Logs a formatted warning-level entry.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Any number of objects to format.</param>
        public void LogWarningFormat(string format, params object[] args) => LogFormat(LogLevel.Warning, format, args);

        /// <summary>
        /// Logs an error-level entry.
        /// </summary>
        /// <param name="args">Any number of values for this entry.</param>
        public void LogError(params object[] args) => Log(LogLevel.Error, args);

        /// <summary>
        /// Logs a formatted error-level entry.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Any number of objects to format.</param>
        public void LogErrorFormat(string format, params object[] args) => LogFormat(LogLevel.Error, format, args);

        /// <summary>
        /// Logs an critical-level entry.
        /// </summary>
        /// <param name="args">Any number of values for this entry.</param>
        public void LogCritical(params object[] args) => Log(LogLevel.Critical, args);

        /// <summary>
        /// Logs a formatted critical-level entry.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Any number of objects to format.</param>
        public void LogCriticalFormat(string format, params object[] args) => LogFormat(LogLevel.Critical, format, args);

        /// <summary>
        /// Logs an entry.
        /// </summary>
        /// <param name="level">Specifies the entry importance, or level.</param>
        /// <param name="args">Any number of values for this entry.</param>
        public void Log(LogLevel level, params object[] args)
        {
            OnWrite(OnFormat(level, FormatItems(args, out Exception ex)));
            if (LogInnerExceptions && ex != null)
            {
                ex = ex.InnerException;
                while (ex != null)
                {
                    OnWrite(OnFormatSecondary(level, $"[INNER EXCEPTION] {FormatException(ex)}"));
                    ex = ex.InnerException;
                }
            }
        }

        /// <summary>
        /// Logs a formatted entry.
        /// </summary>
        /// <param name="level">Specifies the entry importance, or level.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Any number of objects to format.</param>
        public void LogFormat(LogLevel level, string format, params object[] args)
        {
            OnWrite(OnFormat(level, string.Format(format, args)));
        }

        /// <summary>
        /// Deletes the current log file.
        /// </summary>
        public void Delete() => OnDelete();

        #region Overridables

        /// <summary>
        /// Overridable handler to format text before it is written to the log file.
        /// </summary>
        /// <param name="text">Text to be written to the log file.</param>
        /// <returns>The formatted text.</returns>
        protected virtual string OnFormat(LogLevel level, string text)
        {
            return string.Format("[{0:G}][{1}] {2}",
                DateTime.Now,
                level.ToString().ToUpper(),
                text ?? NullString);
        }

        protected virtual string OnFormatSecondary(LogLevel level, string text)
        {
            return string.Format("  --> {0}", text);
        }

        /// <summary>
        /// Overridable handler to write text to the log file.
        /// </summary>
        /// <param name="text">Text to be written to the log file.</param>
        protected virtual void OnWrite(string text)
        {
            if (!string.IsNullOrEmpty(Filename))
            {
                using (StreamWriter writer = File.AppendText(Filename))
                {
                    writer.WriteLine(text ?? NullString);
                }
            }
        }

        /// <summary>
        /// Overridable handler to delete the current log file.
        /// </summary>
        protected virtual void OnDelete()
        {
            if (!string.IsNullOrEmpty(Filename))
                File.Delete(Filename);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Formats a collection of items into a single string.
        /// </summary>
        /// <param name="items">Items to format.</param>
        /// <param name="exception">Returns a reference to the first item that
        /// was an exception, or <c>null</c> if not items were an exception.</param>
        /// <returns>The formatted string.</returns>
        private string FormatItems(object[] items, out Exception exception)
        {
            exception = null;

            StringBuilder builder = new StringBuilder();
            foreach (object item in items)
            {
                if (builder.Length > 0)
                    builder.Append(" : ");
                if (item is Exception ex)
                {
                    builder.AppendFormat(FormatException(ex));
                    if (exception == null)
                        exception = ex;
                }
                else
                    builder.Append(item.ToString());
            }
            return builder.ToString();
        }

        /// <summary>
        /// Formats an exception.
        /// </summary>
        /// <param name="ex">The exception to format.</param>
        /// <returns>The formatted exception.</returns>
        private string FormatException(Exception ex)
        {
            if (ex == null)
                return NullException;
            return $"{ex.GetType().FullName}: {ex.Message}";
        }

        #endregion

    }
}
