//
// JobExecutionContext.cs
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
    /// <summary>JSON wrapper for <see cref="IJobExecutionContext"/>.</summary>
    public class JobExecutionContext
    {
        [JsonProperty("Scheduler")]
        public string Scheduler { get; set; }

        [JsonProperty("Trigger")]
        public Trigger Trigger { get; set; }

        [JsonProperty("Calender")]
        public string Calendar { get; set; }

        [JsonProperty("Recovering")]
        public bool Recovering { get; set; }

        [JsonProperty("RecoveringTriggerKey")]
        public TriggerKey RecoveringTriggerKey { get; set; }

        [JsonProperty("RefireCount")]
        public int RefireCount { get; set; }

        [JsonProperty("MergedJobDataMap")]
        public JobDataMap MergedJobDataMap { get; set; }

        [JsonProperty("JobDetail")]
        public JobDetail JobDetail { get; set; }

        [JsonProperty("JobInstance")]
        public string JobInstance { get; set; }

        [JsonProperty("FireTimeUtc")]
        public DateTimeOffset FireTimeUtc { get; set; }

        [JsonProperty("ScheduledFireTimeUtc")]
        public DateTimeOffset? ScheduledFireTimeUtc { get; set; }

        [JsonProperty("PreviousFireTimeUtc")]
        public DateTimeOffset? PreviousFireTimeUtc { get; set; }

        [JsonProperty("NextFireTimeUtc")]
        public DateTimeOffset? NextFireTimeUtc { get; set; }

        [JsonProperty("FireInstanceId")]
        public string FireInstanceId { get; set; }

        [JsonProperty("JobRunTime")]
        public TimeSpan JobRunTime { get; set; }

        public JobExecutionContext() { }

        public JobExecutionContext(IJobExecutionContext ctx)
        {
            Scheduler = ctx.Scheduler?.SchedulerName;
            Trigger = ctx.Trigger != null ? new Trigger(ctx.Trigger) : null;
            Calendar = ctx.Calendar != null ? ctx.Trigger?.CalendarName : null;
            Recovering = ctx.Recovering;
            RecoveringTriggerKey = Recovering ? new TriggerKey(ctx.RecoveringTriggerKey) : null;
            RefireCount = ctx.RefireCount;
            MergedJobDataMap = ctx.MergedJobDataMap;
            JobDetail = ctx.JobDetail != null ? new JobDetail(ctx.JobDetail) : null;
            JobInstance = ctx.JobInstance?.GetType().FullName;
            FireTimeUtc = ctx.FireTimeUtc;
            ScheduledFireTimeUtc = ctx.ScheduledFireTimeUtc;
            PreviousFireTimeUtc = ctx.PreviousFireTimeUtc;
            NextFireTimeUtc = ctx.NextFireTimeUtc;
            FireInstanceId = ctx.FireInstanceId;
            JobRunTime = ctx.JobRunTime;
        }
    }
}
