//
// SimpleTrigger.cs
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
using Quartz.Impl.Triggers;
using System;
using System.Text.Json.Serialization;

namespace QuartzRestApi.Models.Triggers;

/// <summary>
///     DTO for a <see cref="ISimpleTrigger"/>.
///     Represents a trigger that fires a fixed number of times at a fixed interval.
/// </summary>
public class SimpleTrigger : TriggerBase
{
    #region Properties
    /// <summary>
    ///     The number of times this trigger should repeat after the initial fire.
    ///     Use <see cref="SimpleTriggerImpl.RepeatIndefinitely"/> to repeat forever.
    /// </summary>
    [JsonPropertyName("RepeatCount")]
    public int RepeatCount { get; init; }

    /// <summary>
    ///     The time interval at which this trigger should repeat
    /// </summary>
    [JsonPropertyName("RepeatInterval")]
    public TimeSpan RepeatInterval { get; init; }

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
    public SimpleTrigger() { }

    /// <summary>
    ///     Creates a new <see cref="SimpleTrigger"/> DTO from a Quartz <see cref="ISimpleTrigger"/>
    /// </summary>
    /// <param name="trigger">The Quartz simple trigger to map from</param>
    public SimpleTrigger(ISimpleTrigger trigger) : base(trigger)
    {
        RepeatCount = trigger.RepeatCount;
        RepeatInterval = trigger.RepeatInterval;
        TimesTriggered = trigger.TimesTriggered;
    }
    #endregion

    #region ToTrigger
    /// <summary>
    ///     Converts this DTO back to a Quartz <see cref="Quartz.ITrigger"/> with a simple schedule
    /// </summary>
    /// <returns>A configured Quartz <see cref="Quartz.ITrigger"/></returns>
    public override Quartz.ITrigger ToTrigger()
    {
        var builder = ApplyCommonProperties(TriggerBuilder.Create());

        builder = builder.WithSimpleSchedule(x =>
        {
            x.WithInterval(RepeatInterval);

            if (RepeatCount == SimpleTriggerImpl.RepeatIndefinitely)
                x.RepeatForever();
            else
                x.WithRepeatCount(RepeatCount);
        });

        return builder.Build();
    }
    #endregion
}