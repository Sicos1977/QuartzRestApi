using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuartzRestApi.Wrappers.Calendars;

/// <summary>
///    A json converter for the <see cref="BaseCalendar" />
/// </summary>
internal class BaseConverter : JsonConverter<BaseCalendar>
{
    #region Read
    public override BaseCalendar Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("Type", out var typeProp))
            throw new ArgumentException("Type is not set");

        var type = Enum.Parse<CalendarType>(typeProp.GetString());
        var json = root.GetRawText();

        // Use options without this converter to avoid stack overflow
        var innerOptions = new JsonSerializerOptions(options);
        innerOptions.Converters.Remove(this);

        return type switch
        {
            CalendarType.Cron => JsonSerializer.Deserialize<CronCalendar>(json, innerOptions),
            CalendarType.Daily => JsonSerializer.Deserialize<DailyCalendar>(json, innerOptions),
            CalendarType.Weekly => JsonSerializer.Deserialize<WeeklyCalendar>(json, innerOptions),
            CalendarType.Monthly => JsonSerializer.Deserialize<MonthlyCalendar>(json, innerOptions),
            CalendarType.Annual => JsonSerializer.Deserialize<AnnualCalendar>(json, innerOptions),
            CalendarType.Holiday => JsonSerializer.Deserialize<HolidayCalendar>(json, innerOptions),
            _ => throw new ArgumentException("Invalid type")
        };
    }
    #endregion

    #region Write
    public override void Write(Utf8JsonWriter writer, BaseCalendar value, JsonSerializerOptions options)
    {
        var innerOptions = new JsonSerializerOptions(options);
        innerOptions.Converters.Remove(this);
        JsonSerializer.Serialize(writer, value, value.GetType(), innerOptions);
    }
    #endregion
}