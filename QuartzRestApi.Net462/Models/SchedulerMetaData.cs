//
// SchedulerMetaData.cs
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

namespace QuartzRestApi.Models
{
    /// <summary>JSON wrapper for Quartz scheduler meta-data.</summary>
    public class SchedulerMetaData
    {
        [JsonProperty("InStandbyMode")]
        public bool InStandbyMode { get; set; }

        [JsonProperty("JobStoreType")]
        public string JobStoreType { get; set; }

        [JsonProperty("JobStoreClustered")]
        public bool JobStoreClustered { get; set; }

        [JsonProperty("JobsStoreSupportsPersistence")]
        public bool JobStoreSupportsPersistence { get; set; }

        [JsonProperty("NumbersOfJobsExecuted")]
        public int NumbersOfJobsExecuted { get; set; }

        [JsonProperty("RunningSince")]
        public DateTimeOffset? RunningSince { get; set; }

        [JsonProperty("SchedulerInstanceId")]
        public string SchedulerInstanceId { get; set; }

        [JsonProperty("SchedulerName")]
        public string SchedulerName { get; set; }

        [JsonProperty("SchedulerRemote")]
        public bool SchedulerRemote { get; set; }

        [JsonProperty("SchedulerType")]
        public string SchedulerType { get; set; }

        [JsonProperty("Shutdown")]
        public bool Shutdown { get; set; }

        [JsonProperty("Started")]
        public bool Started { get; set; }

        [JsonProperty("Summary")]
        public string Summary { get; set; }

        [JsonProperty("ThreadPoolSize")]
        public int ThreadPoolSize { get; set; }

        [JsonProperty("ThreadPoolType")]
        public string ThreadPoolType { get; set; }

        [JsonProperty("Version")]
        public string Version { get; set; }

        public SchedulerMetaData() { }

        public SchedulerMetaData(Quartz.SchedulerMetaData meta)
        {
            InStandbyMode = meta.InStandbyMode;
            JobStoreType = meta.JobStoreType?.FullName;
            JobStoreClustered = meta.JobStoreClustered;
            JobStoreSupportsPersistence = meta.JobStoreSupportsPersistence;
            NumbersOfJobsExecuted = meta.NumberOfJobsExecuted;
            RunningSince = meta.RunningSince;
            SchedulerInstanceId = meta.SchedulerInstanceId;
            SchedulerName = meta.SchedulerName;
            SchedulerRemote = meta.SchedulerRemote;
            SchedulerType = meta.SchedulerType?.FullName;
            Shutdown = meta.Shutdown;
            Started = meta.Started;
            Summary = meta.GetSummary();
            ThreadPoolSize = meta.ThreadPoolSize;
            ThreadPoolType = meta.ThreadPoolType?.FullName;
            Version = meta.Version;
        }

        public string ToJsonString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static SchedulerMetaData FromJsonString(string json) => JsonConvert.DeserializeObject<SchedulerMetaData>(json);
    }
}
