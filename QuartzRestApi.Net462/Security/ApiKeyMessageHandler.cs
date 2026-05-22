//
// ApiKeyMessageHandler.cs
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

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace QuartzRestApi.Security;
/// <summary>
///     Web API 2 delegating handler that validates the <c>X-Api-Key</c> header
///     when <see cref="ApiKeyOptions.IsEnabled"/> is <see langword="true"/>.
/// </summary>
internal sealed class ApiKeyMessageHandler : DelegatingHandler
{
    #region Fields
    /// <summary>
    ///     Configuration options for API key authentication.
    /// </summary>
    private readonly ApiKeyOptions _options;
    #endregion

    #region ApiKeyMessageHandler
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiKeyMessageHandler"/> class.
    /// </summary>
    /// <param name="options">The API key configuration options.</param>
    internal ApiKeyMessageHandler(ApiKeyOptions options)
    {
        _options = options;
    }
    #endregion

    #region SendAsync
    /// <summary>
    ///     Sends an HTTP request and validates API key authentication if enabled.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the HTTP response message.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_options.IsEnabled || request.Headers.TryGetValues("X-Api-Key", out var values) &&
            _options.TryGetProfile(values.First(), out _))
            return base.SendAsync(request, cancellationToken);

        var response = request.CreateResponse(HttpStatusCode.Unauthorized);
        return Task.FromResult(response);

    }
    #endregion
}

