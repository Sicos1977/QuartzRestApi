//
// Triggers.cs
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Quartz;

namespace QuartzRestApi.Models.Triggers;

/// <summary>
///    A list of <see cref="Quartz.ITrigger" />s
/// </summary>
public class Triggers : List<TriggerBase>
{
    #region Constructor
    /// <summary>
    ///     Needed for JSON deserialization
    /// </summary>
    public Triggers()
    {
    }

    /// <summary>
    ///     Makes this object and sets all it's needed properties
    /// </summary>
    /// <param name="triggers">A <see cref="ReadOnlyCollection{T}" /> of <see cref="Quartz.ITrigger" />s</param>
    public Triggers(IEnumerable<ITrigger> triggers)
    {
        foreach (var trigger in triggers)
        {
            Add(trigger switch
            {
                ISimpleTrigger simpleTrigger => new SimpleTrigger(simpleTrigger),
                ICalendarIntervalTrigger calendarIntervalTrigger => new CalendarIntervalTrigger(calendarIntervalTrigger),
                ICronTrigger cronTrigger => new CronTrigger(cronTrigger),
                IDailyTimeIntervalTrigger dailyTimeIntervalTrigger =>
                    new DailyTimeIntervalTrigger(dailyTimeIntervalTrigger),
                IRecurrenceTrigger recurrenceTrigger => new RecurrenceTrigger(recurrenceTrigger),
                _ => null
            });
        }
    }
    #endregion

    #region ToJsonString
    /// <summary>
    ///     Returns this object as a json string
    /// </summary>
    /// <returns></returns>
    public string ToJsonString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
    #endregion

    #region FromJsonString
    /// <summary>
    ///     Returns the <see cref="Triggers" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="Triggers" />
    /// </returns>
    public static Triggers FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<Triggers>(json);
    }
    #endregion
}