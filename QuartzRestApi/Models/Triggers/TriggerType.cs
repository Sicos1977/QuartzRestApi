using Quartz;
using System.Text.Json.Serialization;

namespace QuartzRestApi.Models.Triggers;

/// <summary>
///     List of supported trigger types for firing a <see cref="IJob" />
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TriggerType
{
    /// <summary>
    ///     A <see cref="ISimpleTrigger" /> that is used to fire a <see cref="IJob" />
    /// </summary>
    Simple,

    /// <summary>
    ///     A <see cref="ICronTrigger" /> that is used to fire a <see cref="IJob" /> based on a cron schedule
    /// </summary>
    Cron,

    /// <summary>
    ///     A <see cref="IRecurrenceTrigger" /> that is used to fire a <see cref="IJob" /> based on a recurrence rule
    /// </summary>  
    Recurrence,

    /// <summary>
    ///     A <see cref="ICalendarIntervalTrigger" /> that is used to fire a <see cref="IJob" /> based upon repeating calendar time intervals
    /// </summary>
    CalendarInterval,

    /// <summary>
    ///     A <see cref="IDailyTimeIntervalTrigger" /> that is used to fire a <see cref="IJob" /> based upon repeating daily time intervals
    /// </summary>
    DailyTimeInterval
}