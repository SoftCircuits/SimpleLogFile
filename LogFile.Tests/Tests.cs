// Copyright (c) 2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftCircuits.SimpleLogFile;
using System;
using System.IO;

namespace LogFileTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void Test()
        {
            //string path = Path.GetTempFileName();
            string path = @"D:\Users\Jonathan\Desktop\LogFile.txt";

            LogFile logFile = new LogFile(path);

            logFile.LogLevel = LogLevel.None;

            logFile.LogInfo("An information-level log entry");
            logFile.LogWarning("A warning-level log entry");
            logFile.LogError("An error-level log entry");
            logFile.LogCritical("A critical-level log entry");

            logFile.LogDivider();

            Exception ex = new ArgumentNullException("parameterName");
            ex = new InvalidOperationException("Unable to do that", ex);
            ex = new InvalidDataException("Unable to do this", ex);
            ex = new InvalidProgramException("There was a problem!", ex);

            logFile.LogError("Text and an exception (not logging inner exceptions)", ex);

            logFile.LogInnerExceptions = true;
            logFile.LogError("Text and an exception (logging inner exceptions)", ex);
            logFile.LogInnerExceptions = false;

            logFile.LogDivider();

            logFile.LogInfo("I finished the first thing", "I did the second thing", "I breezed through the third thing", ex);

            logFile.LogFormat(LogLevel.Info, "Formatted log entry : {0}:{1}:{2}", 123, 456, 789);

            //File.Delete(path);
        }
    }
}
