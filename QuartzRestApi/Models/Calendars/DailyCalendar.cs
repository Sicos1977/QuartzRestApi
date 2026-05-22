//
// DailyCalendar.cs
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
using System.Text.Json;
using System.Text.Json.Serialization;
using Quartz;

namespace QuartzRestApi.Models.Calendars;

/// <summary>
///     A json wrapper for the <see cref="Quartz.Impl.Calendar.DailyCalendar" />
/// </summary>
public class DailyCalendar : BaseCalendar
{
    #region Properties
    /// <summary>
    ///     Must be in the format &quot;HH:MM[:SS[:mmm]]&quot; where:
    ///     <ul>
    ///         <li>
    ///             HH is the hour of the specified time. The hour should be
    ///             specified using military (24-hour) time and must be in the range
    ///             0 to 23.
    ///         </li>
    ///         <li>
    ///             MM is the minute of the specified time and must be in the range
    ///             0 to 59.
    ///         </li>
    ///         <li>
    ///             SS is the second of the specified time and must be in the range
    ///             0 to 59.
    ///         </li>
    ///         <li>
    ///             mmm is the millisecond of the specified time and must be in the
    ///             range 0 to 999.
    ///         </li>
    ///         <li>items enclosed in brackets ('[', ']') are optional.</li>
    ///         <li>
    ///             The time range starting time must be before the time range ending
    ///             time. Note this means that a time range may not cross daily
    ///             boundaries (10PM - 2AM)
    ///         </li>
    ///     </ul>
    /// </summary>
    /// <remarks>
    ///     Only used when the <see cref="Type" /> is <see cref="CalendarType.Daily" />
    /// </remarks>
    [JsonPropertyName("RangeStartingTime")]
    public string RangeStartingTime { get; internal set; }

    /// <summary>
    ///     Must be in the format &quot;HH:MM[:SS[:mmm]]&quot; where:
    ///     <ul>
    ///         <li>
    ///             HH is the hour of the specified time. The hour should be
    ///             specified using military (24-hour) time and must be in the range
    ///             0 to 23.
    ///         </li>
    ///         <li>
    ///             MM is the minute of the specified time and must be in the range
    ///             0 to 59.
    ///         </li>
    ///         <li>
    ///             SS is the second of the specified time and must be in the range
    ///             0 to 59.
    ///         </li>
    ///         <li>
    ///             mmm is the millisecond of the specified time and must be in the
    ///             range 0 to 999.
    ///         </li>
    ///         <li>items enclosed in brackets ('[', ']') are optional.</li>
    ///         <li>
    ///             The time range starting time must be before the time range ending
    ///             time. Note this means that a time range may not cross daily
    ///             boundaries (10PM - 2AM)
    ///         </li>
    ///     </ul>
    /// </summary>
    [JsonPropertyName("RangeEndingTime")]
    public string RangeEndingTime { get; internal set; }

    /// <summary>
    /// Indicates whether the time range represents an inverted time range (see
    /// class description).
    /// </summary>
    /// <value><c>true</c> if invert time range; otherwise, <c>false</c>.</value>
    [JsonPropertyName("InvertTimeRange")]
    public bool InvertTimeRange { get; internal set; }
    #endregion

    #region Constructors
    /// <summary>
    ///     Parameterless constructor for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    public DailyCalendar() { }

    /// <summary>
    ///     Takes a <see cref="Quartz.Impl.Calendar.DailyCalendar" /> and wraps it in a json object.
    /// </summary>
    /// <param name="dailyCalendar"><see cref="Quartz.Impl.Calendar.DailyCalendar" /></param>
    public DailyCalendar(Quartz.Impl.Calendar.DailyCalendar dailyCalendar) : base(dailyCalendar)
    {
        Type = CalendarType.Daily;
        RangeStartingTime = dailyCalendar.RangeStartingTime;
        RangeEndingTime = dailyCalendar.RangeEndingTime;
        InvertTimeRange = dailyCalendar.InvertTimeRange;
        TimeZone = dailyCalendar.TimeZone;
    }
    #endregion

    #region ToCalendar
    /// <summary>
    ///     Returns this object as a Quartz <see cref="Quartz.ICalendar" />
    /// </summary>
    /// <returns></returns>
    public override ICalendar ToCalendar()
    {
        var result = new Quartz.Impl.Calendar.DailyCalendar(RangeStartingTime, RangeEndingTime)
        {
            Description = Description,
            TimeZone = TimeZone,
            InvertTimeRange = InvertTimeRange
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
    ///     Returns the <see cref="DailyCalendar" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="DailyCalendar" />
    /// </returns>
    public new static DailyCalendar FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<DailyCalendar>(json);
    }
    #endregion
}