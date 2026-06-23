//
// TriggerBaseConverter.cs
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

namespace QuartzRestApi.Models.Triggers;

/// <summary>
///    Custom JSON converter for <see cref="TriggerBase"/> and its derived types.
/// </summary>
internal class TriggerBaseConverter : JsonConverter
{
    #region CanConvert
    /// <summary>
    ///     Determines whether this converter can convert the specified object type. It returns true if the object type is <see cref="TriggerBase"/> or any of its derived types.
    /// </summary>
    /// <param name="objectType">The type of the object to check.</param>
    /// <returns>True if the object type is <see cref="TriggerBase"/> or any of its derived types; otherwise, false.</returns>
    public override bool CanConvert(Type objectType) => objectType == typeof(TriggerBase);
    #endregion

    #region ReadJson
    /// <summary>
    ///     Reads the JSON representation of the object and deserializes it into the appropriate derived type of <see cref="TriggerBase"/> based on the "$type" discriminator.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">The type of the object to deserialize.</param>
    /// <param name="existingValue">The existing value of the object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The deserialized <see cref="TriggerBase"/> object.</returns>
    /// <exception cref="JsonSerializationException"></exception>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["$type"]?.Value<string>() 
                   ?? throw new JsonSerializationException("Missing '$type' discriminator");

        TriggerBase trigger = type switch
        {
            "cron"             => new CronTrigger(),
            "simple"           => new SimpleTrigger(),
            "recurrence"       => new RecurrenceTrigger(),
            "calendarInterval" => new CalendarIntervalTrigger(),
            "dailyTimeInterval"=> new DailyTimeIntervalTrigger(),
            _ => throw new JsonSerializationException($"Unknown trigger type '{type}'")
        };

        serializer.Populate(jsonObject.CreateReader(), trigger);
        return trigger;
    }
    #endregion

    #region WriteJson
    /// <summary>
    ///     Writes the JSON representation of the object, including the "$type" discriminator.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <exception cref="JsonSerializationException"></exception>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null) return;
        var jsonObject = JObject.FromObject(value, serializer);

        var discriminator = value switch
        {
            CronTrigger             => "cron",
            SimpleTrigger           => "simple",
            RecurrenceTrigger       => "recurrence",
            CalendarIntervalTrigger => "calendarInterval",
            DailyTimeIntervalTrigger=> "dailyTimeInterval",
            _ => throw new JsonSerializationException($"Unknown trigger type '{value.GetType().Name}'")
        };

        jsonObject.AddFirst(new JProperty("$type", discriminator));
        jsonObject.WriteTo(writer);
    }
    #endregion
}