//
// ConsoleLogger.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2026 Kees van Spelde. (www.magic-sessions.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// =============================================================================
//
// Console logger implementation using StreamLogger with Console.OpenStandardOutput().
// =============================================================================

#nullable enable
using System;
using Microsoft.Extensions.Logging;

namespace QuartzRestApi.Loggers;

/// <summary>
///     Console logger for LibreOfficeKit. Internally uses <see cref="StreamLogger"/> 
///     with <see cref="Console.OpenStandardOutput()"/>.
/// </summary>
public sealed class ConsoleLogger : ILogger, IDisposable
{
    #region Fields
    /// <summary>
    ///     The underlying stream logger writing to Console.Out
    /// </summary>
    private readonly StreamLogger _streamLogger;
    #endregion

    #region Constructor
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConsoleLogger"/> class.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <param name="minLevel">The minimum log level to output.</param>
    public ConsoleLogger(string? categoryName = null, LogLevel minLevel = LogLevel.Information)
    {
        // Use Console.OpenStandardOutput() to get the console stream
        // leaveOpen: true because we don't own the console stream
        var consoleStream = Console.OpenStandardOutput();
        _streamLogger = new StreamLogger(consoleStream, categoryName, minLevel, leaveOpen: true);
    }
    #endregion

    #region ILogger Implementation
    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _streamLogger.BeginScope(state);

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => _streamLogger.IsEnabled(logLevel);

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        _streamLogger.Log(logLevel, eventId, state, exception, formatter);
    }
    #endregion

    #region IDisposable Implementation
    /// <summary>
    ///     Disposes the logger. Note: Console stream is not disposed (leaveOpen: true).
    /// </summary>
    public void Dispose()
    {
        _streamLogger.Dispose();
    }
    #endregion
}