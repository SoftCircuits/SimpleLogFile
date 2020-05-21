// Copyright (c) 2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System;
using System.IO;
using System.Text;

namespace SoftCircuits.SimpleLogFile
{
    /// <summary>
    /// Supports logging entries to a log file.
    /// </summary>
    public class LogFile
    {
        private static readonly string NullString = "(null)";
        private static readonly string NullExceptionString = "(null exception)";

        /// <summary>
        /// Gets or sets the name of the file log entries are written to. May be set to <c>null</c>,
        /// which disables logging. Note that a derived class can override where log entries are
        /// written.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets which log entries are written to the log file. Only entries with the
        /// specified level or higher will be written to the log file.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets whether the log methods that receive an <see cref="Exception"/> argument
        /// will also log the inner exceptions. Otherwise, only the outer most exception is logged.
        /// </summary>
        public bool LogInnerExceptions { get; set; }

        /// <summary>
        /// Gets or sets whether the full exception class namespace is displayed when formatting
        /// exceptions.
        /// </summary>
        public bool ShowFullExceptionClassName { get; set; }

        /// <summary>
        /// Gets or sets whether the <see cref="LogDivider(char)"/> is enabled. Set this property
        /// to <c>false</c> and that method will have no effect.
        /// </summary>
        public bool DividersEnabled { get; set; }

        /// <summary>
        /// Gets or sets the delimiter inserted between multiple log entry items.
        /// </summary>
        public string LogItemDelimiter { get; set; }

        /// <summary>
        /// Constructs a new <see cref="LogFile"/> instance.
        /// </summary>
        /// <param name="filename">Name of the file log where entries will be written. May be
        /// <c>null</c>, which disables logging.</param>
        /// <param name="logLevel">Determines which log entries are written to the log file.
        /// Only log entries with the specified level or higher will be written.
        /// </param>
        /// <param name="logInnerExceptions">Determines whether the log methods that receive
        /// an <see cref="Exception"/> argument will also log all the inner exceptions.</param>
        public LogFile(string filename, LogLevel logLevel = LogLevel.All, bool logInnerExceptions = false)
        {
            Filename = filename;
            LogLevel = logLevel;
            LogInnerExceptions = logInnerExceptions;
            ShowFullExceptionClassName = false;
            DividersEnabled = true;
            LogItemDelimiter = " : ";
        }

        /// <summary>
        /// Logs an entry.
        /// </summary>
        /// <param name="level">Specifies the entry importance, or level.</param>
        /// <param name="args">Any number of values for this entry.</param>
        public void Log(LogLevel level, params object[] args)
        {
            if (level >= LogLevel)
            {
                OnWrite(OnFormat(level, FormatItems(args, out Exception ex)));
                // Log inner exceptions if specified and we found an exception
                if (LogInnerExceptions && ex != null)
                {
                    ex = ex.InnerException;
                    while (ex != null)
                    {
                        OnWrite(OnFormatSecondary(level, $"[INNER EXCEPTION] {OnFormatException(ex)}"));
                        ex = ex.InnerException;
                    }
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
            if (level >= LogLevel)
                OnWrite(OnFormat(level, string.Format(format, args)));
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
        /// Writes a horizontal divider to help separate groups of log entries.
        /// </summary>
        /// <param name="lineCharacter">Specifies the character used to draw the line.</param>
        public void LogDivider(char lineCharacter = '-')
        {
            if (LogLevel < LogLevel.None && DividersEnabled)
                OnWrite(new string(lineCharacter, 79));
        }

        /// <summary>
        /// Deletes the current log file.
        /// </summary>
        public void Delete() => OnDelete();

        #region Overridables

        /// <summary>
        /// Overridable handler to format text before it is written to the log file.
        /// </summary>
        /// <param name="level">Specifies the entry importance, or level.</param>
        /// <param name="text">Text to be written to the log file.</param>
        /// <returns>The formatted text.</returns>
        protected virtual string OnFormat(LogLevel level, string text)
        {
            return string.Format("[{0:G}][{1}] {2}",
                DateTime.Now,
                level.ToString().ToUpper(),
                text ?? NullString);
        }

        /// <summary>
        /// Overridable handler to format text for secondary log entries.
        /// Currently, this is only used for inner exceptions.
        /// </summary>
        /// <param name="level">Specifies the entry importance, or level.</param>
        /// <param name="text">Text to be written to the log file.</param>
        /// <returns>The formatted text.</returns>
        protected virtual string OnFormatSecondary(LogLevel level, string text)
        {
            return string.Format("  --> {0}", text);
        }

        /// <summary>
        /// Overridable handler to format an exception.
        /// </summary>
        /// <param name="ex">The exception to format.</param>
        /// <returns>The formatted exception.</returns>
        protected virtual string OnFormatException(Exception ex)
        {
            if (ex == null)
                return NullExceptionString;
            Type type = ex.GetType();
            string exceptionName = ShowFullExceptionClassName ? type.FullName : type.Name;
            return $"{exceptionName}: {ex.Message}";
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
        /// <param name="ex">Returns a reference to the first item that
        /// was an exception, or <c>null</c> if no items were exceptions.</param>
        /// <returns>The formatted string.</returns>
        private string FormatItems(object[] items, out Exception ex)
        {
            ex = null;

            StringBuilder builder = new StringBuilder();
            foreach (object item in items)
            {
                if (builder.Length > 0)
                    builder.Append(LogItemDelimiter ?? string.Empty);
                if (item == null)
                {
                    builder.Append(NullString);
                }
                else if (item is Exception exception)
                {
                    builder.Append(OnFormatException(exception));
                    if (ex == null)
                        ex = exception;
                }
                else builder.Append(item.ToString());
            }
            return builder.ToString();
        }

        #endregion

    }
}
