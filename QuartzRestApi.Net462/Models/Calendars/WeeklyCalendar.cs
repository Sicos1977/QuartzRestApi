//
// WeeklyCalendar.cs
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

namespace QuartzRestApi.Models.Calendars
{
    /// <summary>JSON wrapper for <see cref="Quartz.Impl.Calendar.WeeklyCalendar"/>.</summary>
    public class WeeklyCalendar : BaseCalendar
    {
        [JsonProperty("DaysExcluded")]
        public List<bool> DaysExcluded { get; set; } = new List<bool>();

        public WeeklyCalendar() { Type = CalendarType.Weekly; }

        public WeeklyCalendar(Quartz.Impl.Calendar.WeeklyCalendar cal) : base(cal)
        {
            Type = CalendarType.Weekly;
            TimeZone = cal.TimeZone;
            foreach (var day in cal.DaysExcluded)
                DaysExcluded.Add(day);
        }

        public override ICalendar ToCalendar()
        {
            var result = new Quartz.Impl.Calendar.WeeklyCalendar
            {
                Description = Description,
                TimeZone = TimeZone
            };
            for (var i = 0; i < DaysExcluded.Count; i++)
                result.SetDayExcluded((DayOfWeek)(i + 1), DaysExcluded[i]);
            return result;
        }

        public new string ToJsonString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        public static WeeklyCalendar FromJsonString(string json) => JsonConvert.DeserializeObject<WeeklyCalendar>(json);
    }
}
