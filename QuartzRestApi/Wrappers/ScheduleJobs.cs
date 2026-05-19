//
// ScheduleJobs.cs
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Quartz;

namespace QuartzRestApi.Wrappers;

/// <summary>
///     Json wrapper to schedule multiple <see cref="IJob" />s each with their own set of <see cref="ITrigger" />s.
///     Maps to <see cref="IScheduler.ScheduleJobs(IReadOnlyDictionary{IJobDetail,IReadOnlyCollection{ITrigger}},bool,System.Threading.CancellationToken)" />.
/// </summary>
public class ScheduleJobs
{
    #region Properties
    /// <summary>
    ///     The list of jobs with their associated triggers to schedule.
    /// </summary>
    [JsonPropertyName("Jobs")]
    public List<JobDetailWithTriggers> Jobs { get; private set; }

    /// <summary>
    ///     When <c>true</c>, any existing jobs or triggers with the same key will be replaced.
    /// </summary>
    [JsonPropertyName("Replace")]
    public bool Replace { get; private set; }
    #endregion

    #region Constructor
    /// <summary>
    ///     Creates this object and sets its needed properties.
    /// </summary>
    /// <param name="jobs">The list of <see cref="JobDetailWithTriggers" /> to schedule</param>
    /// <param name="replace">When <c>true</c>, existing jobs/triggers with the same key are replaced</param>
    public ScheduleJobs(List<JobDetailWithTriggers> jobs, bool replace)
    {
        Jobs = jobs;
        Replace = replace;
    }
    #endregion

    #region ToSchedulerDictionary
    /// <summary>
    ///     Converts the <see cref="Jobs" /> list to the dictionary format expected by
    ///     <see cref="IScheduler.ScheduleJobs(IReadOnlyDictionary{IJobDetail,IReadOnlyCollection{ITrigger}},bool)" />.
    /// </summary>
    public IReadOnlyDictionary<IJobDetail, IReadOnlyCollection<ITrigger>> ToSchedulerDictionary()
    {
        return new ReadOnlyDictionary<IJobDetail, IReadOnlyCollection<ITrigger>>(
            Jobs.ToDictionary(
                j => j.JobDetail.ToJobDetail(),
                j => j.ToReadOnlyTriggerCollection()));
    }
    #endregion

    #region ToJsonString
    /// <summary>
    ///     Returns this object as a json string.
    /// </summary>
    public string ToJsonString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
    #endregion

    #region FromJsonString
    /// <summary>
    ///     Returns a <see cref="ScheduleJobs" /> object from the given <paramref name="json" /> string.
    /// </summary>
    /// <param name="json">The json string</param>
    public static ScheduleJobs FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<ScheduleJobs>(json);
    }
    #endregion
}