//
// JobKeyWithDataMap.cs
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
using Quartz;

namespace QuartzRestApi.Models.Jobs;

/// <summary>
///     Json wrapper to create a <see cref="Quartz.JobKey" /> with a <see cref="Quartz.JobDataMap" />
/// </summary>
public class JobKeyWithDataMap
{
    #region Properties
    /// <summary>
    ///     The <see cref="JobKey" />
    /// </summary>
    [JsonProperty("JobKey")]
    public JobKey JobKey { get; set; }

    /// <summary>
    ///     The <see cref="JobDataMap" />
    /// </summary>
    [JsonProperty("JobDataMap")]
    public JobDataMap JobDataMap { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    ///     Parameterless constructor for JSON deserialization
    /// </summary>
    [JsonConstructor]
    public JobKeyWithDataMap() { }

    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="jobKey">The <see cref="JobKey" /></param>
    /// <param name="jobDataMap">The <see cref="JobDataMap" /></param>
    public JobKeyWithDataMap(JobKey jobKey, JobDataMap jobDataMap)
    {
        JobKey = jobKey;
        JobDataMap = jobDataMap;
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
    ///     Returns the <see cref="JobKeyWithDataMap" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="JobKeyWithDataMap" />
    /// </returns>
    public static JobKeyWithDataMap FromJsonString(string json)
    {
        return JsonConvert.DeserializeObject<JobKeyWithDataMap>(json);
    }
    #endregion
}