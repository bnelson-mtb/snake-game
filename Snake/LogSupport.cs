// <copyright file="LogSupport.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

namespace CS3500.LogSupport;

public static class LogSupport
{
    public static void LogDetailsBrief(
        this ILogger logger,
        LogLevel level,
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        string fileName = Path.GetFileName(Path.TrimEndingDirectorySeparator(sourceFilePath)) ?? "unknown";

        logger.Log(level,
            "{date} T({threadID}) [{file,-20}:{line,-4} - {method,-20}] '{message}'",
            DateTime.Now.ToString("HH:mm:ss.fff"),
            Thread.CurrentThread.ManagedThreadId,
            fileName[..Math.Min(50, fileName.Length)],
            sourceLineNumber,
            memberName[..Math.Min(20, memberName.Length)],
            message);
    }
}




