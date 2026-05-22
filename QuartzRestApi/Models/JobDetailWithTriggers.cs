//
// JobDetailWithTriggers.cs
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
using System.Text.Json;
using System.Text.Json.Serialization;
using Quartz;

namespace QuartzRestApi.Models;

/// <summary>
///     Json wrapper to create a <see cref="Quartz.IJob" /> with a list of <see cref="Quartz.ITrigger" />s
/// </summary>
public class JobDetailWithTriggers
{
    #region Properties
    /// <summary>
    ///     <see cref="JobDetail" />
    /// </summary>
    [JsonPropertyName("JobDetail")]
    public JobDetail JobDetail { get; private set; }

    /// <summary>
    ///     A list with related <see cref="Trigger" />'s
    /// </summary>
    [JsonPropertyName("Triggers")]
    public List<Trigger> Triggers { get; private set; }

    /// <summary>
    ///     <c>true</c> when the job needs to be replaced
    /// </summary>
    [JsonPropertyName("Replace")]
    public bool Replace { get; private set; }
    #endregion

    #region Constructor
    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="jobDetail">
    ///     <see cref="JobDetail" />
    /// </param>
    /// <param name="triggers">A list with related <see cref="Trigger" />'s</param>
    public JobDetailWithTriggers(JobDetail jobDetail, List<Trigger> triggers)
    {
        JobDetail = jobDetail;
        Triggers = triggers;
    }
    #endregion

    #region ToReadOnlyTriggerCollection
    /// <summary>
    ///     Returns the <see cref="Triggers" /> as a <see cref="IReadOnlyCollection{T}" />
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<ITrigger> ToReadOnlyTriggerCollection()
    {
        return Triggers.Select(trigger => trigger.ToTrigger()).ToList();
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
    ///     Returns the <see cref="JobDetailWithTriggers" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="Trigger" />
    /// </returns>
    public static JobDetailWithTriggers FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<JobDetailWithTriggers>(json);
    }
    #endregion
}