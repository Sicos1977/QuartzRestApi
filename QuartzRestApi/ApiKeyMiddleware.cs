//
// ApiKeyMiddleware.cs
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

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace QuartzRestApi;

/// <summary>
///     Middleware that enforces API key authentication and optional per-profile
///     route restrictions when profiles have been configured on the
///     <see cref="SchedulerHost"/>.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item><b>No profiles configured</b> — authentication is disabled.</item>
///         <item><b>Valid key, unrestricted profile</b> — request passes through.</item>
///         <item><b>Valid key, restricted profile, allowed route</b> — request passes through.</item>
///         <item><b>Valid key, restricted profile, disallowed route</b> — 403 Forbidden.</item>
///         <item><b>Missing or unknown key</b> — 401 Unauthorized.</item>
///     </list>
/// </remarks>
internal sealed class ApiKeyMiddleware(RequestDelegate next, ApiKeyOptions options, ILogger logger = null)
{
    /// <summary>
    ///     The HTTP header name that must carry the API key.
    /// </summary>
    public const string HeaderName = "X-Api-Key";

    /// <summary>
    ///    Processes incoming HTTP requests, enforcing API key authentication and
    ///    optional per-profile route restrictions.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!options.IsEnabled)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var incomingKey))
        {
            logger?.LogWarning("Request to '{Path}' rejected: missing '{Header}' header", context.Request.Path, HeaderName);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Unauthorized: X-Api-Key header is required.").ConfigureAwait(false);
            return;
        }

        if (!options.TryGetProfile(incomingKey!, out var profile))
        {
            logger?.LogWarning("Request to '{Path}' rejected: unknown API key", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Unauthorized: invalid API key.").ConfigureAwait(false);
            return;
        }

        var requestPath = context.Request.Path.Value?.TrimStart('/') ?? string.Empty;

        if (!profile.IsRouteAllowed(requestPath))
        {
            logger?.LogWarning("Profile '{Profile}' denied access to '{Path}'", profile.Name, context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync($"Forbidden: profile '{profile.Name}' is not allowed to call this endpoint.").ConfigureAwait(false);
            return;
        }

        logger?.LogDebug("Profile '{Profile}' granted access to '{Path}'", profile.Name, context.Request.Path);
        await next(context).ConfigureAwait(false);
    }
}
