//
// RecurrenceTrigger.cs
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
using System.Text.Json.Serialization;

namespace QuartzRestApi.Models.Triggers;

/// <summary>
///     DTO for a <see cref="IRecurrenceTrigger"/>.
///     Represents a trigger that fires according to an RFC 5545 RRULE recurrence rule.
/// </summary>
public class RecurrenceTrigger : TriggerBase
{
    #region Properties
    /// <summary>
    ///     The RFC 5545 RRULE string that defines the recurrence schedule,
    ///     e.g. <c>"FREQ=WEEKLY;INTERVAL=2;BYDAY=MO,WE,FR"</c>
    /// </summary>
    [JsonPropertyName("RecurrenceRule")]
    public string RecurrenceRule { get; init; }

    /// <summary>
    ///     The time zone for which the <see cref="RecurrenceRule"/> will be resolved
    /// </summary>
    [JsonPropertyName("TimeZone")]
    public TimeZoneInfo TimeZone { get; init; }

    /// <summary>
    ///     The number of times this trigger has already fired
    /// </summary>
    [JsonPropertyName("TimesTriggered")]
    public int TimesTriggered { get; init; }
    #endregion

    #region Constructors
    /// <summary>
    ///     Parameterless constructor for JSON deserialization
    /// </summary>
    [JsonConstructor]
    public RecurrenceTrigger() { }

    /// <summary>
    ///     Creates a new <see cref="RecurrenceTrigger"/> DTO from a Quartz <see cref="IRecurrenceTrigger"/>
    /// </summary>
    /// <param name="trigger">The Quartz recurrence trigger to map from</param>
    public RecurrenceTrigger(IRecurrenceTrigger trigger) : base(trigger)
    {
        RecurrenceRule = trigger.RecurrenceRule;
        TimeZone = trigger.TimeZone;
        TimesTriggered = trigger.TimesTriggered;
    }
    #endregion

    #region ToTrigger
    /// <summary>
    ///     Converts this DTO back to a Quartz <see cref="ITrigger"/> with a recurrence schedule
    /// </summary>
    /// <returns>A configured Quartz <see cref="ITrigger"/></returns>
    public override ITrigger ToTrigger()
    {
        var builder = ApplyCommonProperties(TriggerBuilder.Create());

        builder = builder.WithRecurrenceSchedule(RecurrenceRule, x =>
        {
            if (TimeZone != null)
                x.InTimeZone(TimeZone);
        });

        return builder.Build();
    }
    #endregion
}