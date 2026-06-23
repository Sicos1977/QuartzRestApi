using Quartz;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable UnusedMember.Global

namespace QuartzRestApi.Models.Triggers;

/// <summary>
///     The time unit on which the interval applies for a <see cref="ICalendarIntervalTrigger" />   
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum RepeatIntervalUnit
{
    /// <summary>
    ///    The interval is in milliseconds.
    /// </summary>
    Millisecond,

    /// <summary>
    ///     The interval is in seconds.
    /// </summary>
    Second,

    /// <summary>
    ///     The interval is in minutes.
    /// </summary>
    Minute,

    /// <summary>
    ///     The interval is in hours.
    /// </summary>
    Hour,

    /// <summary>
    ///     The interval is in days.
    /// </summary>
    Day,

    /// <summary>
    ///     The interval is in weeks.
    /// </summary>
    Week,

    /// <summary>
    ///     The interval is in months.
    /// </summary>
    Month,

    /// <summary>
    ///     The interval is in years.
    /// </summary>
    Year
}