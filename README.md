# SimpleLogFile

[![NuGet version (SoftCircuits.SimpleLogFile)](https://img.shields.io/nuget/v/SoftCircuits.SimpleLogFile.svg?style=flat-square)](https://www.nuget.org/packages/SoftCircuits.SimpleLogFile/)

```
Install-Package SoftCircuits.SimpleLogFile
```

## Overview

Yet another log-file class for .NET. This one was designed to be dead simple to use when writing log entries to a file. (Although it can be extended to write entries elsewhere.)

The class supports different log entry levels, which can be filtered or disabled entirely. The library has special handling for formatting exceptions, and can optionally write all inner exceptions. Virtual functions can be overridden to control log entry formatting, where and how log entries are written, and what the `Delete()` method does.

The library does not keep the log file open. The file is opened each time a log entry needs to be written and then immediately closed. This ensures that all log entries are flushed to disk even if the program crashes unexpectedly. The class was also designed not to raise any exceptions if the log filename is set to `null`. In this case, logging is simply disabled.

## Using the Library

To use the library, start by creating an instance of the `LogFile` class. Then call any of the log methods to write a log entry.

```cs
LogFile logFile = new LogFile(path);

// Log entries with different importance, or 'levels'
logFile.LogInfo("An information-level log entry");
logFile.LogWarning("A warning-level log entry");
logFile.LogError("An error-level log entry");
logFile.LogCritical("A critical-level log entry");

// A divider helps separate groups of log entries
logFile.LogDivider();

// An entry can include any number of objects
logFile.LogError("An error-level log entry", 12345, 'n', "Error");

// The library has special handling for formatting exceptions. LogFile properties control
// whether inner exceptions are written, and whether the name of the exception type
// includes the namespace.
logFile.LogError("This parameter is required", new ArgumentNullException("parameterName"));

// And you can log formatted entries
logFile.LogErrorFormat("Expected {0} items, but found {1} items", 25, 26);
```

The code above produces the following log entries.

```
[5/16/2020 5:56:54 PM][INFO] An information-level log entry
[5/16/2020 5:56:54 PM][WARNING] A warning-level log entry
[5/16/2020 5:56:54 PM][ERROR] An error-level log entry
[5/16/2020 5:56:54 PM][CRITICAL] A critical-level log entry
-------------------------------------------------------------------------------
[5/16/2020 5:56:54 PM][ERROR] An error-level log entry : 12345 : n : Error
[5/16/2020 5:56:54 PM][ERROR] This parameter is required : ArgumentNullException: Value cannot be null. (Parameter 'parameterName')
[5/16/2020 5:56:54 PM][ERROR] Expected 25 items, but found 26 items
```

## Log Levels

Each of the log methods shown in the previous example specifies the log entry importance, or *level*. The `LogFile` class also has a `LogLevel` property. This allows you to easily filter out lower level log entries.

If the log entry's level is not equal to or higher than the `LogFile`'s `LogLevel` property, the entry will not be written to the log file. If the `LogLevel` property of the `LogFile` class is set to `LogLevel.None`, no log entries will be written and logging is effectively disabled.

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

In addition to the methods mentioned above, you can also use the `Log()` method. This method takes a `LogLevel` argument, and requires a little more typing.

```cs
// These two lines are equivalent
logFile.LogError("An error-level log entry");
logFile.Log(LogLevel.Error, "An error-level log entry");
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
logFile.LogError("Something went wrong", ex);

// Now log inner exceptions
logFile.LogInnerExceptions = true;
logFile.LogError("Something went wrong", ex);
```

The code above would produce the following log entries.

```
[5/16/2020 6:38:50 PM][ERROR] Something went wrong : InvalidProgramException: There was a problem!
[5/16/2020 6:38:50 PM][ERROR] Something went wrong : InvalidProgramException: There was a problem!
  --> [INNER EXCEPTION] InvalidDataException: Unable to do this
  --> [INNER EXCEPTION] InvalidOperationException: Unable to do that
  --> [INNER EXCEPTION] ArgumentNullException: Value cannot be null. (Parameter 'parameterName')
```

## Deleting the Log File

In cases where you want to start a fresh log file, the `Delete()` method is provided. It simply deletes the current log file.

```cs
logFile.Delete();
```

## Overriding Behavior

This class was designed to be as straight forward and simple to use as possible. But it can be extended somewhat by deriving your class from `LogFile` and overriding one of the virtual methods.

Each of these methods are documented below.

```cs
protected virtual string OnFormat(LogLevel level, string text);
```

This method formats the text before it gets written to the log file. Override this method to change the formatting.

```cs
protected virtual string OnFormatSecondary(LogLevel level, string text);
```

This method formats the text for a secondary log entry before it gets written to the log file. Currently, the only secondary log entries supported are for the inner exceptions when `LogInnerExceptions` is true. Override this method to change the formatting of secondary entries.

```cs
protected virtual string OnFormatException(Exception ex);
```

This method formats an exception that is part of a log entry. Note that when logging inner exceptions, this method will be called for each inner exception. And so it should normally format only the top-level exception.

```cs
protected virtual void OnWrite(string text);
```

This method performs the task of writing a formatted entry to the log file. You can override this method if you want to change the way the entry is written, or write it to another location.

```cs
protected virtual void OnDelete();
```

This method deletes the log file. Most likely, you will only need to override this method if you have also overridden `OnWrite()` and need to delete a different file or take other action.
