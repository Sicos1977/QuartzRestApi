//
// AnnualCalendar.cs
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
using Newtonsoft.Json;
using Quartz;

namespace QuartzRestApi.Models.Calendars;

/// <summary>
///     A json wrapper for the <see cref="Quartz.Impl.Calendar.AnnualCalendar" />
/// </summary>
public class AnnualCalendar : BaseCalendar
{
    #region Properties
    /// <summary>
    ///     Returns a collection of days of the year that are excluded
    /// </summary>
    [JsonProperty("DaysExcluded")]
    public List<DateTime> DaysExcluded { get; internal set; } = [];
    #endregion

    #region Constructors
    /// <summary>
    ///     Parameterless constructor for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    public AnnualCalendar() { }

    /// <summary>
    ///     Takes a <see cref="Quartz.Impl.Calendar.AnnualCalendar" /> and wraps it in a json object.
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
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
    #endregion

    #region FromJsonString
    /// <summary>
    ///     Returns the <see cref="AnnualCalendar" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="AnnualCalendar" />
    /// </returns>
    public new static AnnualCalendar FromJsonString(string json)
    {
        return JsonConvert.DeserializeObject<AnnualCalendar>(json);
    }
    #endregion
}