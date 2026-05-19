//
// UnitTest1.cs
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Quartz;
using Quartz.Impl;
using QuartzRestApi.Wrappers;
using GroupMatcherType = QuartzRestApi.Wrappers.GroupMatcherType;
using JobKey = QuartzRestApi.Wrappers.JobKey;
using RescheduleJob = QuartzRestApi.Wrappers.RescheduleJob;
using TriggerKey = QuartzRestApi.Wrappers.TriggerKey;

namespace QuartzRestApi.Tests;

/// <summary>
///     Contains unit tests for verifying the behavior and integration of a Quartz scheduler host and connector, including
///     job and trigger management, scheduler state, calendar operations, and error handling.
/// </summary>
/// <remarks>
///     This test class sets up an isolated Quartz scheduler environment for each test, ensuring consistent
///     and repeatable test results. It covers a wide range of scheduler operations, such as starting and stopping the
///     scheduler, adding and removing jobs and triggers, managing calendars, and handling error states. The tests are
///     designed to validate both the expected API behavior and the correct interaction between the scheduler host and
///     connector components.
/// </remarks>
[TestClass]
public class UnitTests
{
    #region Fields
    /// <summary>
    ///     The SchedulerHost instance used to expose the Quartz scheduler for testing. This host is started before each test and stopped afterward to ensure a clean state.
    /// </summary>
    private SchedulerHost _host;

    /// <summary>
    ///     The SchedulerConnector instance used to interact with the SchedulerHost's API during tests. This connector is initialized in the test setup and provides methods for querying and manipulating the scheduler state.
    /// </summary>
    private SchedulerConnector _connector;
    #endregion

    #region Initialize
    /// <summary>
    ///     Initializes the test environment by configuring and starting a Quartz scheduler instance with predefined jobs,
    ///     triggers, and calendars.
    /// </summary>
    /// <remarks>
    ///     This method is executed before each test to ensure a consistent and isolated scheduler state.
    ///     It sets up a scheduler with specific context values, schedules a test job with a trigger, and configures a
    ///     monthly calendar with excluded days. The method also starts a local scheduler host and connector for integration
    ///     testing. This setup is required for tests that depend on a running scheduler instance.
    /// </remarks>
    [TestInitialize]
    public void Initialize()
    {
        var properties = new NameValueCollection
        {
            ["quartz.scheduler.instanceName"] = "SchedulerInstance",
            ["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz",
            ["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz"
        };

        var scheduler = new StdSchedulerFactory(properties).GetScheduler().Result;

        if (scheduler.Context.TryAdd("key1", "value1"))
        {
            scheduler.Context.TryAdd("key2", "value2");
            scheduler.Context.TryAdd("key3", "value3");
        }

        scheduler.Start();

        var schedulerJob = JobBuilder.Create<TestJob>()
            .WithIdentity(new Quartz.JobKey("JobKeyName", "JobKeyGroup"))
            .WithDescription("Test")
            .RequestRecovery()
            .Build();

        var schedulerTrigger = (ISimpleTrigger)TriggerBuilder.Create()
            .WithIdentity("triggerKey", "JobKeyGroup")
            .WithDescription("TestTrigger")
            .StartAt(DateTime.Now)
            .Build();

        var mc = new Quartz.Impl.Calendar.MonthlyCalendar();
        mc.SetDayExcluded(1, true);
        mc.SetDayExcluded(12, true);
        scheduler.AddCalendar("monthlyCalendar", mc, true, true);
        scheduler.ScheduleJob(schedulerJob, schedulerTrigger);

        _host = new SchedulerHost("http://localhost:44344", scheduler, null);
        _host.Start();

        _connector = new SchedulerConnector("http://localhost:44344");
    }
    #endregion

    #region Cleanup
    /// <summary>
    ///     Performs cleanup operations after each test is executed.
    /// </summary>
    /// <remarks>
    ///     This method is called automatically by the test framework after each test method in the class
    ///     completes. It is used to release resources or stop services that were initialized for the test.
    /// </remarks>
    [TestCleanup]
    public void Cleanup()
    {
        _host.Stop();
    }
    #endregion

    #region Helpers
    /// <summary>
    ///     Creates a new durable job detail for a test job with the specified name and group.
    /// </summary>
    /// <remarks>
    ///     The returned job detail is marked as durable and is set to replace any existing job with the
    ///     same key.
    /// </remarks>
    /// <param name="name">The name of the job. Cannot be null or empty.</param>
    /// <param name="group">The group to which the job belongs. Cannot be null or empty.</param>
    /// <returns>A durable JobDetail instance configured for the specified name and group.</returns>
    private static JobDetail MakeJobDetail(string name, string group) =>
        new(
            new JobKey(name, group),
            description: null,
            typeof(TestJob).AssemblyQualifiedName,
            jobDataMap: null,
            durable: true,
            replace: true,
            storeNonDurableWhileAwaitingScheduling: false);

    /// <summary>
    ///    Helper method to create a trigger with the specified parameters. The trigger is configured with a default priority of 5
    /// </summary>
    /// <param name="triggerName">The name of the trigger.</param>
    /// <param name="triggerGroup">The group of the trigger.</param>
    /// <param name="jobName">The name of the job associated with the trigger.</param>
    /// <param name="jobGroup">The group of the job associated with the trigger.</param>
    /// <param name="cron">The cron expression for the trigger schedule.</param>
    /// <returns>A new instance of <see cref="Trigger"/> configured with the specified parameters.</returns>
    private static Trigger MakeTrigger(string triggerName, string triggerGroup, string jobName, string jobGroup, string cron) =>
        new(
            new TriggerKey(triggerName, triggerGroup),
            description: null,
            calendarName: null,
            cronSchedule: cron,
            nextFireTimeUtc: null,
            previousFireTimeUtc: null,
            startTimeUtc: DateTimeOffset.UtcNow,
            endTimeUtc: null,
            finalFireTimeUtc: null,
            priority: 5,
            new JobKey(jobName, jobGroup),
            jobDataMap: null);
    #endregion

    #region Scheduler state

    /// <summary>
    ///     Verifies that <c>IsJobGroupPaused</c> returns <c>false</c> for an active job group.
    /// </summary>
    [TestMethod]
    public void IsJobGroupPaused()
    {
        var paused = _connector.IsJobGroupPaused("JobKeyGroup").Result;
        Assert.IsFalse(paused);
    }

    /// <summary>
    ///     Verifies that <c>IsTriggerGroupPaused</c> returns <c>false</c> for an active trigger group.
    /// </summary>
    [TestMethod]
    public void IsTriggerGroupPaused()
    {
        var paused = _connector.IsTriggerGroupPaused("JobKeyGroup").Result;
        Assert.IsFalse(paused);
    }

    /// <summary>
    ///     Verifies that <c>SchedulerName</c> returns the configured instance name.
    /// </summary>
    [TestMethod]
    public void SchedulerName()
    {
        var name = _connector.SchedulerName().Result;
        Assert.AreEqual("SchedulerInstance", name);
    }

    /// <summary>
    ///     Verifies that <c>SchedulerInstanceId</c> returns the expected non-clustered identifier.
    /// </summary>
    [TestMethod]
    public void SchedulerInstanceId()
    {
        var id = _connector.SchedulerInstanceId().Result;
        Assert.AreEqual("NON_CLUSTERED", id);
    }

    /// <summary>
    ///     Verifies that <c>Context</c> returns all key/value pairs that were added during setup.
    /// </summary>
    [TestMethod]
    public void Context()
    {
        var result = _connector.Context().Result;
        Assert.AreEqual("value1", result["key1"]);
        Assert.AreEqual("value2", result["key2"]);
        Assert.AreEqual("value3", result["key3"]);
    }

    /// <summary>
    ///     Verifies that <c>InStandbyMode</c> returns <c>false</c> when the scheduler is running.
    /// </summary>
    [TestMethod]
    public void InStandbyMode()
    {
        var result = _connector.InStandbyMode().Result;
        Assert.IsFalse(result);
    }

    /// <summary>
    ///     Verifies that <c>IsShutdown</c> returns <c>false</c> when the scheduler is running.
    /// </summary>
    [TestMethod]
    public void IsShutdown()
    {
        var result = _connector.IsShutdown().Result;
        Assert.IsFalse(result);
    }

    /// <summary>
    ///     Verifies that <c>IsStarted</c> returns <c>true</c> when the scheduler is running.
    /// </summary>
    [TestMethod]
    public void IsStarted()
    {
        var result = _connector.IsStarted().Result;
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Verifies that <c>GetMetaData</c> returns a summary containing the word "Quartz".
    /// </summary>
    [TestMethod]
    public void GetMetaData()
    {
        var result = _connector.GetMetaData().Result;
        Assert.IsTrue(result.Summary.Contains("Quartz"));
    }

    /// <summary>
    ///     Verifies that <c>Start</c> completes without error when the scheduler is already running.
    /// </summary>
    [TestMethod]
    public void Start()
    {
        _connector.Start().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Verifies that <c>StartDelayed</c> completes without error.
    /// </summary>
    [TestMethod]
    public void StartDelayed()
    {
        _connector.StartDelayed(1).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that calling <c>Standby</c> puts the scheduler in standby mode
    ///     and that <c>Start</c> brings it back to running.
    /// </summary>
    [TestMethod]
    public void Standby_Then_Start()
    {
        _connector.Standby().GetAwaiter().GetResult();
        var standby = _connector.InStandbyMode().Result;
        Assert.IsTrue(standby);

        _connector.Start().GetAwaiter().GetResult();
        var active = _connector.InStandbyMode().Result;
        Assert.IsFalse(active);
    }

    /// <summary>
    ///     Verifies that <c>Shutdown</c> stops an isolated scheduler instance without affecting
    ///     the shared test scheduler.
    /// </summary>
    [TestMethod]
    public void Shutdown_WithWaitForJobsToComplete()
    {
        var properties = new NameValueCollection
        {
            ["quartz.scheduler.instanceName"] = "ShutdownTestScheduler",
            ["quartz.jobStore.type"]          = "Quartz.Simpl.RAMJobStore, Quartz",
            ["quartz.threadPool.type"]        = "Quartz.Simpl.SimpleThreadPool, Quartz"
        };

        var scheduler = new StdSchedulerFactory(properties).GetScheduler().Result;
        scheduler.Start();

        var isolatedHost = new SchedulerHost("http://localhost:44346", scheduler, null);
        isolatedHost.Start();
        try
        {
            var isolatedConnector = new SchedulerConnector("http://localhost:44346");
            isolatedConnector.Shutdown(waitForJobsToComplete: false).GetAwaiter().GetResult();

            var isShutdown = isolatedConnector.IsShutdown().Result;
            Assert.IsTrue(isShutdown);
        }
        finally
        {
            isolatedHost.Stop();
        }
    }
    #endregion

    #region Groups
    /// <summary>
    ///     Verifies that <c>GetJobGroupNames</c> returns the group that was created during setup.
    /// </summary>
    [TestMethod]
    public void GetJobGroupNames()
    {
        var groups = _connector.GetJobGroupNames().Result;
        Assert.IsTrue(groups.Contains("JobKeyGroup"));
    }

    /// <summary>
    ///     Verifies that <c>GetTriggerGroupNames</c> returns the group that was created during setup.
    /// </summary>
    [TestMethod]
    public void GetTriggerGroupNames()
    {
        var groups = _connector.GetTriggerGroupNames().Result;
        Assert.IsTrue(groups.Contains("JobKeyGroup"));
    }

    /// <summary>
    ///     Verifies that <c>GetPausedTriggerGroups</c> returns a non-null result.
    /// </summary>
    [TestMethod]
    public void GetPausedTriggerGroups()
    {
        var groups = _connector.GetPausedTriggerGroups().Result;
        Assert.IsNotNull(groups);
    }
    #endregion

    #region Keys
    /// <summary>
    ///     Verifies that <c>GetJobKeys</c> returns the job key that was added during setup
    ///     when filtering by its group with an Equals matcher.
    /// </summary>
    [TestMethod]
    public void GetJobKeys()
    {
        var matcher = new GroupMatcher<Quartz.JobKey>(GroupMatcherType.Equals, "JobKeyGroup");
        var keys = _connector.GetJobKeys(matcher).Result;
        Assert.IsTrue(keys.Any(k => k.Name == "JobKeyName"));
    }

    /// <summary>
    /// Verifies that <c>GetTriggerKeys</c> returns the trigger key that was added during setup
    /// when filtering by its group with an Equals matcher.
    /// </summary>
    [TestMethod]
    public void GetTriggerKeys()
    {
        var matcher = new GroupMatcher<Quartz.TriggerKey>(GroupMatcherType.Equals, "JobKeyGroup");
        var keys = _connector.GetTriggerKeys(matcher).Result;
        Assert.IsTrue(keys.Any(k => k.Name == "triggerKey"));
    }
    #endregion

    #region Job and trigger detail
    /// <summary>
    ///     Verifies that <c>GetJobDetail</c> returns the correct detail for an existing job.
    /// </summary>
    [TestMethod]
    public void GetJobDetail()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        var detail = _connector.GetJobDetail(jobKey).Result;
        Assert.IsNotNull(detail);
        Assert.AreEqual("JobKeyName", detail.JobKey.Name);
    }

    /// <summary>
    ///     Verifies that <c>GetTriggersOfJob</c> returns the trigger that was associated with
    ///     the job during setup.
    /// </summary>
    [TestMethod]
    public void GetTriggersOfJob()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        var triggers = _connector.GetTriggersOfJob(jobKey).Result;
        Assert.IsTrue(triggers.Any(t => t.TriggerKey.Name == "triggerKey"));
    }

    /// <summary>
    ///     Verifies that <c>GetTrigger</c> returns the correct trigger for an existing trigger key.
    /// </summary>
    [TestMethod]
    public void GetTrigger()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");
        var trigger = _connector.GetTrigger(triggerKey).Result;
        Assert.IsNotNull(trigger);
        Assert.AreEqual("triggerKey", trigger.TriggerKey.Name);
    }

    /// <summary>
    ///     Verifies that <c>GetTriggerState</c> returns a non-empty state string for an existing trigger.
    /// </summary>
    [TestMethod]
    public void GetTriggerState()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");
        var state = _connector.GetTriggerState(triggerKey).Result;
        Assert.IsFalse(string.IsNullOrWhiteSpace(state));
    }

    /// <summary>
    ///     Verifies that <c>GetCurrentlyExecutingJobs</c> returns a non-null result.
    /// </summary>
    [TestMethod]
    public void GetCurrentlyExecutingJobs()
    {
        var result = _connector.GetCurrentlyExecutingJobs().Result;
        Assert.IsNotNull(result);
    }
    #endregion

    #region Existence checks
    /// <summary>
    ///     Verifies that <c>CheckExists</c> returns <c>true</c> for a job key that is present in the scheduler.
    /// </summary>
    [TestMethod]
    public void CheckExistsJobKey_Existing()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        var exists = _connector.CheckExists(jobKey).Result;
        Assert.IsTrue(exists);
    }

    /// <summary>
    ///     Verifies that <c>CheckExists</c> returns <c>false</c> for a job key that is not in the scheduler.
    /// </summary>
    [TestMethod]
    public void CheckExistsJobKey_NotExisting()
    {
        var jobKey = new JobKey("DoesNotExist", "JobKeyGroup");
        var exists = _connector.CheckExists(jobKey).Result;
        Assert.IsFalse(exists);
    }

    /// <summary>
    ///     Verifies that <c>CheckExists</c> returns <c>true</c> for a trigger key that is present in the scheduler.
    /// </summary>
    [TestMethod]
    public void CheckExistsTriggerKey_Existing()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");
        var exists = _connector.CheckExists(triggerKey).Result;
        Assert.IsTrue(exists);
    }

    /// <summary>
    ///     Verifies that <c>CheckExists</c> returns <c>false</c> for a trigger key that is not in the scheduler.
    /// </summary>
    [TestMethod]
    public void CheckExistsTriggerKey_NotExisting()
    {
        var triggerKey = new TriggerKey("DoesNotExist", "JobKeyGroup");
        var exists = _connector.CheckExists(triggerKey).Result;
        Assert.IsFalse(exists);
    }
    #endregion

    #region Pause and resume
    /// <summary>
    ///     Verifies that <c>PauseJob</c> pauses the job group and that <c>ResumeJob</c> restores it.
    /// </summary>
    [TestMethod]
    public void PauseJob_And_ResumeJob()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");

        _connector.PauseJob(jobKey).GetAwaiter().GetResult();
        var paused = _connector.IsJobGroupPaused("JobKeyGroup").Result;
        Assert.IsTrue(paused);

        _connector.ResumeJob(jobKey).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>PauseJobs</c> pauses the matching job group and that <c>ResumeJobs</c> restores it.
    /// </summary>
    [TestMethod]
    public void PauseJobs_And_ResumeJobs()
    {
        var matcher = new GroupMatcher<Quartz.JobKey>(GroupMatcherType.Equals, "JobKeyGroup");

        _connector.PauseJobs(matcher).GetAwaiter().GetResult();
        var paused = _connector.IsJobGroupPaused("JobKeyGroup").Result;
        Assert.IsTrue(paused);

        _connector.ResumeJobs(matcher).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>PauseTrigger</c> pauses the trigger group and that <c>ResumeTrigger</c> restores it.
    /// </summary>
    [TestMethod]
    public void PauseTrigger_And_ResumeTrigger()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");

        _connector.PauseTrigger(triggerKey).GetAwaiter().GetResult();
        var paused = _connector.IsTriggerGroupPaused("JobKeyGroup").Result;
        Assert.IsTrue(paused);

        _connector.ResumeTrigger(triggerKey).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>PauseTriggers</c> pauses the matching trigger group and that
    ///     <c>ResumeTriggers</c> restores it.
    /// </summary>
    [TestMethod]
    public void PauseTriggers_And_ResumeTriggers()
    {
        var matcher = new GroupMatcher<Quartz.TriggerKey>(GroupMatcherType.Equals, "JobKeyGroup");

        _connector.PauseTriggers(matcher).GetAwaiter().GetResult();
        var paused = _connector.IsTriggerGroupPaused("JobKeyGroup").Result;
        Assert.IsTrue(paused);

        _connector.ResumeTriggers(matcher).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>PauseAllTriggers</c> pauses all trigger groups and that
    ///     <c>ResumeAllTriggers</c> restores them.
    /// </summary>
    [TestMethod]
    public void PauseAllTriggers_And_ResumeAllTriggers()
    {
        _connector.PauseAllTriggers().GetAwaiter().GetResult();
        var paused = _connector.IsTriggerGroupPaused("JobKeyGroup").Result;
        Assert.IsTrue(paused);

        _connector.ResumeAllTriggers().GetAwaiter().GetResult();
    }
    #endregion

    #region Scheduling
    /// <summary>
    ///     Verifies that <c>ScheduleJob</c> with a combined job detail and trigger returns a valid first fire time.
    /// </summary>
    [TestMethod]
    public void ScheduleJob_WithJobDetailAndTrigger()
    {
        var request = new JobDetailWithTrigger(
            MakeJobDetail("ScheduledJob1", "TestGroup"),
            MakeTrigger("ScheduledTrigger1", "TestGroup", "ScheduledJob1", "TestGroup", "0 * * ? * *"));

        var fireTime = _connector.ScheduleJob(request).Result;
        Assert.AreNotEqual(DateTimeOffset.MinValue, fireTime);

        _connector.DeleteJob(new JobKey("ScheduledJob1", "TestGroup")).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>ScheduleJob</c> with a trigger-only payload schedules a trigger
    ///     against an already existing job.
    /// </summary>
    [TestMethod]
    public void ScheduleJob_WithTriggerOnly()
    {
        _connector.AddJob(MakeJobDetail("JobForTriggerOnly", "TestGroup")).GetAwaiter().GetResult();

        var trigger = MakeTrigger("TriggerOnly1", "TestGroup", "JobForTriggerOnly", "TestGroup", "0 * * ? * *");
        _connector.ScheduleJob(trigger).GetAwaiter().GetResult();

        var exists = _connector.CheckExists(new TriggerKey("TriggerOnly1", "TestGroup")).Result;
        Assert.IsTrue(exists);

        _connector.DeleteJob(new JobKey("JobForTriggerOnly", "TestGroup")).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>ScheduleJobWithTriggers</c> schedules a job with multiple triggers.
    /// </summary>
    [TestMethod]
    public void ScheduleJobWithTriggers()
    {
        var request = new JobDetailWithTriggers(
            MakeJobDetail("MultiTriggerJob", "TestGroup"),
            [
                MakeTrigger("MultiTrigger1", "TestGroup", "MultiTriggerJob", "TestGroup", "0 * * ? * *"),
                MakeTrigger("MultiTrigger2", "TestGroup", "MultiTriggerJob", "TestGroup", "0 30 * ? * *")
            ]);

        _connector.ScheduleJobWithTriggers(request).GetAwaiter().GetResult();

        var exists = _connector.CheckExists(new JobKey("MultiTriggerJob", "TestGroup")).Result;
        Assert.IsTrue(exists);

        _connector.DeleteJob(new JobKey("MultiTriggerJob", "TestGroup")).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>ScheduleJobs</c> schedules a batch of jobs in a single call.
    /// </summary>
    [TestMethod]
    public void ScheduleJobs_Multiple()
    {
        var jobs = new List<JobDetailWithTriggers>
        {
            new JobDetailWithTriggers(
                MakeJobDetail("BatchJob1", "BatchGroup"),
                [MakeTrigger("BatchTrigger1", "BatchGroup", "BatchJob1", "BatchGroup", "0 * * ? * *")])
        };

        _connector.ScheduleJobs(new ScheduleJobs(jobs, replace: true)).GetAwaiter().GetResult();

        var exists = _connector.CheckExists(new JobKey("BatchJob1", "BatchGroup")).Result;
        Assert.IsTrue(exists);

        _connector.DeleteJob(new JobKey("BatchJob1", "BatchGroup")).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>RescheduleJob</c> replaces an existing trigger with a new one
    ///  and returns a valid next fire time.
    /// </summary>
    [TestMethod]
    public void RescheduleJob_Test()
    {
        var reschedule = new RescheduleJob(
            new TriggerKey("triggerKey", "JobKeyGroup"),
            MakeTrigger("triggerKeyRescheduled", "JobKeyGroup", "JobKeyName", "JobKeyGroup", "0 30 * ? * *"));

        var fireTime = _connector.RescheduleJob(reschedule).Result;
        Assert.IsNotNull(fireTime);

        _connector.RescheduleJob(new RescheduleJob(
            new TriggerKey("triggerKeyRescheduled", "JobKeyGroup"),
            MakeTrigger("triggerKey", "JobKeyGroup", "JobKeyName", "JobKeyGroup", "0 * * ? * *")
        )).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>UnscheduleJob</c> removes the trigger and that the job can be
    ///     rescheduled afterwards.
    /// </summary>
    [TestMethod]
    public void UnscheduleJob_And_Reschedule()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");

        var unscheduled = _connector.UnscheduleJob(triggerKey).Result;
        Assert.IsTrue(unscheduled);

        var quartzTrigger = TriggerBuilder.Create()
            .WithIdentity("triggerKey", "JobKeyGroup")
            .ForJob(new Quartz.JobKey("JobKeyName", "JobKeyGroup"))
            .WithCronSchedule("0 * * ? * *")
            .Build();
        _connector.ScheduleJob(new Trigger(quartzTrigger)).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>UnscheduleJobs</c> removes multiple triggers in a single call.
    /// </summary>
    [TestMethod]
    public void UnscheduleJobs_Multiple()
    {
        _connector.AddJob(MakeJobDetail("UnscheduleJobA", "TestGroup")).GetAwaiter().GetResult();
        _connector.ScheduleJob(MakeTrigger("UnscheduleTriggerA", "TestGroup", "UnscheduleJobA", "TestGroup", "0 * * ? * *")).GetAwaiter().GetResult();
        _connector.ScheduleJob(MakeTrigger("UnscheduleTriggerB", "TestGroup", "UnscheduleJobA", "TestGroup", "0 30 * ? * *")).GetAwaiter().GetResult();

        var keys = new TriggerKeys([
            new Quartz.TriggerKey("UnscheduleTriggerA", "TestGroup"),
            new Quartz.TriggerKey("UnscheduleTriggerB", "TestGroup")
        ]);

        var result = _connector.UnscheduleJobs(keys).Result;
        Assert.IsTrue(result);

        _connector.DeleteJob(new JobKey("UnscheduleJobA", "TestGroup")).GetAwaiter().GetResult();
    }
    #endregion

    #region Job management
    /// <summary>
    ///     Verifies that <c>AddJob</c> persists a durable job and that <c>DeleteJob</c> removes it.
    /// </summary>
    [TestMethod]
    public void AddJob_And_DeleteJob()
    {
        _connector.AddJob(MakeJobDetail("AddedJob", "TestGroup")).GetAwaiter().GetResult();

        var exists = _connector.CheckExists(new JobKey("AddedJob", "TestGroup")).Result;
        Assert.IsTrue(exists);

        var deleted = _connector.DeleteJob(new JobKey("AddedJob", "TestGroup")).Result;
        Assert.IsTrue(deleted);

        var existsAfter = _connector.CheckExists(new JobKey("AddedJob", "TestGroup")).Result;
        Assert.IsFalse(existsAfter);
    }

    /// <summary>
    ///     Verifies that <c>DeleteJobs</c> removes multiple jobs in a single call.
    /// </summary>
    [TestMethod]
    public void DeleteJobs_Multiple()
    {
        _connector.AddJob(MakeJobDetail("DeleteJobA", "TestGroup")).GetAwaiter().GetResult();
        _connector.AddJob(MakeJobDetail("DeleteJobB", "TestGroup")).GetAwaiter().GetResult();

        var keys = new JobKeys([
            new Quartz.JobKey("DeleteJobA", "TestGroup"),
            new Quartz.JobKey("DeleteJobB", "TestGroup")
        ]);

        var result = _connector.DeleteJobs(keys).Result;
        Assert.IsTrue(result);

        Assert.IsFalse(_connector.CheckExists(new JobKey("DeleteJobA", "TestGroup")).Result);
        Assert.IsFalse(_connector.CheckExists(new JobKey("DeleteJobB", "TestGroup")).Result);
    }

    /// <summary>
    ///     Verifies that <c>TriggerJob</c> fires a job immediately without error.
    /// </summary>
    [TestMethod]
    public void TriggerJob_ByJobKey()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        _connector.TriggerJob(jobKey).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>TriggerJob</c> fires a job immediately with an additional data map.
    /// </summary>
    [TestMethod]
    public void TriggerJob_WithDataMap()
    {
        var jobKeyWithMap = new JobKeyWithDataMap(
            new JobKey("JobKeyName", "JobKeyGroup"),
            new JobDataMap { { "param1", "value1" } });

        _connector.TriggerJob(jobKeyWithMap).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>Clear</c> removes all jobs and triggers from an isolated scheduler.
    /// </summary>
    [TestMethod]
    public void Clear()
    {
        var properties = new NameValueCollection
        {
            ["quartz.scheduler.instanceName"] = "ClearTestScheduler",
            ["quartz.jobStore.type"]          = "Quartz.Simpl.RAMJobStore, Quartz",
            ["quartz.threadPool.type"]        = "Quartz.Simpl.SimpleThreadPool, Quartz"
        };

        var scheduler = new StdSchedulerFactory(properties).GetScheduler().Result;
        scheduler.Start();

        var job = JobBuilder.Create<TestJob>()
            .WithIdentity("ClearJob", "ClearGroup")
            .Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity("ClearTrigger", "ClearGroup")
            .WithCronSchedule("0 * * ? * *")
            .Build();
        scheduler.ScheduleJob(job, trigger);

        var isolatedHost = new SchedulerHost("http://localhost:44345", scheduler, null);
        isolatedHost.Start();
        try
        {
            var isolatedConnector = new SchedulerConnector("http://localhost:44345");
            isolatedConnector.Clear().GetAwaiter().GetResult();

            var groups = isolatedConnector.GetJobGroupNames().Result;
            Assert.IsFalse(groups.Any());
        }
        finally
        {
            isolatedHost.Stop();
        }
    }
    #endregion

    #region Calendars
    /// <summary>
    ///    Verifies that <c>GetCalendarNames</c> returns the calendar that was added during setup.
    /// </summary>
    [TestMethod]
    public void GetCalendarNames()
    {
        var names = _connector.GetCalendarNames().Result;
        Assert.IsTrue(names.Contains("monthlyCalendar"));
    }

    /// <summary>
    /// Verifies that <c>GetCalendar</c> returns a non-empty JSON representation of the calendar.
    /// </summary>
    [TestMethod]
    public void GetCalendar()
    {
        var json = _connector.GetCalendar("monthlyCalendar").Result;
        Assert.IsFalse(string.IsNullOrWhiteSpace(json));
    }

    /// <summary>
    ///     Verifies that <c>AddCalendar</c> persists a new calendar and that <c>DeleteCalendar</c>
    ///     removes it afterwards.
    /// </summary>
    [TestMethod]
    public void AddCalendar_And_DeleteCalendar()
    {
        var quartzCalendar = new Quartz.Impl.Calendar.MonthlyCalendar();
        var calendar = new Wrappers.Calendars.MonthlyCalendar(quartzCalendar)
        {
            Name = "testCalendar",
            Replace = false,
            UpdateTriggers = false
        };

        _connector.AddCalendar(calendar).GetAwaiter().GetResult();
        var names = _connector.GetCalendarNames().Result;
        Assert.IsTrue(names.Contains("testCalendar"));

        var deleted = _connector.DeleteCalendar("testCalendar").Result;
        Assert.IsTrue(deleted);
    }
    #endregion

    #region Interrupt
    /// <summary>
    ///     Verifies that <c>InterruptJob</c> by job key completes without error regardless of
    ///     whether the job is currently executing.
    /// </summary>
    [TestMethod]
    public void InterruptJob_ByJobKey()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        _connector.InterruptJob(jobKey).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Verifies that <c>InterruptJob</c> by a non-existent fire instance ID returns <c>false</c>.
    /// </summary>
    [TestMethod]
    public void InterruptJob_ByFireInstanceId_NotFound()
    {
        var result = _connector.InterruptJob("nonexistent-fire-id").Result;
        Assert.IsFalse(result);
    }
    #endregion

    #region Error state
    /// <summary>
    ///     Verifies that <c>ResetTriggerFromErrorState</c> completes without error when the
    ///     trigger is not in an error state.
    /// </summary>
    [TestMethod]
    public void ResetTriggerFromErrorState()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");
        _connector.ResetTriggerFromErrorState(triggerKey).GetAwaiter().GetResult();
    }
    #endregion
}
