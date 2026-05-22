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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Quartz;
using Quartz.Impl;
using QuartzRestApi.Models;
using GroupMatcherType = QuartzRestApi.Models.GroupMatcherType;
using JobKey = QuartzRestApi.Models.JobKey;
using RescheduleJob = QuartzRestApi.Models.RescheduleJob;
using TriggerKey = QuartzRestApi.Models.TriggerKey;
using TestJob = QuartzRestApi.FallbackNoOpJob;

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
    private static SchedulerHost _host;

    /// <summary>
    ///     The SchedulerConnector instance used to interact with the SchedulerHost's API during tests. This connector is initialized in the test setup and provides methods for querying and manipulating the scheduler state.
    /// </summary>
    private static SchedulerConnector _connector;
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
    [ClassInitialize]
    public static async Task InitializeClass(TestContext context)
    {
        var properties = new NameValueCollection
        {
            ["quartz.scheduler.instanceName"] = "SchedulerInstance",
            ["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz",
            ["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz"
        };

        var scheduler = new StdSchedulerFactory(properties).GetScheduler().GetAwaiter().GetResult();

        scheduler.Context.Add("key1", "value1");
        scheduler.Context.Add("key2", "value2");
        scheduler.Context.Add("key3", "value3");

        await scheduler.Start();

        var schedulerJob = JobBuilder.Create<TestJob>()
            .WithIdentity(new Quartz.JobKey("JobKeyName", "JobKeyGroup"))
            .WithDescription("Test")
            .RequestRecovery()
            .StoreDurably()
            .Build();

        var schedulerTrigger = (ISimpleTrigger)TriggerBuilder.Create()
            .WithIdentity("TriggerKey", "JobKeyGroup")
            .WithDescription("TestTrigger")
            .StartAt(DateTime.Now.AddHours(1))
            .Build();

        var mc = new Quartz.Impl.Calendar.MonthlyCalendar();
        mc.SetDayExcluded(1, true);
        mc.SetDayExcluded(12, true);
        await scheduler.AddCalendar("monthlyCalendar", mc, true, true);
        await scheduler.ScheduleJob(schedulerJob, schedulerTrigger);

        _host = new SchedulerHost("http://localhost:44344", scheduler, (string)null);
        await _host.Start();

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
    [ClassCleanup]
    public static async Task CleanupClass()
    {
        await _host.Stop();
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
    public async Task IsJobGroupPaused()
    {
        var paused = await _connector.IsJobGroupPaused("JobKeyGroup");
        Assert.IsFalse(paused);
    }

    /// <summary>
    ///     Verifies that <c>IsTriggerGroupPaused</c> returns <c>false</c> for an active trigger group.
    /// </summary>
    [TestMethod]
    public async Task IsTriggerGroupPaused()
    {
        var paused = await _connector.IsTriggerGroupPaused("JobKeyGroup");
        Assert.IsFalse(paused);
    }

    /// <summary>
    ///     Verifies that <c>SchedulerName</c> returns the configured instance name.
    /// </summary>
    [TestMethod]
    public async Task SchedulerName()
    {
        var name = await _connector.SchedulerName();
        Assert.AreEqual("SchedulerInstance", name);
    }

    /// <summary>
    ///     Verifies that <c>SchedulerInstanceId</c> returns the expected non-clustered identifier.
    /// </summary>
    [TestMethod]
    public async Task SchedulerInstanceId()
    {
        var id = await _connector.SchedulerInstanceId();
        Assert.AreEqual("NON_CLUSTERED", id);
    }

    /// <summary>
    ///     Verifies that <c>Context</c> returns all key/value pairs that were added during setup.
    /// </summary>
    [TestMethod]
    public async Task Context()
    {
        var result = await _connector.Context();
        Assert.AreEqual("value1", result["key1"].ToString());
        Assert.AreEqual("value2", result["key2"].ToString());
        Assert.AreEqual("value3", result["key3"].ToString());
    }

    /// <summary>
    ///     Verifies that <c>InStandbyMode</c> returns <c>false</c> when the scheduler is running.
    /// </summary>
    [TestMethod]
    public async Task InStandbyMode()
    {
        var result = await _connector.InStandbyMode();
        Assert.IsFalse(result);
    }

    /// <summary>
    ///     Verifies that <c>IsShutdown</c> returns <c>false</c> when the scheduler is running.
    /// </summary>
    [TestMethod]
    public async Task IsShutdown()
    {
        var result = await _connector.IsShutdown();
        Assert.IsFalse(result);
    }

    /// <summary>
    ///     Verifies that <c>IsStarted</c> returns <c>true</c> when the scheduler is running.
    /// </summary>
    [TestMethod]
    public async Task IsStarted()
    {
        var result = await _connector.IsStarted();
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Verifies that <c>GetMetaData</c> returns a summary containing the word "Quartz".
    /// </summary>
    [TestMethod]
    public async Task GetMetaData()
    {
        var result = await _connector.GetMetaData();
        Assert.IsTrue(result.Summary.Contains("Quartz"));
    }

    /// <summary>
    ///     Verifies that <c>Start</c> completes without error when the scheduler is already running.
    /// </summary>
    [TestMethod]
    public async Task Start()
    {
        await _connector.Start();
    }

    /// <summary>
    /// Verifies that <c>StartDelayed</c> completes without error.
    /// </summary>
    [TestMethod]
    public async Task StartDelayed()
    {
        await _connector.StartDelayed(1);
    }

    /// <summary>
    ///     Verifies that calling <c>Standby</c> puts the scheduler in standby mode
    ///     and that <c>Start</c> brings it back to running.
    /// </summary>
    [TestMethod]
    public async Task Standby_Then_Start()
    {
        await _connector.Standby();
        var standby = await _connector.InStandbyMode();
        Assert.IsTrue(standby);

        await _connector.Start();
        var active = await _connector.InStandbyMode();
        Assert.IsFalse(active);
    }

    /// <summary>
    ///     Verifies that <c>Shutdown</c> stops an isolated scheduler instance without affecting
    ///     the shared test scheduler.
    /// </summary>
    [TestMethod]
    public async Task Shutdown_WithWaitForJobsToComplete()
    {
        var properties = new NameValueCollection
        {
            ["quartz.scheduler.instanceName"] = "ShutdownTestScheduler",
            ["quartz.jobStore.type"]          = "Quartz.Simpl.RAMJobStore, Quartz",
            ["quartz.threadPool.type"]        = "Quartz.Simpl.SimpleThreadPool, Quartz"
        };

        var scheduler = await new StdSchedulerFactory(properties).GetScheduler();
        scheduler.Start().GetAwaiter().GetResult();

        var isolatedHost = new SchedulerHost("http://localhost:44346", scheduler, (string)null);
        await isolatedHost.Start();
        try
        {
            var isolatedConnector = new SchedulerConnector("http://localhost:44346");
            await isolatedConnector.Shutdown(waitForJobsToComplete: false);

            var isShutdown = await isolatedConnector.IsShutdown();
            Assert.IsTrue(isShutdown);
        }
        finally
        {
            await isolatedHost.Stop();
        }
    }
    #endregion

    #region Groups
    /// <summary>
    ///     Verifies that <c>GetJobGroupNames</c> returns the group that was created during setup.
    /// </summary>
    [TestMethod]
    public async Task GetJobGroupNames()
    {
        var groups = await _connector.GetJobGroupNames();
        Assert.IsTrue(groups.Contains("JobKeyGroup"));
    }

    /// <summary>
    ///     Verifies that <c>GetTriggerGroupNames</c> returns the group that was created during setup.
    /// </summary>
    [TestMethod]
    public async Task GetTriggerGroupNames()
    {
        var groups = await _connector.GetTriggerGroupNames();
        Assert.IsTrue(groups.Contains("JobKeyGroup"));
    }

    /// <summary>
    ///     Verifies that <c>GetPausedTriggerGroups</c> returns a non-null result.
    /// </summary>
    [TestMethod]
    public async Task GetPausedTriggerGroups()
    {
        var groups = await _connector.GetPausedTriggerGroups();
        Assert.IsNotNull(groups);
    }
    #endregion

    #region Keys
    /// <summary>
    ///     Verifies that <c>GetJobKeys</c> returns the job key that was added during setup
    ///     when filtering by its group with an Equals matcher.
    /// </summary>
    [TestMethod]
    public async Task GetJobKeys()
    {
        var matcher = new GroupMatcher<Quartz.JobKey>(GroupMatcherType.Equals, "JobKeyGroup");
        var keys = await _connector.GetJobKeys(matcher);
        Assert.IsTrue(keys.Any(k => k.Name == "JobKeyName"));
    }

    /// <summary>
    /// Verifies that <c>GetTriggerKeys</c> returns the trigger key that was added during setup
    /// when filtering by its group with an Equals matcher.
    /// </summary>
    [TestMethod]
    public async Task GetTriggerKeys()
    {
        var matcher = new GroupMatcher<Quartz.TriggerKey>(GroupMatcherType.Equals, "JobKeyGroup");
        var keys = await _connector.GetTriggerKeys(matcher);
        Assert.IsTrue(keys.Any(k => k.Name == "TriggerKey"));
    }
    #endregion

    #region Job and trigger detail
    /// <summary>
    ///     Verifies that <c>GetJobDetail</c> returns the correct detail for an existing job.
    /// </summary>
    [TestMethod]
    public async Task GetJobDetail()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        var detail = await _connector.GetJobDetail(jobKey);
        Assert.IsNotNull(detail);
        Assert.AreEqual("JobKeyName", detail.JobKey.Name);
    }

    /// <summary>
    ///     Verifies that <c>GetTriggersOfJob</c> returns the trigger that was associated with
    ///     the job during setup.
    /// </summary>
    [TestMethod]
    public async Task GetTriggersOfJob()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        var triggers = await _connector.GetTriggersOfJob(jobKey);
        Assert.IsTrue(triggers.Any(t => t.TriggerKey.Name == "TriggerKey"));
    }

    /// <summary>
    ///     Verifies that <c>GetTrigger</c> returns the correct trigger for an existing trigger key.
    /// </summary>
    [TestMethod]
    public async Task GetTrigger()
    {
        var triggerKey = new TriggerKey("TriggerKey", "JobKeyGroup");
        var trigger = await _connector.GetTrigger(triggerKey);
        Assert.IsNotNull(trigger);
        Assert.AreEqual("TriggerKey", trigger.TriggerKey.Name);
    }

    /// <summary>
    ///     Verifies that <c>GetTriggerState</c> returns a non-empty state string for an existing trigger.
    /// </summary>
    [TestMethod]
    public async Task GetTriggerState()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");
        var state = await _connector.GetTriggerState(triggerKey);
        Assert.IsFalse(string.IsNullOrWhiteSpace(state));
    }

    /// <summary>
    ///     Verifies that <c>GetCurrentlyExecutingJobs</c> returns a non-null result.
    /// </summary>
    [TestMethod]
    public async Task GetCurrentlyExecutingJobs()
    {
        var result = await _connector.GetCurrentlyExecutingJobs();
        Assert.IsNotNull(result);
    }
    #endregion

    #region Existence checks
    /// <summary>
    ///     Verifies that <c>CheckExists</c> returns <c>true</c> for a job key that is present in the scheduler.
    /// </summary>
    [TestMethod]
    public async Task CheckExistsJobKey_Existing()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        var exists = await _connector.CheckExists(jobKey);
        Assert.IsTrue(exists);
    }

    /// <summary>
    ///     Verifies that <c>CheckExists</c> returns <c>false</c> for a job key that is not in the scheduler.
    /// </summary>
    [TestMethod]
    public async Task CheckExistsJobKey_NotExisting()
    {
        var jobKey = new JobKey("DoesNotExist", "JobKeyGroup");
        var exists = await _connector.CheckExists(jobKey);
        Assert.IsFalse(exists);
    }

    /// <summary>
    ///     Verifies that <c>CheckExists</c> returns <c>true</c> for a trigger key that is present in the scheduler.
    /// </summary>
    [TestMethod]
    public async Task CheckExistsTriggerKey_Existing()
    {
        var triggerKey = new TriggerKey("TriggerKey", "JobKeyGroup");
        var exists = await _connector.CheckExists(triggerKey);
        Assert.IsTrue(exists);
    }

    /// <summary>
    ///     Verifies that <c>CheckExists</c> returns <c>false</c> for a trigger key that is not in the scheduler.
    /// </summary>
    [TestMethod]
    public async Task CheckExistsTriggerKey_NotExisting()
    {
        var triggerKey = new TriggerKey("DoesNotExist", "JobKeyGroup");
        var exists = await _connector.CheckExists(triggerKey);
        Assert.IsFalse(exists);
    }
    #endregion

    #region Pause and resume
    /// <summary>
    ///     Verifies that <c>PauseJob</c> pauses the job group and that <c>ResumeJob</c> restores it.
    /// </summary>
    [TestMethod]
    public async Task PauseJob_And_ResumeJob()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        var triggerKey = new TriggerKey("TriggerKey", "JobKeyGroup");

        await _connector.PauseJob(jobKey);
        var state = await _connector.GetTriggerState(triggerKey);
        Assert.AreEqual("Paused", state);

        await _connector.ResumeJob(jobKey);
    }

    /// <summary>
    ///     Verifies that <c>PauseJobs</c> pauses the matching job group and that <c>ResumeJobs</c> restores it.
    /// </summary>
    [TestMethod]
    public async Task PauseJobs_And_ResumeJobs()
    {
        var matcher = new GroupMatcher<Quartz.JobKey>(GroupMatcherType.Equals, "JobKeyGroup");

        await _connector.PauseJobs(matcher);
        var paused = await _connector.IsJobGroupPaused("JobKeyGroup");
        Assert.IsTrue(paused);

        await _connector.ResumeJobs(matcher);
    }

    /// <summary>
    ///     Verifies that <c>PauseTrigger</c> pauses the trigger group and that <c>ResumeTrigger</c> restores it.
    /// </summary>
    [TestMethod]
    public async Task PauseTrigger_And_ResumeTrigger()
    {
        var triggerKey = new TriggerKey("TriggerKey", "JobKeyGroup");

        await _connector.PauseTrigger(triggerKey);
        var state = await _connector.GetTriggerState(triggerKey);
        Assert.AreEqual("Paused", state);

        await _connector.ResumeTrigger(triggerKey);
    }

    /// <summary>
    ///     Verifies that <c>PauseTriggers</c> pauses the matching trigger group and that
    ///     <c>ResumeTriggers</c> restores it.
    /// </summary>
    [TestMethod]
    public async Task PauseTriggers_And_ResumeTriggers()
    {
        var matcher = new GroupMatcher<Quartz.TriggerKey>(GroupMatcherType.Equals, "JobKeyGroup");

        await _connector.PauseTriggers(matcher);
        var paused = await _connector.IsTriggerGroupPaused("JobKeyGroup");
        Assert.IsTrue(paused);

        await _connector.ResumeTriggers(matcher);
    }

    /// <summary>
    ///     Verifies that <c>PauseAllTriggers</c> pauses all trigger groups and that
    ///     <c>ResumeAllTriggers</c> restores them.
    /// </summary>
    [TestMethod]
    public async Task PauseAllTriggers_And_ResumeAllTriggers()
    {
        await _connector.PauseAllTriggers();
        var paused = await _connector.IsTriggerGroupPaused("JobKeyGroup");
        Assert.IsTrue(paused);

        await _connector.ResumeAllTriggers();
    }
    #endregion

    #region Scheduling
    /// <summary>
    ///     Verifies that <c>ScheduleJob</c> with a combined job detail and trigger returns a valid first fire time.
    /// </summary>
    [TestMethod]
    public async Task ScheduleJob_WithJobDetailAndTrigger()
    {
        var request = new JobDetailWithTrigger(
            MakeJobDetail("ScheduledJob1", "TestGroup"),
            MakeTrigger("ScheduledTrigger1", "TestGroup", "ScheduledJob1", "TestGroup", "0 * * ? * *"));

        var fireTime = await _connector.ScheduleJob(request);
        Assert.AreNotEqual(DateTimeOffset.MinValue, fireTime);

        await _connector.DeleteJob(new JobKey("ScheduledJob1", "TestGroup"));
    }

    /// <summary>
    ///     Verifies that <c>ScheduleJob</c> with a trigger-only payload schedules a trigger
    ///     against an already existing job.
    /// </summary>
    [TestMethod]
    public async Task ScheduleJob_WithTriggerOnly()
    {
        await _connector.AddJob(MakeJobDetail("JobForTriggerOnly", "TestGroup"));

        var trigger = MakeTrigger("TriggerOnly1", "TestGroup", "JobForTriggerOnly", "TestGroup", "0 * * ? * *");
        await _connector.ScheduleJob(trigger);

        var exists = await _connector.CheckExists(new TriggerKey("TriggerOnly1", "TestGroup"));
        Assert.IsTrue(exists);

        await _connector.DeleteJob(new JobKey("JobForTriggerOnly", "TestGroup"));
    }

    /// <summary>
    ///     Verifies that <c>ScheduleJobWithTriggers</c> schedules a job with multiple triggers.
    /// </summary>
    [TestMethod]
    public async Task ScheduleJobWithTriggers()
    {
        var request = new JobDetailWithTriggers(
            MakeJobDetail("MultiTriggerJob", "TestGroup"),
            new List<Trigger>
            {
                MakeTrigger("MultiTrigger1", "TestGroup", "MultiTriggerJob", "TestGroup", "0 * * ? * *"),
                MakeTrigger("MultiTrigger2", "TestGroup", "MultiTriggerJob", "TestGroup", "0 30 * ? * *")
            });

        await _connector.ScheduleJobWithTriggers(request);

        var exists = await _connector.CheckExists(new JobKey("MultiTriggerJob", "TestGroup"));
        Assert.IsTrue(exists);

        await _connector.DeleteJob(new JobKey("MultiTriggerJob", "TestGroup"));
    }

    /// <summary>
    ///     Verifies that <c>ScheduleJobs</c> schedules a batch of jobs in a single call.
    /// </summary>
    [TestMethod]
    public async Task ScheduleJobs_Multiple()
    {
        var jobs = new List<JobDetailWithTriggers>
        {
            new(MakeJobDetail("BatchJob1", "BatchGroup"),
                new List<Trigger>
                    { MakeTrigger("BatchTrigger1", "BatchGroup", "BatchJob1", "BatchGroup", "0 * * ? * *") })
        };

        await _connector.ScheduleJobs(new ScheduleJobs(jobs, replace: true));

        var exists = await _connector.CheckExists(new JobKey("BatchJob1", "BatchGroup"));
        Assert.IsTrue(exists);

        await _connector.DeleteJob(new JobKey("BatchJob1", "BatchGroup"));
    }

    /// <summary>
    ///     Verifies that <c>RescheduleJob</c> replaces an existing trigger with a new one
    ///     and returns a valid next fire time.
    /// </summary>
    [TestMethod]
    public async Task RescheduleJob_Test()
    {
        var reschedule = new RescheduleJob(
            new TriggerKey("TriggerKey", "JobKeyGroup"),
            MakeTrigger("TriggerKeyRescheduled", "JobKeyGroup", "JobKeyName", "JobKeyGroup", "0 30 * ? * *"));

        var fireTime = await _connector.RescheduleJob(reschedule);
        Assert.IsNotNull(fireTime);

        await _connector.RescheduleJob(new RescheduleJob(
            new TriggerKey("TriggerKeyRescheduled", "JobKeyGroup"),
            MakeTrigger("TriggerKey", "JobKeyGroup", "JobKeyName", "JobKeyGroup", "0 * * ? * *")
        ));
    }

    /// <summary>
    ///     Verifies that <c>UnscheduleJob</c> removes the trigger and that the job can be
    ///     rescheduled afterwards.
    /// </summary>
    [TestMethod]
    public async Task UnscheduleJob_And_Reschedule()
    {
        var triggerKey = new TriggerKey("TriggerKey", "JobKeyGroup");

        var unscheduled = await _connector.UnscheduleJob(triggerKey);
        Assert.IsTrue(unscheduled);

        var quartzTrigger = TriggerBuilder.Create()
            .WithIdentity("TriggerKey", "JobKeyGroup")
            .ForJob(new Quartz.JobKey("JobKeyName", "JobKeyGroup"))
            .WithCronSchedule("0 * * ? * *")
            .Build();
        await _connector.ScheduleJob(new Trigger(quartzTrigger));
    }

    /// <summary>
    ///     Verifies that <c>UnscheduleJobs</c> removes multiple triggers in a single call.
    /// </summary>
    [TestMethod]
    public async Task UnscheduleJobs_Multiple()
    {
        await _connector.AddJob(MakeJobDetail("UnscheduleJobA", "TestGroup"));
        await _connector.ScheduleJob(MakeTrigger("UnscheduleTriggerA", "TestGroup", "UnscheduleJobA", "TestGroup", "0 * * ? * *"));
        await _connector.ScheduleJob(MakeTrigger("UnscheduleTriggerB", "TestGroup", "UnscheduleJobA", "TestGroup", "0 30 * ? * *"));

        var keys = new TriggerKeys(new List<Quartz.TriggerKey>
        {
            new Quartz.TriggerKey("UnscheduleTriggerA", "TestGroup"),
            new Quartz.TriggerKey("UnscheduleTriggerB", "TestGroup")
        }.AsReadOnly());

        var result = await _connector.UnscheduleJobs(keys);
        Assert.IsTrue(result);

        await _connector.DeleteJob(new JobKey("UnscheduleJobA", "TestGroup"));
    }
    #endregion

    #region Job management
    /// <summary>
    ///     Verifies that <c>AddJob</c> persists a durable job and that <c>DeleteJob</c> removes it.
    /// </summary>
    [TestMethod]
    public async Task AddJob_And_DeleteJob()
    {
        await _connector.AddJob(MakeJobDetail("AddedJob", "TestGroup"));

        var exists = await _connector.CheckExists(new JobKey("AddedJob", "TestGroup"));
        Assert.IsTrue(exists);

        var deleted = await _connector.DeleteJob(new JobKey("AddedJob", "TestGroup"));
        Assert.IsTrue(deleted);

        var existsAfter = await _connector.CheckExists(new JobKey("AddedJob", "TestGroup"));
        Assert.IsFalse(existsAfter);
    }

    /// <summary>
    ///     Verifies that <c>DeleteJobs</c> removes multiple jobs in a single call.
    /// </summary>
    [TestMethod]
    public async Task DeleteJobs_Multiple()
    {
        await _connector.AddJob(MakeJobDetail("DeleteJobA", "TestGroup"));
        await _connector.AddJob(MakeJobDetail("DeleteJobB", "TestGroup"));

        var keys = new JobKeys(new List<Quartz.JobKey>
        {
            new Quartz.JobKey("DeleteJobA", "TestGroup"),
            new Quartz.JobKey("DeleteJobB", "TestGroup")
        }.AsReadOnly());

        var result = await _connector.DeleteJobs(keys);
        Assert.IsTrue(result);

        Assert.IsFalse(await _connector.CheckExists(new JobKey("DeleteJobA", "TestGroup")));
        Assert.IsFalse(await _connector.CheckExists(new JobKey("DeleteJobB", "TestGroup")));
    }

    /// <summary>
    ///     Verifies that <c>TriggerJob</c> fires a job immediately without error.
    /// </summary>
    [TestMethod]
    public async Task TriggerJob_ByJobKey()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        await _connector.TriggerJob(jobKey);
    }

    /// <summary>
    ///     Verifies that <c>TriggerJob</c> fires a job immediately with an additional data map.
    /// </summary>
    [TestMethod]
    public async Task TriggerJob_WithDataMap()
    {
        var jobKeyWithMap = new JobKeyWithDataMap(
            new JobKey("JobKeyName", "JobKeyGroup"),
            new JobDataMap { { "param1", "value1" } });

        await _connector.TriggerJob(jobKeyWithMap);
    }

    /// <summary>
    ///     Verifies that <c>Clear</c> removes all jobs and triggers from an isolated scheduler.
    /// </summary>
    [TestMethod]
    public async Task Clear()
    {
        var properties = new NameValueCollection
        {
            ["quartz.scheduler.instanceName"] = "ClearTestScheduler",
            ["quartz.jobStore.type"]          = "Quartz.Simpl.RAMJobStore, Quartz",
            ["quartz.threadPool.type"]        = "Quartz.Simpl.SimpleThreadPool, Quartz"
        };

        var scheduler = await new StdSchedulerFactory(properties).GetScheduler();
        scheduler.Start().GetAwaiter().GetResult();

        var job = JobBuilder.Create<TestJob>()
            .WithIdentity("ClearJob", "ClearGroup")
            .Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity("ClearTrigger", "ClearGroup")
            .WithCronSchedule("0 * * ? * *")
            .Build();
        await scheduler.ScheduleJob(job, trigger);

        var isolatedHost = new SchedulerHost("http://localhost:44345", scheduler, (string)null);
        await isolatedHost.Start();
        try
        {
            var isolatedConnector = new SchedulerConnector("http://localhost:44345");
            await isolatedConnector.Clear();

            var groups = await isolatedConnector.GetJobGroupNames();
            Assert.IsFalse(groups.Any());
        }
        finally
        {
            await isolatedHost.Stop();
        }
    }
    #endregion

    #region Calendars
    /// <summary>
    ///    Verifies that <c>GetCalendarNames</c> returns the calendar that was added during setup.
    /// </summary>
    [TestMethod]
    public async Task GetCalendarNames()
    {
        var names = await _connector.GetCalendarNames();
        Assert.IsTrue(names.Contains("monthlyCalendar"));
    }

    /// <summary>
    /// Verifies that <c>GetCalendar</c> returns a non-empty JSON representation of the calendar.
    /// </summary>
    [TestMethod]
    public async Task GetCalendar()
    {
        var json = await _connector.GetCalendar("monthlyCalendar");
        Assert.IsFalse(string.IsNullOrWhiteSpace(json));
    }

    /// <summary>
    ///     Verifies that <c>AddCalendar</c> persists a new calendar and that <c>DeleteCalendar</c>
    ///     removes it afterwards.
    /// </summary>
    [TestMethod]
    public async Task AddCalendar_And_DeleteCalendar()
    {
        var quartzCalendar = new Quartz.Impl.Calendar.MonthlyCalendar();
        var calendar = new Models.Calendars.MonthlyCalendar(quartzCalendar)
        {
            Name = "testCalendar",
            Replace = false,
            UpdateTriggers = false
        };

        await _connector.AddCalendar(calendar);
        var names = await _connector.GetCalendarNames();
        Assert.IsTrue(names.Contains("testCalendar"));

        var deleted = await _connector.DeleteCalendar("testCalendar");
        Assert.IsTrue(deleted);
    }
    #endregion

    #region Interrupt
    /// <summary>
    ///     Verifies that <c>InterruptJob</c> by job key completes without error regardless of
    ///     whether the job is currently executing.
    /// </summary>
    [TestMethod]
    public async Task InterruptJob_ByJobKey()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        await _connector.InterruptJob(jobKey);
    }

    /// <summary>
    ///     Verifies that <c>InterruptJob</c> by a non-existent fire instance ID returns <c>false</c>.
    /// </summary>
    [TestMethod]
    public async Task InterruptJob_ByFireInstanceId_NotFound()
    {
        var result = await _connector.InterruptJob("nonexistent-fire-id");
        Assert.IsFalse(result);
    }
    #endregion

    #region Error state
    /// <summary>
    ///     Verifies that <c>ResetTriggerFromErrorState</c> completes without error when the
    ///     trigger is not in an error state.
    /// </summary>
    [TestMethod]
    public async Task ResetTriggerFromErrorState()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");
        await _connector.ResetTriggerFromErrorState(triggerKey);
    }
    #endregion
}

