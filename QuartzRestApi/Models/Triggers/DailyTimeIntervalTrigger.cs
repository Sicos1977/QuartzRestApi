//
// DailyTimeIntervalTrigger.cs
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
using System.Text.Json.Serialization;

namespace QuartzRestApi.Models.Triggers;

/// <summary>
///     DTO for a <see cref="IDailyTimeIntervalTrigger"/>.
///     Represents a trigger that fires at a fixed interval within a configurable daily time window.
/// </summary>
public class DailyTimeIntervalTrigger : TriggerBase
{
    #region Properties
    /// <summary>
    ///     The number of times this trigger should repeat.
    ///     Set to <c>-1</c> to repeat indefinitely within the daily time window.
    /// </summary>
    [JsonPropertyName("RepeatCount")]
    public int RepeatCount { get; init; }

    /// <summary>
    ///     The interval at which this trigger repeats, expressed in units of <see cref="RepeatIntervalUnit"/>
    /// </summary>
    [JsonPropertyName("RepeatInterval")]
    public int RepeatInterval { get; init; }

    /// <summary>
    ///     The unit of the repeat interval, e.g. <c>Second</c>, <c>Minute</c> or <c>Hour</c>
    /// </summary>
    [JsonPropertyName("RepeatIntervalUnit")]
    public RepeatIntervalUnit RepeatIntervalUnit { get; init; }

    /// <summary>
    ///     The time of day at which this trigger starts firing each day
    /// </summary>
    [JsonPropertyName("StartTimeOfDay")]
    public TimeOfDay StartTimeOfDay { get; init; }

    /// <summary>
    ///     The time of day at which this trigger stops firing each day
    /// </summary>
    [JsonPropertyName("EndTimeOfDay")]
    public TimeOfDay EndTimeOfDay { get; init; }

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
    public DailyTimeIntervalTrigger() { }

    /// <summary>
    ///     Creates a new <see cref="DailyTimeIntervalTrigger"/> DTO from a Quartz <see cref="IDailyTimeIntervalTrigger"/>
    /// </summary>
    /// <param name="trigger">The Quartz daily time interval trigger to map from</param>
    public DailyTimeIntervalTrigger(IDailyTimeIntervalTrigger trigger) : base(trigger)
    {
        RepeatCount = trigger.RepeatCount;
        RepeatInterval = trigger.RepeatInterval;
        RepeatIntervalUnit = (RepeatIntervalUnit)trigger.RepeatIntervalUnit;
        StartTimeOfDay = trigger.StartTimeOfDay;
        EndTimeOfDay = trigger.EndTimeOfDay;
        TimesTriggered = trigger.TimesTriggered;
    }
    #endregion

    #region ToTrigger
    /// <summary>
    ///     Converts this DTO back to a Quartz <see cref="Quartz.ITrigger"/> with a daily time interval schedule
    /// </summary>
    /// <returns>A configured Quartz <see cref="Quartz.ITrigger"/></returns>
    public override ITrigger ToTrigger()
    {
        var builder = ApplyCommonProperties(TriggerBuilder.Create());

        builder = builder.WithDailyTimeIntervalSchedule(x =>
        {
            x.WithInterval(RepeatInterval, (IntervalUnit)RepeatIntervalUnit);

            if (RepeatCount > 0)
                x.WithRepeatCount(RepeatCount);

            if (StartTimeOfDay != null)
                x.StartingDailyAt(StartTimeOfDay);

            if (EndTimeOfDay != null)
                x.EndingDailyAt(EndTimeOfDay);
        });

        return builder.Build();
    }
    #endregion
}