//
// RescheduleJob.cs
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

using Newtonsoft.Json;
using QuartzRestApi.Models.Triggers;

namespace QuartzRestApi.Models.Jobs;

/// <summary>
///     Class used to read or create json to reschedule a job
/// </summary>
public class RescheduleJob
{
    #region Properties
    /// <summary>
    ///     The current trigger key
    /// </summary>
    [JsonProperty("CurrentTriggerKey")]
    public TriggerKey CurrentTriggerKey { get; set; }

    /// <summary>
    ///     The new trigger
    /// </summary>
    [JsonProperty("NewTrigger")]
    public TriggerBase Trigger { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    ///     Creates this object and sets its needed properties
    /// </summary>
    /// <param name="currentTriggerKey">The current <see cref="TriggerKey" /></param>
    /// <param name="newTrigger">The new <see cref="TriggerBase" /></param>
    public RescheduleJob(TriggerKey currentTriggerKey, TriggerBase newTrigger)
    {
        CurrentTriggerKey = currentTriggerKey;
        Trigger = newTrigger;
    }

    /// <summary>
    ///     Creates this object for JSON deserialization
    /// </summary>
    public RescheduleJob()
    {
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
    ///     Returns the <see cref="RescheduleJob" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="RescheduleJob" />
    /// </returns>
    public static RescheduleJob FromJsonString(string json)
    {
        return JsonConvert.DeserializeObject<RescheduleJob>(json);
    }
    #endregion
}