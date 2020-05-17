# SimpleLogFile

[![NuGet version (SoftCircuits.SimpleLogFile)](https://img.shields.io/nuget/v/SoftCircuits.SimpleLogFile.svg?style=flat-square)](https://www.nuget.org/packages/SoftCircuits.SimpleLogFile/)

```
Install-Package SoftCircuits.SimpleLogFile
```

## Overview

Yet another log-file class for .NET. This one was designed to be dead simple to use when needing to log entries to a file.

Supports different entry levels, which can be filtered out or disabled altogether. Correctly formats Exceptions and can optionally include all inner exceptions. Virtual functions can be overridden to control entry formatting, where entries are written, and what the Delete method does.

## Using the Library

To use the library, start by creating an instance of the `LogFile` class.

```cs
LogFile logFile = new LogFile(path);

logFile.LogInfo("An information-level log entry");
logFile.LogError("An error-level log entry", "There was an error");
logFile.LogErrorFormat("There were {0} items", 25);
```

The code above would produce the following log entries.

```
[5/16/2020 5:56:54 PM][INFO] An information-level log entry
[5/16/2020 5:56:54 PM][ERROR] An error-level log entry : There was an error
[5/16/2020 5:56:54 PM][ERROR] There were 25 items
```

## Log Levels

The `LogFile` class provides the `LogInfo()`, `LogWarning()`, `LogError()`, and `LogCritical()` methods for creating log entries. There are also format versions of each method.

The method you choose determines the log entry importance, or *level*. The `LogLevel` property of the `LogFile` class can also be set. Only log entries with the same level or higher will be written to the log file. If the `LogLevel` property of the `LogFile` class is set to `LogLevel.None`, no log entries will be entered and logging is effectively disabled.

```cs
logFile.LogLevel = LogLevel.Warning;

// This entry will not be written because LogLevel.Info is a lower level than LogLevel.Warning
logFile.LogInfo("This is an information-level entry");

// This entry will be written because LogLevel.Error is a higher level than LogLevel.Warning
logFile.LogError("This is an error-level entry");

logFile.LogLevel = LogLevel.None;

// Now even a critical log entry will not be written
logFile.LogCritical("This is an error-level entry");
```

In addition to the methods mentioned above, you can also use the `Log()` method. This method takes a `LogLevel` argument. This version requires a little more typing.

```cs
logFile.Log(LogLevel.Info, "This is an information-level entry");
```

## Exceptions

SimpleLogFile has some special handling for .NET exceptions (`Exception` and classes that derive from it). Consider the code below.

```cs
// Create an exception with inner exceptions
Exception ex = new ArgumentNullException("parameterName");
ex = new InvalidOperationException("Unable to do that", ex);
ex = new InvalidDataException("Unable to do this", ex);
ex = new InvalidProgramException("There was a problem!", ex);

// No inner exceptions logged here
logFile.LogError("Text and an exception (not logging inner exceptions)", ex);

// Now inner exceptions are logged
logFile.LogInnerExceptions = true;
logFile.LogError("Text and an exception (logging inner exceptions)", ex);
```

The code above would produce the following log entries.

```
[5/16/2020 6:38:50 PM][ERROR] Text and an exception (not logging inner exceptions) : System.InvalidProgramException: There was a problem!
[5/16/2020 6:38:50 PM][ERROR] Text and an exception (logging inner exceptions) : System.InvalidProgramException: There was a problem!
  --> [INNER EXCEPTION] System.IO.InvalidDataException: Unable to do this
  --> [INNER EXCEPTION] System.InvalidOperationException: Unable to do that
  --> [INNER EXCEPTION] System.ArgumentNullException: Value cannot be null. (Parameter 'parameterName')
```

## Deleting the Log File

In cases where you want to start a fresh log file, the `Delete()` method is provided. It simply deletes the current log file.

```cs
logFile.Delete();
```

## Overriding Behavior

This class was designed to be as straight forward and simple to use as possible. But it can be extended somewhat by deriving your class from `LogFile` and overriding one of the virtual methods.

Each of these methods are documented below.

#### protected virtual string OnFormat(LogLevel level, string text)

This method formats the text before it gets written to the log file. Override this method to change the formatting.

#### protected virtual string OnFormatSecondary(LogLevel level, string text)

This method formats the text for a secondary log entry before it gets written to the log file. Currently, the only secondary log entries supported are for the inner exceptions when `LogInnerExceptions` is true. Override this method to change the formatting of secondary entries.

#### protected virtual void OnWrite(string text)

This method performs the task of writing a formatted entry to the log file. You can override this method if you want to change the way the entry is written, or write it to another location.

#### protected virtual void OnDelete()

This method deletes the log file. Most likely, you will only need to override this method if you have also overridden `OnWrite()` and need to delete a different file or take other action.
