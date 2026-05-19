//
// ApiKeyProfile.cs
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
using System.Collections.Generic;
using System.Linq;

namespace QuartzRestApi;

/// <summary>
///     Represents a named API key profile that controls which Scheduler endpoints
///     the bearer of <see cref="ApiKey"/> is allowed to call.
/// </summary>
/// <remarks>
///     <para>
///         When <see cref="AllowedRoutes"/> is empty (the default) the profile grants
///         access to <b>all</b> endpoints — equivalent to the old single-key behaviour.
///     </para>
///     <para>
///         When one or more routes are listed in <see cref="AllowedRoutes"/> the bearer
///         may only call those routes; any other request returns <c>403 Forbidden</c>.
///     </para>
///     <para>
///         Route names are matched case-insensitively against the path segment after the
///         leading slash, e.g. <c>"Scheduler/Start"</c> or <c>"Scheduler/DeleteJob"</c>.
///         Wildcard suffixes are supported: <c>"Scheduler/Shutdown*"</c> matches both
///         <c>Scheduler/Shutdown</c> and <c>Scheduler/Shutdown/true</c>.
///     </para>
/// </remarks>
/// <example>
/// <code>
/// // Read-only profile — may only inspect the scheduler, never mutate it.
/// var readOnly = new ApiKeyProfile(
///     name:    "ReadOnly",
///     apiKey:  "key-readonly-abc123",
///     allowedRoutes: [
///         "Scheduler/SchedulerName",
///         "Scheduler/SchedulerInstanceId",
///         "Scheduler/GetMetaData",
///         "Scheduler/GetJobGroupNames",
///         "Scheduler/GetTriggerGroupNames",
///         "Scheduler/GetJobKeys",
///         "Scheduler/GetJobDetail",
///         "Scheduler/GetTrigger",
///         "Scheduler/GetTriggerState",
///         "Scheduler/GetCurrentlyExecutingJobs"
///     ]);
///
/// // Admin profile — full access (empty allowedRoutes = all routes allowed).
/// var admin = new ApiKeyProfile("Admin", "key-admin-xyz789");
///
/// var host = new SchedulerHost("http://localhost:45000", scheduler, logger,
///     profiles: [admin, readOnly]);
/// </code>
/// </example>
public sealed class ApiKeyProfile
{
    #region Properties

    /// <summary>
    ///     A human-readable name for this profile, used in log messages.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The secret API key that identifies this profile.
    ///     Sent by the client in the <c>X-Api-Key</c> HTTP header.
    /// </summary>
    public string ApiKey { get; }

    /// <summary>
    ///     The set of route paths this profile is allowed to access.
    ///     An empty collection means <b>all routes are allowed</b>.
    /// </summary>
    /// <remarks>
    ///     Each entry is matched case-insensitively against the full request path
    ///     (without leading slash).  A trailing <c>*</c> acts as a wildcard:
    ///     <c>"Scheduler/Shutdown*"</c> matches <c>Scheduler/Shutdown</c> and
    ///     <c>Scheduler/Shutdown/true</c>.
    /// </remarks>
    public IReadOnlyCollection<string> AllowedRoutes { get; }

    #endregion

    #region Constructor

    /// <summary>
    ///     Creates a new <see cref="ApiKeyProfile"/>.
    /// </summary>
    /// <param name="name">A human-readable name for this profile.</param>
    /// <param name="apiKey">The secret API key for this profile.</param>
    /// <param name="allowedRoutes">
    ///     Optional whitelist of allowed route paths.
    ///     Pass <see langword="null"/> or an empty collection to allow all routes.
    /// </param>
    public ApiKeyProfile(string name, string apiKey, IEnumerable<string> allowedRoutes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Profile name must not be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key must not be empty.", nameof(apiKey));

        Name = name;
        ApiKey = apiKey;
        AllowedRoutes = allowedRoutes?.ToList().AsReadOnly()
                        ?? (IReadOnlyCollection<string>)Array.Empty<string>();
    }

    #endregion

    #region IsRouteAllowed

    /// <summary>
    ///     Returns <see langword="true"/> when this profile grants access to
    ///     <paramref name="requestPath"/>.
    /// </summary>
    /// <param name="requestPath">
    ///     The HTTP request path without leading slash,
    ///     e.g. <c>"Scheduler/Start"</c>.
    /// </param>
    internal bool IsRouteAllowed(string requestPath)
    {
        // Empty whitelist → full access.
        if (AllowedRoutes.Count == 0)
            return true;

        foreach (var route in AllowedRoutes)
        {
            if (route.EndsWith('*'))
            {
                // Wildcard prefix match.
                var prefix = route[..^1];
                if (requestPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else if (string.Equals(requestPath, route, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    #endregion
}
