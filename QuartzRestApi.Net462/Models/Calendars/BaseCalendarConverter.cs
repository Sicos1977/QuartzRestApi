//
// BaseCalendarConverter.cs
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
using Newtonsoft.Json.Linq;
using Quartz.Impl.Calendar;
using BaseCalendar = Quartz.Impl.Calendar.BaseCalendar;

namespace QuartzRestApi.Models.Calendars
{
    /// <summary>Newtonsoft.Json converter that dispatches to the correct concrete calendar subtype.</summary>
    internal class BaseCalendarConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(BaseCalendar);

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var typeToken = obj["Type"] ?? throw new JsonSerializationException("Missing 'Type' property on calendar JSON.");
            var calendarType = (CalendarType)Enum.Parse(typeof(CalendarType), typeToken.Value<string>(), true);

            BaseCalendar result;
            switch (calendarType)
            {
                case CalendarType.Cron:    result = new CronCalendar();    break;
                case CalendarType.Daily:   result = new DailyCalendar();   break;
                case CalendarType.Weekly:  result = new WeeklyCalendar();  break;
                case CalendarType.Monthly: result = new MonthlyCalendar(); break;
                case CalendarType.Annual:  result = new AnnualCalendar();  break;
                case CalendarType.Holiday: result = new HolidayCalendar(); break;
                default: throw new NotSupportedException($"Unknown CalendarType: {calendarType}");
            }

            // Populate all properties (bypasses the converter for the concrete type)
            using (var subReader = obj.CreateReader())
            {
                var innerSerializer = new JsonSerializer();
                foreach (var converter in serializer.Converters)
                    if (!(converter is BaseCalendarConverter))
                        innerSerializer.Converters.Add(converter);

                innerSerializer.Populate(subReader, result);
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => throw new NotSupportedException("Use default serialization for writing.");
    }
}
