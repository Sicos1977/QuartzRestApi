//
// TriggerBase.cs
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

using QuartzRestApi.Models.Jobs;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuartzRestApi.Models.Triggers;

/// <summary>
///     Abstract base class for all trigger DTOs.
///     Contains the common properties shared across all Quartz trigger types.
/// </summary>
/// <remarks>
///     Uses <see cref="JsonPolymorphicAttribute"/> to support correct JSON (de)serialization
///     of derived trigger types via the <c>$type</c> discriminator property.
/// </remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(CronTrigger), "cron")]
[JsonDerivedType(typeof(SimpleTrigger), "simple")]
[JsonDerivedType(typeof(RecurrenceTrigger), "recurrence")]
[JsonDerivedType(typeof(CalendarIntervalTrigger), "calendarInterval")]
[JsonDerivedType(typeof(DailyTimeIntervalTrigger), "dailyTimeInterval")]
public abstract class TriggerBase : JobKeyWithDataMap
{
    #region Properties
    /// <summary>
    ///     The <see cref="TriggerKey"/> that uniquely identifies this trigger
    /// </summary>
    [JsonPropertyName("TriggerKey")]
    public TriggerKey TriggerKey { get; init; }

    /// <summary>
    ///     An optional description for this trigger
    /// </summary>
    [JsonPropertyName("Description")]
    public string Description { get; init; }

    /// <summary>
    ///     The name of the calendar associated with this trigger, or <c>null</c> if none
    /// </summary>
    [JsonPropertyName("CalendarName")]
    public string CalendarName { get; init; }

    /// <summary>
    ///     The next scheduled fire time in UTC, or <c>null</c> if the trigger will not fire again
    /// </summary>
    [JsonPropertyName("NextFireTimeUtc")]
    public DateTimeOffset? NextFireTimeUtc { get; init; }

    /// <summary>
    ///     The previous fire time in UTC, or <c>null</c> if the trigger has not fired yet
    /// </summary>
    [JsonPropertyName("PreviousFireTimeUtc")]
    public DateTimeOffset? PreviousFireTimeUtc { get; init; }

    /// <summary>
    ///     The time in UTC at which the trigger should start firing
    /// </summary>
    [JsonPropertyName("StartTimeUtc")]
    public DateTimeOffset StartTimeUtc { get; init; }

    /// <summary>
    ///     The time in UTC at which the trigger should stop firing, or <c>null</c> if no end time is set
    /// </summary>
    [JsonPropertyName("EndTimeUtc")]
    public DateTimeOffset? EndTimeUtc { get; init; }

    /// <summary>
    ///     The last time in UTC at which the trigger will fire, or <c>null</c> if it repeats indefinitely
    /// </summary>
    [JsonPropertyName("FinalFireTimeUtc")]
    public DateTimeOffset? FinalFireTimeUtc { get; init; }

    /// <summary>
    ///     The priority of this trigger. Higher values take precedence when multiple triggers fire at the same time
    /// </summary>
    [JsonPropertyName("Priority")]
    public int Priority { get; init; }

    /// <summary>
    ///     Returns <c>true</c> when this trigger has millisecond precision
    /// </summary>
    [JsonPropertyName("HasMillisecondPrecision")]
    public bool HasMillisecondPrecision { get; init; }
    #endregion

    #region Constructors
    /// <summary>
    ///     Parameterless constructor for JSON deserialization
    /// </summary>
    [JsonConstructor]
    protected TriggerBase() { }

    /// <summary>
    ///     Initializes the base properties from a Quartz <see cref="Quartz.ITrigger"/>
    /// </summary>
    /// <param name="trigger">The Quartz trigger to map from</param>
    protected TriggerBase(Quartz.ITrigger trigger) : base(new JobKey(trigger.JobKey), trigger.JobDataMap)
    {
        TriggerKey = new TriggerKey(trigger.Key);
        Description = trigger.Description;
        CalendarName = trigger.CalendarName;
        NextFireTimeUtc = trigger.GetNextFireTimeUtc();
        PreviousFireTimeUtc = trigger.GetPreviousFireTimeUtc();
        StartTimeUtc = trigger.StartTimeUtc;
        EndTimeUtc = trigger.EndTimeUtc;
        FinalFireTimeUtc = trigger.FinalFireTimeUtc;
        Priority = trigger.Priority;
        HasMillisecondPrecision = trigger.HasMillisecondPrecision;
    }
    #endregion

    #region ToTrigger
    /// <summary>
    ///     Converts this DTO back to a Quartz <see cref="Quartz.ITrigger"/>
    /// </summary>
    /// <returns>A configured Quartz <see cref="Quartz.ITrigger"/></returns>
    // ReSharper disable once UnusedMember.Global
    public abstract Quartz.ITrigger ToTrigger();
    #endregion

    #region ApplyCommonProperties
    /// <summary>
    ///     Applies the common trigger properties to the given <see cref="Quartz.TriggerBuilder"/>
    /// </summary>
    /// <param name="builder">The <see cref="Quartz.TriggerBuilder"/> to configure</param>
    /// <returns>The configured <see cref="Quartz.TriggerBuilder"/></returns>
    protected Quartz.TriggerBuilder ApplyCommonProperties(Quartz.TriggerBuilder builder)
    {
        builder = builder
            .ForJob(JobKey.ToJobKey())
            .WithIdentity(TriggerKey.ToTriggerKey())
            .WithPriority(Priority)
            .StartAt(StartTimeUtc);

        if (EndTimeUtc.HasValue)
            builder = builder.EndAt(EndTimeUtc.Value);

        if (!string.IsNullOrWhiteSpace(CalendarName))
            builder = builder.ModifiedByCalendar(CalendarName);

        if (!string.IsNullOrWhiteSpace(Description))
            builder = builder.WithDescription(Description);

        if (JobDataMap != null)
            builder = builder.UsingJobData(JobDataMap);

        return builder;
    }
    #endregion

    #region ToJsonString
    /// <summary>
    ///     Serializes this trigger to a JSON string
    /// </summary>
    /// <returns>A JSON string representation of this trigger</returns>
    public new string ToJsonString()
    {
        return JsonSerializer.Serialize(this, GetType(), new JsonSerializerOptions { WriteIndented = true });
    }
    #endregion

    #region FromJsonString
    /// <summary>
    ///     Deserializes a <see cref="TriggerBase"/> (or derived type) from the given <paramref name="json"/> string.
    ///     The correct subtype is resolved automatically via the <c>$type</c> discriminator.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized <see cref="TriggerBase"/> instance</returns>
    public new static TriggerBase FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<TriggerBase>(json);
    }
    #endregion
}