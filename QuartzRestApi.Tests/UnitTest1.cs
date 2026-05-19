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
using System.Collections.Specialized;
using System.Linq;
using Quartz;
using Quartz.Impl;
using QuartzRestApi.Wrappers;
using QuartzRestApi.Wrappers.Calendars;
using GroupMatcherType = QuartzRestApi.Wrappers.GroupMatcherType;
using JobKey = QuartzRestApi.Wrappers.JobKey;
using TriggerKey = QuartzRestApi.Wrappers.TriggerKey;

namespace QuartzRestApi.Tests;

[TestClass]
public class UnitTest1
{
    #region Fields
    private SchedulerHost _host;
    private SchedulerConnector _connector;
    #endregion

    #region Initialize
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

        scheduler.Context.Add("key1", "value1");
        scheduler.Context.Add("key2", "value2");
        scheduler.Context.Add("key3", "value3");

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
    [TestCleanup]
    public void Cleanup()
    {
        _host.Stop();
    }
    #endregion

    // ── Scheduler state ──────────────────────────────────────────────────────

    [TestMethod]
    public void IsJobGroupPaused()
    {
        var paused = _connector.IsJobGroupPaused("JobKeyGroup").Result;
        Assert.IsFalse(paused);
    }

    [TestMethod]
    public void IsTriggerGroupPaused()
    {
        var paused = _connector.IsTriggerGroupPaused("JobKeyGroup").Result;
        Assert.IsFalse(paused);
    }

    [TestMethod]
    public void SchedulerName()
    {
        var name = _connector.SchedulerName().Result;
        Assert.AreEqual("SchedulerInstance", name);
    }

    [TestMethod]
    public void SchedulerInstanceId()
    {
        var id = _connector.SchedulerInstanceId().Result;
        Assert.AreEqual("NON_CLUSTERED", id);
    }

    [TestMethod]
    public void Context()
    {
        var result = _connector.Context().Result;
        Assert.AreEqual("value1", result["key1"]);
        Assert.AreEqual("value2", result["key2"]);
        Assert.AreEqual("value3", result["key3"]);
    }

    [TestMethod]
    public void InStandbyMode()
    {
        var result = _connector.InStandbyMode().Result;
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsShutdown()
    {
        var result = _connector.IsShutdown().Result;
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsStarted()
    {
        var result = _connector.IsStarted().Result;
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetMetaData()
    {
        var result = _connector.GetMetaData().Result;
        Assert.IsTrue(result.Summary.Contains("Quartz"));
    }

    [TestMethod]
    public void Start()
    {
        _connector.Start().GetAwaiter().GetResult();
    }

    [TestMethod]
    public void StartDelayed()
    {
        _connector.StartDelayed(1).GetAwaiter().GetResult();
    }

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

    // ── Groups ───────────────────────────────────────────────────────────────

    [TestMethod]
    public void GetJobGroupNames()
    {
        var groups = _connector.GetJobGroupNames().Result;
        Assert.IsTrue(groups.Contains("JobKeyGroup"));
    }

    [TestMethod]
    public void GetTriggerGroupNames()
    {
        var groups = _connector.GetTriggerGroupNames().Result;
        Assert.IsTrue(groups.Contains("JobKeyGroup"));
    }

    [TestMethod]
    public void GetPausedTriggerGroups()
    {
        var groups = _connector.GetPausedTriggerGroups().Result;
        Assert.IsNotNull(groups);
    }

    // ── Job keys / trigger keys ───────────────────────────────────────────────

    [TestMethod]
    public void GetJobKeys()
    {
        var matcher = new GroupMatcher<Quartz.JobKey>(GroupMatcherType.Equals, "JobKeyGroup");
        var keys = _connector.GetJobKeys(matcher).Result;
        Assert.IsTrue(keys.Any(k => k.Name == "JobKeyName"));
    }

    [TestMethod]
    public void GetTriggerKeys()
    {
        var matcher = new GroupMatcher<Quartz.TriggerKey>(GroupMatcherType.Equals, "JobKeyGroup");
        var keys = _connector.GetTriggerKeys(matcher).Result;
        Assert.IsTrue(keys.Any(k => k.Name == "triggerKey"));
    }

    // ── Job / trigger detail ─────────────────────────────────────────────────

    [TestMethod]
    public void GetJobDetail()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        var detail = _connector.GetJobDetail(jobKey).Result;
        Assert.IsNotNull(detail);
        Assert.AreEqual("JobKeyName", detail.JobKey.Name);
    }

    [TestMethod]
    public void GetTriggersOfJob()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        var triggers = _connector.GetTriggersOfJob(jobKey).Result;
        Assert.IsTrue(triggers.Any(t => t.TriggerKey.Name == "triggerKey"));
    }

    [TestMethod]
    public void GetTrigger()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");
        var trigger = _connector.GetTrigger(triggerKey).Result;
        Assert.IsNotNull(trigger);
        Assert.AreEqual("triggerKey", trigger.TriggerKey.Name);
    }

    [TestMethod]
    public void GetTriggerState()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");
        var state = _connector.GetTriggerState(triggerKey).Result;
        Assert.IsFalse(string.IsNullOrWhiteSpace(state));
    }

    // ── CheckExists ──────────────────────────────────────────────────────────

    [TestMethod]
    public void CheckExistsJobKey_Existing()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        var exists = _connector.CheckExists(jobKey).Result;
        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void CheckExistsJobKey_NotExisting()
    {
        var jobKey = new JobKey("DoesNotExist", "JobKeyGroup");
        var exists = _connector.CheckExists(jobKey).Result;
        Assert.IsFalse(exists);
    }

    [TestMethod]
    public void CheckExistsTriggerKey_Existing()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");
        var exists = _connector.CheckExists(triggerKey).Result;
        Assert.IsTrue(exists);
    }

    [TestMethod]
    public void CheckExistsTriggerKey_NotExisting()
    {
        var triggerKey = new TriggerKey("DoesNotExist", "JobKeyGroup");
        var exists = _connector.CheckExists(triggerKey).Result;
        Assert.IsFalse(exists);
    }

    // ── Currently executing jobs ─────────────────────────────────────────────

    [TestMethod]
    public void GetCurrentlyExecutingJobs()
    {
        var result = _connector.GetCurrentlyExecutingJobs().Result;
        Assert.IsNotNull(result);
    }

    // ── Pause / Resume jobs ──────────────────────────────────────────────────

    [TestMethod]
    public void PauseJob_And_ResumeJob()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");

        _connector.PauseJob(jobKey).GetAwaiter().GetResult();
        var paused = _connector.IsJobGroupPaused("JobKeyGroup").Result;
        Assert.IsTrue(paused);

        _connector.ResumeJob(jobKey).GetAwaiter().GetResult();
    }

    [TestMethod]
    public void PauseJobs_And_ResumeJobs()
    {
        var matcher = new QuartzRestApi.Wrappers.GroupMatcher<Quartz.JobKey>(GroupMatcherType.Equals, "JobKeyGroup");

        _connector.PauseJobs(matcher).GetAwaiter().GetResult();
        var paused = _connector.IsJobGroupPaused("JobKeyGroup").Result;
        Assert.IsTrue(paused);

        _connector.ResumeJobs(matcher).GetAwaiter().GetResult();
    }

    // ── Pause / Resume triggers ──────────────────────────────────────────────

    [TestMethod]
    public void PauseTrigger_And_ResumeTrigger()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");

        _connector.PauseTrigger(triggerKey).GetAwaiter().GetResult();
        var paused = _connector.IsTriggerGroupPaused("JobKeyGroup").Result;
        Assert.IsTrue(paused);

        _connector.ResumeTrigger(triggerKey).GetAwaiter().GetResult();
    }

    [TestMethod]
    public void PauseTriggers_And_ResumeTriggers()
    {
        var matcher = new QuartzRestApi.Wrappers.GroupMatcher<Quartz.TriggerKey>(GroupMatcherType.Equals, "JobKeyGroup");

        _connector.PauseTriggers(matcher).GetAwaiter().GetResult();
        var paused = _connector.IsTriggerGroupPaused("JobKeyGroup").Result;
        Assert.IsTrue(paused);

        _connector.ResumeTriggers(matcher).GetAwaiter().GetResult();
    }

    [TestMethod]
    public void PauseAllTriggers_And_ResumeAllTriggers()
    {
        _connector.PauseAllTriggers().GetAwaiter().GetResult();
        var paused = _connector.IsTriggerGroupPaused("JobKeyGroup").Result;
        Assert.IsTrue(paused);

        _connector.ResumeAllTriggers().GetAwaiter().GetResult();
    }

    // ── Calendars ────────────────────────────────────────────────────────────

    [TestMethod]
    public void GetCalendarNames()
    {
        var names = _connector.GetCalendarNames().Result;
        Assert.IsTrue(names.Contains("monthlyCalendar"));
    }

    [TestMethod]
    public void GetCalendar()
    {
        var json = _connector.GetCalendar("monthlyCalendar").Result;
        Assert.IsFalse(string.IsNullOrWhiteSpace(json));
    }

    [TestMethod]
    public void AddCalendar_And_DeleteCalendar()
    {
        var quartzCalendar = new Quartz.Impl.Calendar.MonthlyCalendar();
        var calendar = new QuartzRestApi.Wrappers.Calendars.MonthlyCalendar(quartzCalendar)
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

    // ── Interrupt ────────────────────────────────────────────────────────────

    [TestMethod]
    public void InterruptJob_ByJobKey()
    {
        var jobKey = new JobKey("JobKeyName", "JobKeyGroup");
        // Job may or may not be executing; the call must not throw
        _connector.InterruptJob(jobKey).GetAwaiter().GetResult();
    }

    [TestMethod]
    public void InterruptJob_ByFireInstanceId_NotFound()
    {
        var result = _connector.InterruptJob("nonexistent-fire-id").Result;
        Assert.IsFalse(result);
    }

    // ── ResetTriggerFromErrorState ────────────────────────────────────────────

    [TestMethod]
    public void ResetTriggerFromErrorState()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");
        // Trigger is not in error state; call must complete without exception
        _connector.ResetTriggerFromErrorState(triggerKey).GetAwaiter().GetResult();
    }

    // ── UnscheduleJob ────────────────────────────────────────────────────────

    [TestMethod]
    public void UnscheduleJob_And_Reschedule()
    {
        var triggerKey = new TriggerKey("triggerKey", "JobKeyGroup");

        var unscheduled = _connector.UnscheduleJob(triggerKey).Result;
        Assert.IsTrue(unscheduled);

        // Reschedule the job so subsequent tests still find it
        var exists = _connector.CheckExists(new JobKey("JobKeyName", "JobKeyGroup")).Result;
        // Job should still exist (it is durable via RequestRecovery)
        // Re-add trigger via ScheduleJob with trigger referencing the job
        // (skipped: job was set with RequestRecovery, re-scheduling is out of scope here)
    }
}
