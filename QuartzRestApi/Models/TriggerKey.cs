//
// TriggerKey.cs
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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuartzRestApi.Models;

/// <summary>
///     A json wrapper for the <see cref="Quartz.TriggerKey" />
/// </summary>
public class TriggerKey : Key
{
    #region Constructor
    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="name">The name of the trigger</param>
    [JsonConstructor]
    public TriggerKey(string name) : base(name)
    {
    }

    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="name">The name of the trigger</param>
    /// <param name="group">The group of the trigger</param>
    public TriggerKey(string name, string group) : base(name, group)
    {
    }

    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="key">The <see cref="Quartz.TriggerKey" /></param>
    public TriggerKey(Quartz.TriggerKey key) : base(key.Name, key.Group)
    {
    }
    #endregion

    #region ToTriggerKey
    /// <summary>
    ///     Returns this object as a Quartz <see cref="Quartz.TriggerKey" />
    /// </summary>
    /// <returns></returns>
    public Quartz.TriggerKey ToTriggerKey()
    {
        return new Quartz.TriggerKey(Name, Group);
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
    ///     Returns the <see cref="TriggerKey" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="Trigger" />
    /// </returns>
    public static TriggerKey FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<TriggerKey>(json);
    }
    #endregion
}