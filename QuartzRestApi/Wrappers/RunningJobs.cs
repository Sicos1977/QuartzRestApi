//
// RunningJobs.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2022 - 2024 Magic-Sessions. (www.magic-sessions.com)
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

namespace QuartzRestApi.Wrappers;
//public class RunningJobs : IJobExecutionContext
//{
//    public void Put(object key, object objectValue)
//    {
//        throw new NotImplementedException();
//    }

//    public object Get(object key)
//    {
//        throw new NotImplementedException();
//    }

//    public IScheduler Scheduler { get; }
//    public ITrigger Trigger { get; }
//    public ICalendar Calendar { get; }
//    public bool Recovering { get; }
//    public TriggerKey RecoveringTriggerKey { get; }
//    public int RefireCount { get; }
//    public JobDataMap MergedJobDataMap { get; }
//    public IJobDetail JobDetail { get; }
//    public IJob JobInstance { get; }
//    public DateTimeOffset FireTimeUtc { get; }
//    public DateTimeOffset? ScheduledFireTimeUtc { get; }
//    public DateTimeOffset? PreviousFireTimeUtc { get; }
//    public DateTimeOffset? NextFireTimeUtc { get; }
//    public string FireInstanceId { get; }
//    public object Result { get; set; }
//    public TimeSpan JobRunTime { get; }
//    public CancellationToken CancellationToken { get; }
//}