//
// ApiKeyOptions.cs
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

using System.Collections.Generic;
using System.Linq;

namespace QuartzRestApi;

/// <summary>
///     Holds the API key profiles that are passed from <see cref="SchedulerHost"/>
///     into the DI container so <see cref="ApiKeyMiddleware"/> can read them.
/// </summary>
internal sealed class ApiKeyOptions
{
    #region Properties
    /// <summary>
    ///     All configured profiles.  Empty when authentication is disabled.
    /// </summary>
    public IReadOnlyList<ApiKeyProfile> Profiles { get; }

    /// <summary>
    ///     <see langword="true"/> when at least one profile is configured and
    ///     authentication should be enforced.
    /// </summary>
    public bool IsEnabled => Profiles.Count > 0;
    #endregion

    #region Constructors
    /// <summary>
    ///     Creates options from an explicit list of profiles.
    /// </summary>
    internal ApiKeyOptions(IEnumerable<ApiKeyProfile> profiles)
    {
        Profiles = profiles?.Where(p => p != null).ToList() ?? (IReadOnlyList<ApiKeyProfile>)[];
    }

    /// <summary>
    ///     Convenience constructor for the backwards-compatible single-key scenario.
    ///     When <paramref name="apiKey"/> is null or empty authentication is disabled.
    /// </summary>
    internal ApiKeyOptions(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Profiles = [];
            return;
        }

        // Single key → admin profile with full access (empty whitelist).
        Profiles = [new ApiKeyProfile("Default", apiKey)];
    }
    #endregion

    #region TryGetProfile
    /// <summary>
    ///     Tries to find the profile that owns <paramref name="apiKey"/>.
    /// </summary>
    internal bool TryGetProfile(string apiKey, out ApiKeyProfile profile)
    {
        profile = Profiles.FirstOrDefault(p => p.ApiKey == apiKey);   // Intentionally case-sensitive for secrets.

        return profile != null;
    }
    #endregion
}
