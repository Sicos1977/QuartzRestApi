//
// CalendarIntervalTrigger.cs
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

using Quartz;
using System;
using Newtonsoft.Json;

namespace QuartzRestApi.Models.Triggers;

/// <summary>
///     DTO for a <see cref="ICalendarIntervalTrigger"/>.
///     Represents a trigger that fires at a fixed calendar-based interval, e.g. every 2 weeks or every 3 months.
/// </summary>
public class CalendarIntervalTrigger : TriggerBase
{
    #region Properties
    /// <summary>
    ///     The interval at which this trigger repeats, expressed in units of <see cref="RepeatIntervalUnit"/>
    /// </summary>
    [JsonProperty("RepeatInterval")]
    public int RepeatInterval { get; set; }

    /// <summary>
    ///     The unit of the repeat interval, e.g. <c>Day</c>, <c>Week</c> or <c>Month</c>
    /// </summary>
    [JsonProperty("RepeatIntervalUnit")]
    public RepeatIntervalUnit RepeatIntervalUnit { get; set; }

    /// <summary>
    ///     The time zone for which this trigger will be resolved
    /// </summary>
    [JsonProperty("TimeZone")]
    public TimeZoneInfo TimeZone { get; set; }

    /// <summary>
    ///     When <c>true</c>, the trigger preserves the configured hour of day across daylight saving time transitions
    /// </summary>
    [JsonProperty("PreserveHourOfDayAcrossDaylightSavings")]
    public bool PreserveHourOfDayAcrossDaylightSavings { get; set; }

    /// <summary>
    ///     When <c>true</c>, the trigger skips firing on days where the configured hour does not exist
    ///     due to a daylight saving time transition (e.g. the clock jumps from 2:00 to 3:00)
    /// </summary>
    [JsonProperty("SkipDayIfHourDoesNotExist")]
    public bool SkipDayIfHourDoesNotExist { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    ///     Parameterless constructor for JSON deserialization
    /// </summary>
    [JsonConstructor]
    public CalendarIntervalTrigger() { }

    /// <summary>
    ///     Creates a new <see cref="CalendarIntervalTrigger"/> DTO from a Quartz <see cref="ICalendarIntervalTrigger"/>
    /// </summary>
    /// <param name="trigger">The Quartz calendar interval trigger to map from</param>
    public CalendarIntervalTrigger(ICalendarIntervalTrigger trigger) : base(trigger)
    {
        RepeatInterval = trigger.RepeatInterval;
        RepeatIntervalUnit = (RepeatIntervalUnit)trigger.RepeatIntervalUnit;
        TimeZone = trigger.TimeZone;
        PreserveHourOfDayAcrossDaylightSavings = trigger.PreserveHourOfDayAcrossDaylightSavings;
        SkipDayIfHourDoesNotExist = trigger.SkipDayIfHourDoesNotExist;
    }
    #endregion

    #region ToTrigger
    /// <summary>
    ///     Converts this DTO back to a Quartz <see cref="ITrigger"/> with a calendar interval schedule
    /// </summary>
    /// <returns>A configured Quartz <see cref="ITrigger"/></returns>
    public override ITrigger ToTrigger()
    {
        var builder = ApplyCommonProperties(TriggerBuilder.Create());

        builder = builder.WithCalendarIntervalSchedule(x =>
        {
            x.WithInterval(RepeatInterval, (IntervalUnit)RepeatIntervalUnit);

            if (TimeZone != null)
                x.InTimeZone(TimeZone);

            x.PreserveHourOfDayAcrossDaylightSavings(PreserveHourOfDayAcrossDaylightSavings);
            x.SkipDayIfHourDoesNotExist(SkipDayIfHourDoesNotExist);
        });

        return builder.Build();
    }
    #endregion
}