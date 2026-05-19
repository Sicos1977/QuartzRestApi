using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Quartz;

namespace QuartzRestApi.Wrappers.Calendars;

/// <summary>
///     A json wrapper for the <see cref="Quartz.Impl.Calendar.CronCalendar" />
/// </summary>
public class CronCalendar : BaseCalendar
{
    #region Properties
    /// <summary>
    ///     The cron expression
    /// </summary>
    /// <remarks>
    ///     Only used when the <see cref="Type" /> is <see cref="CalendarType.Cron" />
    /// </remarks>
    [JsonPropertyName("CronExpression")]
    public string CronExpression { get; internal set; }
    #endregion

    #region Constructor
    /// <summary>
    ///     Takes a <see cref="Quartz.Impl.Calendar.CronCalendar" /> and wraps it in a json object
    /// </summary>
    /// <param name="cronCalendar"><see cref="Quartz.Impl.Calendar.CronCalendar" /></param>
    public CronCalendar(Quartz.Impl.Calendar.CronCalendar cronCalendar) : base(cronCalendar)
    {
        Type = CalendarType.Cron;
        CronExpression = cronCalendar?.CronExpression.CronExpressionString;
        TimeZone = cronCalendar?.TimeZone;
    }
    #endregion

    #region ToCalendar
    /// <summary>
    ///     Returns this object as a Quartz <see cref="ICalendar" />
    /// </summary>
    /// <returns></returns>
    public override ICalendar ToCalendar()
    {
        var result = new Quartz.Impl.Calendar.CronCalendar(CronExpression)
        {
            Description = Description,
            TimeZone = TimeZone
        };

        return result;
    }
    #endregion

    #region ToJsonString
    /// <summary>
    ///     Returns this object as a json string
    /// </summary>
    /// <returns></returns>
    public new string ToJsonString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
    #endregion

    #region FromJsonString
    /// <summary>
    ///     Returns the <see cref="CronCalendar" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="CronCalendar" />
    /// </returns>
    public new static CronCalendar FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<CronCalendar>(json);
    }
    #endregion
}