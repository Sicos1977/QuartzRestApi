//
// GroupMatcher.cs
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
using System.Text.Json;
using System.Text.Json.Serialization;
using Quartz.Util;

namespace QuartzRestApi.Wrappers;

/// <summary>
///     Json wrapper for the Quartz <see cref="Quartz.Impl.Matchers.GroupMatcher{T}" />
/// </summary>
/// <typeparam name="T"></typeparam>
public class GroupMatcher<T> where T : Key<T>
{
    #region Properties
    /// <summary>
    ///     The <see cref="GroupMatcherType" />
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("Type")]
    public GroupMatcherType Type { get; private set; }

    /// <summary>
    ///     The value to match
    /// </summary>
    [JsonPropertyName("Value")]
    public string Value { get; private set; }
    #endregion

    #region Constructor
    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="type">The <see cref="GroupMatcherType" /></param>
    /// <param name="value">The value to match</param>
    public GroupMatcher(GroupMatcherType type, string value)
    {
        Type = type;
        Value = value;
    }
    #endregion

    #region ToJsonString
    /// <summary>
    ///     Returns this object as a json string
    /// </summary>
    /// <returns></returns>
    public string ToJsonString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
    #endregion

    #region FromJsonString
    /// <summary>
    ///     Returns the <see cref="GroupMatcher{T}" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="Trigger" />
    /// </returns>
    public static GroupMatcher<T> FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<GroupMatcher<T>>(json);
    }
    #endregion

    #region ToGroupMatcher
    public Quartz.Impl.Matchers.GroupMatcher<T> ToGroupMatcher()
    {
        switch (Type)
        {
            case GroupMatcherType.Contains:
                return Quartz.Impl.Matchers.GroupMatcher<T>.GroupContains(Value);

            case GroupMatcherType.EndsWith:
                return Quartz.Impl.Matchers.GroupMatcher<T>.GroupEndsWith(Value);

            case GroupMatcherType.Equals:
                return Quartz.Impl.Matchers.GroupMatcher<T>.GroupEquals(Value);

            case GroupMatcherType.StartsWith:
                return Quartz.Impl.Matchers.GroupMatcher<T>.GroupStartsWith(Value);

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion
}