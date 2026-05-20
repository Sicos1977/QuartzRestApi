//
// HolidayCalendar.cs
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

    #region Constructors
    /// <summary>
    ///     Parameterless constructor for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    public HolidayCalendar() { }

    /// <summary>
    ///     Takes a <see cref="Quartz.Impl.Calendar.HolidayCalendar" /> and wraps it in a json object.
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
    ///     Returns the <see cref="HolidayCalendar" /> object from the given <paramref name="json" /> string
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