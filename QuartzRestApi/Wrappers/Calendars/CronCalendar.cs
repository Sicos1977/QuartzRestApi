//
// CronCalendar.cs
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