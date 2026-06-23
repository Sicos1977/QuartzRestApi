//
// BaseCalendar.cs
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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Quartz;

namespace QuartzRestApi.Models.Calendars;

/// <summary>
///     The base calendar for all calendars
/// </summary>
[JsonConverter(typeof(BaseConverter))]
public abstract class BaseCalendar
{
    #region Properties
    /// <summary>
    ///     The name of the calendar
    /// </summary>
    [JsonProperty("Name")]
    public string Name { get; set; }

    /// <summary>
    ///     The <see cref="CalendarType"/> of the calendar
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("Type")]
    public CalendarType Type { get; internal set; }

    /// <summary>
    ///     The description of the calendar
    /// </summary>
    [JsonProperty("Description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; internal set; }

    /// <summary>
    ///     The time zone of the calendar
    /// </summary>
    [JsonProperty("TimeZone", NullValueHandling = NullValueHandling.Ignore)]
    public TimeZoneInfo TimeZone { get; internal set; }

    /// <summary>
    ///     The base of the calendar
    /// </summary>
    [JsonProperty("CalendarBase", NullValueHandling = NullValueHandling.Ignore)]
    public string CalendarBase { get; internal set; }

    /// <summary>
    ///     If set to <c>true</c> [replace].
    /// </summary>
    [JsonProperty("Replace", NullValueHandling = NullValueHandling.Ignore)]
    public bool Replace { get; set; }

    /// <summary>
    ///     Whether to update existing triggers that
    ///     referenced the already existing calendar so that they are 'correct'
    ///     based on the new trigger.
    /// </summary>
    [JsonProperty("UpdateTriggers", NullValueHandling = NullValueHandling.Ignore)]
    public bool UpdateTriggers { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    ///     Parameterless constructor for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    protected BaseCalendar() { }

    /// <summary>
    ///     Creates this object and sets it's needed properties from the given <see cref="ICalendar" />.
    /// </summary>
    /// <param name="calendar">The Quartz <see cref="ICalendar" /> to wrap.</param>
    protected BaseCalendar(ICalendar calendar)
    {
        Description = calendar?.Description;
    }
    #endregion

    #region ToJsonString
    /// <summary>
    ///     Returns this object as a json string
    /// </summary>
    /// <returns></returns>
    public string ToJsonString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
    #endregion

    #region FromJsonString
    /// <summary>
    ///     Returns the <see cref="BaseCalendar" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="BaseCalendar" />
    /// </returns>
    public static BaseCalendar FromJsonString(string json)
    {
        return JsonConvert.DeserializeObject<BaseCalendar>(json);
    }
    #endregion

    #region ToCalendar
    /// <summary>
    ///     Converts the current instance to an equivalent calendar representation.
    /// </summary>
    /// <returns>
    ///     An object that implements the <see cref="ICalendar"/> interface and represents the calendar equivalent of the
    /// current instance.
    /// </returns>
    public abstract ICalendar ToCalendar();
    #endregion
}