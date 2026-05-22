//
// JobDetailWithTriggers.cs
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Quartz;

namespace QuartzRestApi.Models
{
    /// <summary>JSON wrapper for scheduling a job with multiple triggers.</summary>
    public class JobDetailWithTriggers
    {
        [JsonProperty("JobDetail")]
        public JobDetail JobDetail { get; set; }

        [JsonProperty("Triggers")]
        public List<Trigger> Triggers { get; set; }

        [JsonProperty("Replace")]
        public bool Replace { get; set; }

        public JobDetailWithTriggers() { }

        public JobDetailWithTriggers(JobDetail jobDetail, List<Trigger> triggers)
        {
            JobDetail = jobDetail;
            Triggers = triggers;
        }

        public IReadOnlyCollection<ITrigger> ToReadOnlyTriggerCollection()
            => new ReadOnlyCollection<ITrigger>(Triggers.Select(t => t.ToTrigger()).ToList());

        public string ToJsonString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static JobDetailWithTriggers FromJsonString(string json) => JsonConvert.DeserializeObject<JobDetailWithTriggers>(json);
    }
}
