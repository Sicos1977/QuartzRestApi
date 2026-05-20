//
// SchedulerConnectorException.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2022 - 2026 Magic-Sessions. (www.magic-sessions.com)
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

using System;

namespace QuartzRestApi.Exceptions;

/// <summary>
///     Exception that is thrown by <see cref="SchedulerConnector" /> when a communication
///     error occurs with the remote <see cref="SchedulerHost" />.
/// </summary>
/// <remarks>
///     This exception indicates that the fault originated inside the
///     <c>QuartzRestApi</c> assembly, either because the host returned a
///     non-success HTTP status code or because the response could not be
///     processed as expected.
/// </remarks>
public class SchedulerConnectorException : Exception
{
    #region Constructors
    /// <summary>
    ///     Initializes a new instance of <see cref="SchedulerConnectorException" />.
    /// </summary>
    public SchedulerConnectorException() { }

    /// <summary>
    ///     Initializes a new instance of <see cref="SchedulerConnectorException" /> with a
    ///     descriptive error message.
    /// </summary>
    /// <param name="message">A message that describes the error.</param>
    public SchedulerConnectorException(string message) : base(message) { }

    /// <summary>
    ///     Initializes a new instance of <see cref="SchedulerConnectorException" /> with a
    ///     descriptive error message and a reference to the inner exception that caused this
    ///     exception.
    /// </summary>
    /// <param name="message">A message that describes the error.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or <see langword="null" />
    ///     if no inner exception is specified.
    /// </param>
    public SchedulerConnectorException(string message, Exception innerException) : base(message, innerException) { }
    #endregion
}
