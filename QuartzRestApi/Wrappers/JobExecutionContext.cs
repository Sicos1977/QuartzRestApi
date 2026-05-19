using System;
using System.Text.Json.Serialization;
using Quartz;

namespace QuartzRestApi.Wrappers;

/// <summary>
///     Json wrapper for the Quartz <see cref="Quartz.IJobExecutionContext" />
/// </summary>
public class JobExecutionContext(IJobExecutionContext jobExecutionContext)
{
    #region Properties
    /// <summary>
    ///     Get a handle to the <see cref="IScheduler" /> instance that fired the
    ///     <see cref="IJob" />.
    /// </summary>
    [JsonPropertyName("Scheduler")]
    public IScheduler Scheduler { get; private set; } = jobExecutionContext.Scheduler;

    /// <summary>
    ///     Get a handle to the <see cref="ITrigger" /> instance that fired the
    ///     <see cref="IJob" />.
    /// </summary>
    [JsonPropertyName("Trigger")]
    public Trigger Trigger { get; private set; } = new(jobExecutionContext.Trigger);

    /// <summary>
    ///     Get a handle to the <see cref="ICalendar" /> referenced by the <see cref="ITrigger" />
    ///     instance that fired the <see cref="IJob" />.
    /// </summary>
    [JsonPropertyName("Calender")]
    public ICalendar Calendar { get; private set; } = jobExecutionContext.Calendar;

    /// <summary>
    ///     If the <see cref="IJob" /> is being re-executed because of a 'recovery'
    ///     situation, this method will return <see langword="true" />.
    /// </summary>
    [JsonPropertyName("Recovering")]
    public bool Recovering { get; private set; } = jobExecutionContext.Recovering;

    /// <summary>
    ///     Returns the <see cref="TriggerKeys" /> of the originally scheduled and now recovering job.
    /// </summary>
    [JsonPropertyName("RecoveringTriggerKey")]
    public Quartz.TriggerKey RecoveringTriggerKey { get; private set; } = jobExecutionContext.RecoveringTriggerKey;

    /// <summary>
    ///     Gets the refire count.
    /// </summary>
    [JsonPropertyName("RefireCount")]
    public int RefireCount { get; private set; } = jobExecutionContext.RefireCount;

    /// <summary>
    ///     Get the convenience <see cref="JobDataMap" /> of this execution context.
    /// </summary>
    [JsonPropertyName("MergedJobDataMap")]
    public JobDataMap MergedJobDataMap { get; private set; } = jobExecutionContext.MergedJobDataMap;

    /// <summary>
    ///     Get the <see cref="JobDetail" /> associated with the <see cref="IJob" />.
    /// </summary>
    [JsonPropertyName("JobDetail")]
    public JobDetail JobDetail { get; private set; } = new(jobExecutionContext.JobDetail);

    /// <summary>
    ///     Get the instance of the <see cref="IJob" /> that was created for this execution.
    /// </summary>
    [JsonPropertyName("JobInstance")]
    public string JobInstance { get; private set; } = jobExecutionContext.JobInstance.ToString();

    /// <summary>
    ///     The actual time the trigger fired.
    /// </summary>
    [JsonPropertyName("FireTimeUtc")]
    public DateTimeOffset FireTimeUtc { get; private set; } = jobExecutionContext.FireTimeUtc;

    /// <summary>
    ///     The scheduled time the trigger fired for.
    /// </summary>
    [JsonPropertyName("ScheduledFireTimeUtc")]
    public DateTimeOffset? ScheduledFireTimeUtc { get; private set; } = jobExecutionContext.ScheduledFireTimeUtc;

    /// <summary>
    ///     Gets the previous fire time.
    /// </summary>
    [JsonPropertyName("PreviousFireTimeUtc")]
    public DateTimeOffset? PreviousFireTimeUtc { get; private set; } = jobExecutionContext.PreviousFireTimeUtc;

    /// <summary>
    ///     Gets the next fire time.
    /// </summary>
    [JsonPropertyName("NextFireTimeUtc")]
    public DateTimeOffset? NextFireTimeUtc { get; private set; } = jobExecutionContext.NextFireTimeUtc;

    /// <summary>
    ///     Get the unique id that identifies this particular firing instance of the trigger.
    /// </summary>
    [JsonPropertyName("FireInstanceId")]
    public string FireInstanceId { get; private set; } = jobExecutionContext.FireInstanceId;

    /// <summary>
    ///     The amount of time the job ran for.
    /// </summary>
    [JsonPropertyName("JobRunTime")]
    public TimeSpan JobRunTime { get; private set; } = jobExecutionContext.JobRunTime;
    #endregion
}