//
// StreamLogger.cs
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
// Stream-based logger implementation for QuartzRestApi.
// Writes all log output to a provided stream (e.g., file, memory, network).
// =============================================================================

#nullable enable
using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace QuartzRestApi.Loggers;

/// <summary>
///     Logger that writes to a stream (file, memory, network, etc.).
/// </summary>
public sealed class StreamLogger : ILogger, IDisposable
{
    #region Fields
    /// <summary>
    ///     The category name
    /// </summary>
    private readonly string? _categoryName;

    /// <summary>
    ///     Minimal log level
    /// </summary>
    private readonly LogLevel _minLevel;

    /// <summary>
    ///     The stream writer for output
    /// </summary>
    private readonly StreamWriter _writer;

    /// <summary>
    ///     Whether this logger owns the stream and should dispose it
    /// </summary>
    private readonly bool _leaveOpen;

#if NETSTANDARD2_0
    /// <summary>
    ///     Lock for thread-safe writing
    /// </summary>
    private readonly object _lock = new();
#else
    /// <summary>
    ///     Lock for thread-safe writing
    /// </summary>
    private readonly Lock _lock = new();
#endif

    /// <summary>
    ///     Whether this instance has been disposed
    /// </summary>
    private bool _disposed;
    #endregion

    #region Constructor
    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamLogger"/> class.
    /// </summary>
    /// <param name="stream">The stream to write log output to.</param>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <param name="minLevel">The minimum log level to output. Defaults to Information.</param>
    /// <param name="leaveOpen">Whether to leave the stream open when disposing. Defaults to false.</param>
    public StreamLogger(Stream stream, string? categoryName = null, LogLevel minLevel = LogLevel.Information, bool leaveOpen = false)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        if (!stream.CanWrite)
            throw new ArgumentException("Stream must be writable.", nameof(stream));

        _categoryName = categoryName;
        _minLevel = minLevel;
        _leaveOpen = leaveOpen;
        _writer = new StreamWriter(stream) { AutoFlush = true };
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamLogger"/> class with a file path.
    /// </summary>
    /// <param name="filePath">The file path to write log output to.</param>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <param name="minLevel">The minimum log level to output. Defaults to Information.</param>
    /// <param name="append">Whether to append to existing file or overwrite. Defaults to true.</param>
    public StreamLogger(string filePath, string categoryName, LogLevel minLevel = LogLevel.Information, bool append = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        if (string.IsNullOrWhiteSpace(categoryName))
            throw new ArgumentException("Category name cannot be null or whitespace.", nameof(categoryName));

        _categoryName = categoryName;
        _minLevel = minLevel;
        _leaveOpen = false;

        var fileStream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read);
        _writer = new StreamWriter(fileStream) { AutoFlush = true };
    }
    #endregion

    #region ILogger Implementation
    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (_disposed)
            return;

        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var level = logLevel switch
        {
            LogLevel.Trace       => "TRC",
            LogLevel.Debug       => "DBG",
            LogLevel.Information => "INF",
            LogLevel.Warning     => "WRN",
            LogLevel.Error       => "ERR",
            LogLevel.Critical    => "CRT",
            _                    => "   "
        };

        lock (_lock)
        {
            if (_disposed)
                return;

            _writer.WriteLine($"{timestamp} [{level}]:{(!string.IsNullOrWhiteSpace(_categoryName) ? $" {_categoryName}" : string.Empty)} - {message}");

            if (exception != null)
                _writer.WriteLine($"      {exception}");
        }
    }
    #endregion

    #region IDisposable Implementation
    /// <summary>
    ///     Disposes the logger and optionally the underlying stream.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_lock)
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                _writer.Flush();

                if (!_leaveOpen)
                    _writer.Dispose();
            }
            catch
            {
                // Ignore dispose errors
            }
        }
    }
    #endregion
}