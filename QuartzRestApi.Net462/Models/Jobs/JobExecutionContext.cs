//
// JobExecutionContext.cs
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
using Newtonsoft.Json;
using Quartz;
using QuartzRestApi.Models.Triggers;
using TriggerKey = QuartzRestApi.Models.Triggers.TriggerKey;

namespace QuartzRestApi.Models.Jobs;

/// <summary>
///     Json wrapper for the Quartz <see cref="Quartz.IJobExecutionContext" />
/// </summary>
public class JobExecutionContext
{
    #region Properties
    /// <summary>
    ///     The name of the <see cref="IScheduler" /> instance that fired the <see cref="IJob" />.
    /// </summary>
    [JsonProperty("Scheduler")]
    public string Scheduler { get; set; }

    /// <summary>
    ///     Get a handle to the <see cref="ITrigger" /> instance that fired the
    ///     <see cref="IJob" />.
    /// </summary>
    [JsonProperty("Trigger")]
    public TriggerBase Trigger { get; set; }

    /// <summary>
    ///     The name of the <see cref="ICalendar" /> referenced by the <see cref="ITrigger" />
    ///     instance that fired the <see cref="IJob" />, or <see langword="null" /> when no calendar was used.
    /// </summary>
    [JsonProperty("Calender")]
    public string Calendar { get; set; }

    /// <summary>
    ///     If the <see cref="IJob" /> is being re-executed because of a 'recovery'
    ///     situation, this method will return <see langword="true" />.
    /// </summary>
    [JsonProperty("Recovering")]
    public bool Recovering { get; set; }

    /// <summary>
    ///     Returns the <see cref="TriggerKey" /> of the originally scheduled and now recovering job,
    ///     or <see langword="null" /> when <see cref="Recovering" /> is <see langword="false" />.
    /// </summary>
    [JsonProperty("RecoveringTriggerKey")]
    public TriggerKey RecoveringTriggerKey { get; set; }

    /// <summary>
    ///     Gets the refire count.
    /// </summary>
    [JsonProperty("RefireCount")]
    public int RefireCount { get; set; }

    /// <summary>
    ///     Get the convenience <see cref="JobDataMap" /> of this execution context.
    /// </summary>
    [JsonProperty("MergedJobDataMap")]
    public JobDataMap MergedJobDataMap { get; set; }

    /// <summary>
    ///     Get the <see cref="JobDetail" /> associated with the <see cref="IJob" />.
    /// </summary>
    [JsonProperty("JobDetail")]
    public JobDetail JobDetail { get; set; }

    /// <summary>
    ///     Get the instance of the <see cref="IJob" /> that was created for this execution.
    /// </summary>
    [JsonProperty("JobInstance")]
    public string JobInstance { get; set; }

    /// <summary>
    ///     The actual time the trigger fired.
    /// </summary>
    [JsonProperty("FireTimeUtc")]
    public DateTimeOffset FireTimeUtc { get; set; }

    /// <summary>
    ///     The scheduled time the trigger fired for.
    /// </summary>
    [JsonProperty("ScheduledFireTimeUtc")]
    public DateTimeOffset? ScheduledFireTimeUtc { get; set; }

    /// <summary>
    ///     Gets the previous fire time.
    /// </summary>
    [JsonProperty("PreviousFireTimeUtc")]
    public DateTimeOffset? PreviousFireTimeUtc { get; set; }

    /// <summary>
    ///     Gets the next fire time.
    /// </summary>
    [JsonProperty("NextFireTimeUtc")]
    public DateTimeOffset? NextFireTimeUtc { get; set; }

    /// <summary>
    ///     Get the unique id that identifies this particular firing instance of the trigger.
    /// </summary>
    [JsonProperty("FireInstanceId")]
    public string FireInstanceId { get; set; }

    /// <summary>
    ///     The amount of time the job ran for.
    /// </summary>
    [JsonProperty("JobRunTime")]
    public TimeSpan JobRunTime { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    ///     Parameterless constructor for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    public JobExecutionContext() { }

    /// <summary>
    ///     Creates this object and populates all properties from the given
    ///     <see cref="IJobExecutionContext" />.
    /// </summary>
    /// <param name="jobExecutionContext">
    ///     The Quartz <see cref="IJobExecutionContext" /> that describes the currently
    ///     executing job instance.
    /// </param>
    public JobExecutionContext(IJobExecutionContext jobExecutionContext)
    {
        Scheduler = jobExecutionContext.Scheduler.SchedulerName;

        Trigger = jobExecutionContext.Trigger switch
        {
            ISimpleTrigger simpleTrigger => new SimpleTrigger(simpleTrigger),
            ICalendarIntervalTrigger calendarIntervalTrigger => new CalendarIntervalTrigger(calendarIntervalTrigger),
            ICronTrigger cronTrigger => new CronTrigger(cronTrigger),
            IDailyTimeIntervalTrigger dailyTimeIntervalTrigger => new DailyTimeIntervalTrigger(dailyTimeIntervalTrigger),
            IRecurrenceTrigger recurrenceTrigger => new RecurrenceTrigger(recurrenceTrigger),
            _ => null
        };

        Calendar = jobExecutionContext.Trigger.CalendarName;
        Recovering = jobExecutionContext.Recovering;
        RecoveringTriggerKey = jobExecutionContext.Recovering ? new TriggerKey(jobExecutionContext.RecoveringTriggerKey) : null;
        RefireCount = jobExecutionContext.RefireCount;
        MergedJobDataMap = jobExecutionContext.MergedJobDataMap;
        JobDetail = new JobDetail(jobExecutionContext.JobDetail);
        JobInstance = jobExecutionContext.JobInstance.ToString();
        FireTimeUtc = jobExecutionContext.FireTimeUtc;
        ScheduledFireTimeUtc = jobExecutionContext.ScheduledFireTimeUtc;
        PreviousFireTimeUtc = jobExecutionContext.PreviousFireTimeUtc;
        NextFireTimeUtc = jobExecutionContext.NextFireTimeUtc;
        FireInstanceId = jobExecutionContext.FireInstanceId;
        JobRunTime = jobExecutionContext.JobRunTime;
    }
    #endregion
}