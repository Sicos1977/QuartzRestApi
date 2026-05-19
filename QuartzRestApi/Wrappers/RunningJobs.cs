//
// RunningJobs.cs
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
using System.Threading;
using Quartz;

namespace QuartzRestApi.Wrappers;

public class RunningJobs : IJobExecutionContext
{
    /// <summary>
    /// Put the specified value into the context's data map with the given key.
    /// Possibly useful for sharing data between listeners and jobs.
    /// <para>
    /// NOTE: this data is volatile - it is lost after the job execution
    /// completes, and all TriggerListeners and JobListeners have been
    /// notified.
    /// </para>
    /// </summary>
    /// <param name="key">
    /// </param>
    /// <param name="objectValue">
    /// </param>
    public void Put(object key, object objectValue)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the value with the given key from the context's data map.
    /// </summary>
    /// <param name="key">
    /// </param>
    public object Get(object key)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a handle to the <see cref="T:Quartz.IScheduler" /> instance that fired the
    /// <see cref="T:Quartz.IJob" />.
    /// </summary>
    public IScheduler Scheduler { get; }

    /// <summary>
    /// Get a handle to the <see cref="T:Quartz.ITrigger" /> instance that fired the
    /// <see cref="T:Quartz.IJob" />.
    /// </summary>
    public ITrigger Trigger { get; }

    /// <summary>
    /// Get a handle to the <see cref="T:Quartz.ICalendar" /> referenced by the <see cref="T:Quartz.ITrigger" />
    /// instance that fired the <see cref="T:Quartz.IJob" />.
    /// </summary>
    public ICalendar Calendar { get; }

    /// <summary>
    /// If the <see cref="T:Quartz.IJob" /> is being re-executed because of a 'recovery'
    /// situation, this method will return <see langword="true" />.
    /// </summary>
    public bool Recovering { get; }

    /// <summary>
    /// Returns the <see cref="T:Quartz.TriggerKey" /> of the originally scheduled and now recovering job.
    /// </summary>
    /// <remarks>
    /// When recovering a previously failed job execution this property returns the identity
    /// of the originally firing trigger. This recovering job will have been scheduled for
    /// the same firing time as the original job, and so is available via the
    /// <see cref="P:Quartz.IJobExecutionContext.ScheduledFireTimeUtc" /> property. The original firing time of the job can be
    /// accessed via the <see cref="F:Quartz.SchedulerConstants.FailedJobOriginalTriggerFiretime" />
    /// element of this job's <see cref="T:Quartz.JobDataMap" />.
    /// </remarks>
    public Quartz.TriggerKey RecoveringTriggerKey { get; }

    /// <summary>Gets the refire count.</summary>
    /// <value>The refire count.</value>
    public int RefireCount { get; }

    /// <summary>
    /// Get the convenience <see cref="T:Quartz.JobDataMap" /> of this execution context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="T:Quartz.JobDataMap" /> found on this object serves as a convenience -
    /// it is a merge of the <see cref="T:Quartz.JobDataMap" /> found on the
    /// <see cref="P:Quartz.IJobExecutionContext.JobDetail" /> and the one found on the <see cref="T:Quartz.ITrigger" />, with
    /// the value in the latter overriding any same-named values in the former.
    /// <i>It is thus considered a 'best practice' that the Execute code of a Job
    /// retrieve data from the JobDataMap found on this object.</i>
    /// </para>
    /// <para>
    /// NOTE: Do not expect value 'set' into this JobDataMap to somehow be
    /// set back onto a job's own JobDataMap.
    /// </para>
    /// <para>
    /// Attempts to change the contents of this map typically result in an
    /// illegal state.
    /// </para>
    /// </remarks>
    public JobDataMap MergedJobDataMap { get; }

    /// <summary>
    /// Get the <see cref="P:Quartz.IJobExecutionContext.JobDetail" /> associated with the <see cref="T:Quartz.IJob" />.
    /// </summary>
    public IJobDetail JobDetail { get; }

    /// <summary>
    /// Get the instance of the <see cref="T:Quartz.IJob" /> that was created for this
    /// execution.
    /// <para>
    /// Note: The Job instance is not available through remote scheduler
    /// interfaces.
    /// </para>
    /// </summary>
    public IJob JobInstance { get; }

    /// <summary>
    /// The actual time the trigger fired. For instance the scheduled time may
    /// have been 10:00:00 but the actual fire time may have been 10:00:03 if
    /// the scheduler was too busy.
    /// </summary>
    /// <returns> Returns the fireTimeUtc.</returns>
    /// <seealso cref="P:Quartz.IJobExecutionContext.ScheduledFireTimeUtc" />
    public DateTimeOffset FireTimeUtc { get; }

    /// <summary>
    /// The scheduled time the trigger fired for. For instance the scheduled
    /// time may have been 10:00:00 but the actual fire time may have been
    /// 10:00:03 if the scheduler was too busy.
    /// </summary>
    /// <returns> Returns the scheduledFireTimeUtc.</returns>
    /// <seealso cref="P:Quartz.IJobExecutionContext.FireTimeUtc" />
    public DateTimeOffset? ScheduledFireTimeUtc { get; }

    /// <summary>Gets the previous fire time.</summary>
    /// <value>The previous fire time.</value>
    public DateTimeOffset? PreviousFireTimeUtc { get; }

    /// <summary>Gets the next fire time.</summary>
    /// <value>The next fire time.</value>
    public DateTimeOffset? NextFireTimeUtc { get; }

    /// <summary>
    /// Get the unique Id that identifies this particular firing instance of the
    /// trigger that triggered this job execution.  It is unique to this
    /// JobExecutionContext instance as well.
    /// </summary>
    /// <returns>the unique fire instance id</returns>
    /// <seealso cref="M:Quartz.IScheduler.Interrupt(System.String,System.Threading.CancellationToken)" />
    public string FireInstanceId { get; }

    /// <summary>
    /// Returns the result (if any) that the <see cref="T:Quartz.IJob" /> set before its
    /// execution completed (the type of object set as the result is entirely up
    /// to the particular job).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The result itself is meaningless to Quartz, but may be informative
    /// to <see cref="T:Quartz.IJobListener" />s or
    /// <see cref="T:Quartz.ITriggerListener" />s that are watching the job's
    /// execution.
    /// </para>
    /// Set the result (if any) of the <see cref="T:Quartz.IJob" />'s execution (the type of
    /// object set as the result is entirely up to the particular job).
    /// <para>
    /// The result itself is meaningless to Quartz, but may be informative
    /// to <see cref="T:Quartz.IJobListener" />s or
    /// <see cref="T:Quartz.ITriggerListener" />s that are watching the job's
    /// execution.
    /// </para>
    /// </remarks>
    public object Result { get; set; }

    /// <summary>
    /// The amount of time the job ran for.  The returned
    /// value will be <see cref="F:System.TimeSpan.MinValue" /> until the job has actually completed (or thrown an
    /// exception), and is therefore generally only useful to
    /// <see cref="T:Quartz.IJobListener" />s and <see cref="T:Quartz.ITriggerListener" />s.
    /// </summary>
    public TimeSpan JobRunTime { get; }

    /// <summary>
    /// Returns the cancellation token which will be cancelled when the job cancellation has been requested via
    /// <see cref="M:Quartz.IScheduler.Interrupt(Quartz.JobKey,System.Threading.CancellationToken)" />
    /// or <see cref="M:Quartz.IScheduler.Interrupt(System.String,System.Threading.CancellationToken)" />.
    /// </summary>
    public CancellationToken CancellationToken { get; }
}
