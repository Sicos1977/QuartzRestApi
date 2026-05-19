using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Quartz;

namespace QuartzRestApi.Wrappers.Calendars;

/// <summary>
///     A json wrapper for the <see cref="Quartz.Impl.Calendar.AnnualCalendar" />
/// </summary>
public class AnnualCalendar : BaseCalendar
{
    #region Properties
    /// <summary>
    ///     Returns a collection of days of the year that are excluded
    /// </summary>
    [JsonPropertyName("DaysExcluded")]
    public List<DateTime> DaysExcluded { get; internal set; } = [];
    #endregion

    #region Constructor
    /// <summary>
    ///     Takes a <see cref="Quartz.Impl.Calendar.AnnualCalendar" /> and wraps it in a json object
    /// </summary>
    /// <param name="annualCalendar"><see cref="Quartz.Impl.Calendar.AnnualCalendar" /></param>
    public AnnualCalendar(Quartz.Impl.Calendar.AnnualCalendar annualCalendar) : base(annualCalendar)
    {
        foreach(var day in annualCalendar.DaysExcluded)
            DaysExcluded.Add(day);
    }
    #endregion

    #region ToCalendar
    /// <summary>
    ///     Returns this object as a Quartz <see cref="ICalendar" />
    /// </summary>
    /// <returns></returns>
    public override ICalendar ToCalendar()
    {
        var result = new Quartz.Impl.Calendar.AnnualCalendar
        {
            Description = Description,
            TimeZone = TimeZone
        };

        foreach (var day in DaysExcluded)
            result.SetDayExcluded(day, true);

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
    ///     Returns the <see cref="AnnualCalendar" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="Trigger" />
    /// </returns>
    public new static AnnualCalendar FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<AnnualCalendar>(json);
    }
    #endregion
}