//
// BaseConverter.cs
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace QuartzRestApi.Models.Calendars;

/// <summary>
///    A json converter for the <see cref="BaseCalendar" />
/// </summary>
internal class BaseConverter : JsonConverter
{
    #region Properties
    /// <summary>
    ///     Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <param name="objectType">The type of the object to check.</param>
    /// <returns>True if this instance can convert the specified object type; otherwise, false.</returns>
    public override bool CanConvert(Type objectType) => objectType == typeof(BaseCalendar);

    /// <summary>
    ///    Gets a value indicating whether this <see cref="JsonConverter" /> can write JSON.
    /// </summary>
    public override bool CanWrite => false;
    #endregion

    #region ReadJson
    /// <summary>
    ///     Reads the JSON representation of the object and deserializes it into the appropriate <see cref="BaseCalendar" /> subclass based on the "Type" property in the JSON.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader" /> to read from.</param>
    /// <param name="objectType">The type of the object to deserialize.</param>
    /// <param name="existingValue">The existing value of the object being deserialized.</param>
    /// <param name="serializer">The <see cref="JsonSerializer" /> to use for deserialization.</param>
    /// <returns>The deserialized <see cref="BaseCalendar" /> object.</returns>
    /// <exception cref="JsonSerializationException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        var typeToken = obj["Type"] ?? throw new JsonSerializationException("Missing 'Type' property on calendar JSON.");
        var calendarType = (CalendarType)Enum.Parse(typeof(CalendarType), typeToken.Value<string>(), true);

        BaseCalendar result = calendarType switch
        {
            CalendarType.Cron => new CronCalendar(),
            CalendarType.Daily => new DailyCalendar(),
            CalendarType.Weekly => new WeeklyCalendar(),
            CalendarType.Monthly => new MonthlyCalendar(),
            CalendarType.Annual => new AnnualCalendar(),
            CalendarType.Holiday => new HolidayCalendar(),
            _ => throw new NotSupportedException($"Unknown CalendarType: {calendarType}")
        };

        // Populate all properties (bypasses the converter for the concrete type)
        using var subReader = obj.CreateReader();
        var innerSerializer = new JsonSerializer();
        
        foreach (var converter in serializer.Converters)
            if (converter is not BaseConverter)
                innerSerializer.Converters.Add(converter);

        innerSerializer.Populate(subReader, result);

        return result;
    }
    #endregion

    #region WriteJson
    /// <summary>
    ///    Writes the JSON representation of the object. This method is not supported and will throw a <see cref="NotSupportedException" />.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    /// <exception cref="NotSupportedException"></exception>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotSupportedException("Use default serialization for writing.");
    #endregion
}