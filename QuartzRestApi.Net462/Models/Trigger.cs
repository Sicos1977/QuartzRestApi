//
// Trigger.cs
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
using Quartz;

namespace QuartzRestApi.Models
{
    /// <summary>JSON wrapper for a Quartz trigger.</summary>
    public class Trigger : JobKeyWithDataMap
    {
        [JsonProperty("TriggerKey")]
        public TriggerKey TriggerKey { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("CalendarName")]
        public string CalendarName { get; set; }

        [JsonProperty("CronSchedule")]
        public string CronSchedule { get; set; }

        [JsonProperty("NextFireTimeUtc")]
        public DateTimeOffset? NextFireTimeUtc { get; set; }

        [JsonProperty("PreviousFireTimeUtc")]
        public DateTimeOffset? PreviousFireTimeUtc { get; set; }

        [JsonProperty("StartTimeUtc")]
        public DateTimeOffset StartTimeUtc { get; set; }

        [JsonProperty("EndTimeUtc")]
        public DateTimeOffset? EndTimeUtc { get; set; }

        [JsonProperty("FinalFireTimeUtc")]
        public DateTimeOffset? FinalFireTimeUtc { get; set; }

        [JsonProperty("Priority")]
        public int Priority { get; set; }

        [JsonProperty("HasMillisecondPrecision")]
        public bool HasMillisecondPrecision { get; set; }

        public Trigger() { }

        public Trigger(ITrigger trigger)
        {
            TriggerKey = new TriggerKey(trigger.Key);
            JobKey = trigger.JobKey != null ? new JobKey(trigger.JobKey) : null;
            JobDataMap = trigger.JobDataMap;
            Description = trigger.Description;
            CalendarName = trigger.CalendarName;
            NextFireTimeUtc = trigger.GetNextFireTimeUtc();
            PreviousFireTimeUtc = trigger.GetPreviousFireTimeUtc();
            StartTimeUtc = trigger.StartTimeUtc;
            EndTimeUtc = trigger.EndTimeUtc;
            FinalFireTimeUtc = trigger.FinalFireTimeUtc;
            Priority = trigger.Priority;
            HasMillisecondPrecision = trigger.HasMillisecondPrecision;
            CronSchedule = (trigger is ICronTrigger cron) ? cron.CronExpressionString : null;
        }

        public ITrigger ToTrigger()
        {
            var builder = TriggerBuilder.Create()
                .WithIdentity(TriggerKey.Name, TriggerKey.Group ?? "DEFAULT");

            if (JobKey != null)
                builder.ForJob(new Quartz.JobKey(JobKey.Name, JobKey.Group ?? "DEFAULT"));

            builder.StartAt(StartTimeUtc);

            if (EndTimeUtc.HasValue)
                builder.EndAt(EndTimeUtc.Value);

            if (!string.IsNullOrWhiteSpace(CronSchedule))
                builder.WithCronSchedule(CronSchedule);
            else
                builder.WithSimpleSchedule();

            if (!string.IsNullOrWhiteSpace(CalendarName))
                builder.ModifiedByCalendar(CalendarName);

            if (JobDataMap != null && JobDataMap.Count > 0)
                builder.UsingJobData(JobDataMap);

            builder.WithPriority(Priority > 0 ? Priority : 5);

            return builder.Build();
        }

        public string ToJsonString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static Trigger FromJsonString(string json) => JsonConvert.DeserializeObject<Trigger>(json);
    }
}
