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

namespace QuartzRestApi.Security
{
    /// <summary>
    ///     Holds the API key profiles that are passed into the <see cref="ApiKeyMessageHandler"/>
    ///     so requests can be authenticated.
    /// </summary>
    public sealed class ApiKeyOptions
    {
        #region Profiles
        /// <summary>All configured profiles. Empty when authentication is disabled.</summary>
        public IReadOnlyList<ApiKeyProfile> Profiles { get; }

        /// <summary><see langword="true"/> when at least one profile is configured.</summary>
        public bool IsEnabled => Profiles.Count > 0;

        /// <summary>Creates options from an explicit list of profiles.</summary>
        public ApiKeyOptions(IEnumerable<ApiKeyProfile> profiles)
        {
            Profiles = new List<ApiKeyProfile>(profiles ?? Enumerable.Empty<ApiKeyProfile>());
        }
        #endregion

        #region ApiKeyOptions
        /// <summary>
        ///     Convenience constructor for the single-key scenario.
        ///     When <paramref name="apiKey"/> is null or empty, authentication is disabled.
        /// </summary>
        public ApiKeyOptions(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Profiles = new List<ApiKeyProfile>();
                return;
            }

            Profiles = new List<ApiKeyProfile> { new ApiKeyProfile("Default", apiKey) };
        }
        #endregion

        #region Profiles
        /// <summary>Tries to find the profile that owns <paramref name="apiKey"/>.</summary>
        public bool TryGetProfile(string apiKey, out ApiKeyProfile profile)
        {
            profile = Profiles.FirstOrDefault(p => p.ApiKey == apiKey);
            return profile != null;
        }
        #endregion
    }
}