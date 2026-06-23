//
// TriggerKeys.cs
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace QuartzRestApi.Models.Triggers;

/// <summary>
///     Json wrapper for a list of Quartz <see cref="Quartz.TriggerKey" />s
/// </summary>
public class TriggerKeys : List<TriggerKey>
{
    #region Constructor
    /// <summary>
    ///     Needed for JSON deserialization
    /// </summary>
    public TriggerKeys()
    {
    }

    /// <summary>
    ///     Makes this object and sets all it's needed properties
    /// </summary>
    /// <param name="triggerKeys">A <see cref="ReadOnlyCollection{T}" /> of <see cref="Quartz.TriggerKey" />s</param>
    public TriggerKeys(IEnumerable<Quartz.TriggerKey> triggerKeys)
    {
        foreach (var triggerKey in triggerKeys)
            Add(new TriggerKey(triggerKey.Name, triggerKey.Group));
    }
    #endregion

    #region ToTriggerKeys
    /// <summary>
    ///     Returns a <see cref="ReadOnlyCollection{T}" /> of <see cref="Quartz.TriggerKey" />s
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<Quartz.TriggerKey> ToTriggerKeys()
    {
        var result = this.Select(m => m.ToTriggerKey()).ToList();
        return new ReadOnlyCollection<Quartz.TriggerKey>(result);
    }
    #endregion

    #region ToJsonString
    /// <summary>
    ///     Returns this object as a json string
    /// </summary>
    /// <returns></returns>
    public string ToJsonString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
    #endregion

    #region FromJsonString
    /// <summary>
    ///     Returns the <see cref="TriggerKeys" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="TriggerKeys" />
    /// </returns>
    public static TriggerKeys FromJsonString(string json)
    {
        return JsonConvert.DeserializeObject<TriggerKeys>(json);
    }
    #endregion
}