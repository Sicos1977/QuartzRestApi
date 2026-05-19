using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Quartz;

namespace QuartzRestApi.Wrappers.Calendars;

/// <summary>
///     A json wrapper for the <see cref="Quartz.Impl.Calendar.HolidayCalendar" />
/// </summary>
public class HolidayCalendar : BaseCalendar
{
    #region Properties
    /// <summary>
    ///     Returns a collection of dates representing the excluded
    ///     days. Only the month, day and year of the returned dates are
    ///     significant
    /// </summary>
    [JsonPropertyName("ExcludedDates")]
    public List<DateTime> ExcludedDates { get; internal set; }
    #endregion

    #region Constructor
    /// <summary>
    ///     Takes a <see cref="Quartz.Impl.Calendar.HolidayCalendar" /> and wraps it in a json object
    /// </summary>
    /// <param name="holidayCalendar"><see cref="Quartz.Impl.Calendar.HolidayCalendar" /></param>
    public HolidayCalendar(Quartz.Impl.Calendar.HolidayCalendar holidayCalendar) : base(holidayCalendar)
    {
        Type = CalendarType.Holiday;

        foreach (var excludedDate in holidayCalendar.ExcludedDates)
            ExcludedDates.Add(excludedDate);

        TimeZone = holidayCalendar.TimeZone;
    }
    #endregion

    #region ToCalendar
    /// <summary>
    ///     Returns this object as a Quartz <see cref="ICalendar" />
    /// </summary>
    /// <returns></returns>
    public override ICalendar ToCalendar()
    {
        var result = new Quartz.Impl.Calendar.HolidayCalendar
        {
            Description = Description,
            TimeZone = TimeZone
        };

        foreach (var day in ExcludedDates)
            result.AddExcludedDate(day);

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
    ///     Returns the <see cref="json" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="HolidayCalendar" />
    /// </returns>
    public new static HolidayCalendar FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<HolidayCalendar>(json);
    }
    #endregion
}