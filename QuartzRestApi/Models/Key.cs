//
// Key.cs
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

using System.Text.Json.Serialization;

namespace QuartzRestApi.Models;

/// <summary>
///     A json wrapper for the <see cref="Quartz.TriggerKey" /> or <see cref="Quartz.JobKey" />
/// </summary>
public class Key
{
    #region Fields
    /// <summary>
    ///     Returns the name of the trigger
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; init; }

    /// <summary>
    ///     Returns the group of the trigger
    /// </summary>
    [JsonPropertyName("Group")]
    public string Group { get; init; }
    #endregion

    #region Constructor
    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="name">The name</param>
    public Key(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="group">The group</param>
    public Key(string name, string group)
    {
        Name = name;
        Group = group;
    }
    #endregion
}