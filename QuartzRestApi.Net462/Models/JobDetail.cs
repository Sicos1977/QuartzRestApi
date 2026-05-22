//
// JobDetail.cs
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

namespace QuartzRestApi.Models;
/// <summary>JSON wrapper for <see cref="IJobDetail"/>.</summary>
public class JobDetail
{
    [JsonProperty("JobKey")]
    public JobKey JobKey { get; set; }

    [JsonProperty("Description")]
    public string Description { get; set; }

    [JsonProperty("JobType")]
    public string JobType { get; set; }

    [JsonProperty("JobDataMap")]
    public JobDataMap JobDataMap { get; set; }

    [JsonProperty("Durable")]
    public bool Durable { get; set; }

    [JsonProperty("Replace")]
    public bool Replace { get; set; }

    [JsonProperty("StoreNonDurableWhileAwaitingScheduling")]
    public bool StoreNonDurableWhileAwaitingScheduling { get; set; }

    public JobDetail() { }

    public JobDetail(
        JobKey jobKey,
        string description,
        string jobType,
        JobDataMap jobDataMap,
        bool durable,
        bool replace,
        bool storeNonDurableWhileAwaitingScheduling)
    {
        JobKey = jobKey;
        Description = description;
        JobType = jobType;
        JobDataMap = jobDataMap;
        Durable = durable;
        Replace = replace;
        StoreNonDurableWhileAwaitingScheduling = storeNonDurableWhileAwaitingScheduling;
    }

    public JobDetail(IJobDetail detail)
    {
        JobKey = new JobKey(detail.Key);
        Description = detail.Description;
        JobType = detail.JobType?.FullName;
        JobDataMap = detail.JobDataMap;
        Durable = detail.Durable;
    }

    public IJobDetail ToJobDetail()
    {
        var type = string.IsNullOrWhiteSpace(JobType) ? null : Type.GetType(JobType);
        if (type == null)
            type = typeof(FallbackNoOpJob);
        var builder = JobBuilder.Create(type)
            .WithIdentity(JobKey.Name, JobKey.Group ?? "DEFAULT")
            .WithDescription(Description);

        if (Durable)
            builder.StoreDurably();

        if (JobDataMap != null && JobDataMap.Count > 0)
            builder.UsingJobData(JobDataMap);

        return builder.Build();
    }

    public string ToJsonString() => JsonConvert.SerializeObject(this, Formatting.Indented);

    public static JobDetail FromJsonString(string json) => JsonConvert.DeserializeObject<JobDetail>(json);
}

