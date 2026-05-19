using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;

namespace QuartzRestApi.Tests;

[TestClass]
public class UnitTest1
{
    #region Fields
    private SchedulerHost _host;
    private SchedulerConnector _connector;
    #endregion

    #region Initialize
    /// <summary>
    ///     Setup testing
    /// </summary>
    [TestInitialize]
    public void Initialize()
    {
        var properties = new NameValueCollection
        {
            ["quartz.scheduler.exporter.bindName"] = "Scheduler",
            ["quartz.scheduler.exporter.channelName"] = "tcpQuartz",
            ["quartz.scheduler.exporter.channelType"] = "tcp",
            ["quartz.scheduler.exporter.port"] = "45000",
            ["quartz.scheduler.exporter.type"] = "Quartz.Simpl.RemotingSchedulerExporter, Quartz",
            ["quartz.scheduler.instanceName"] = "SchedulerInstance",
            ["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz",
            ["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz"
        };

        var scheduler = new StdSchedulerFactory(properties).GetScheduler().Result;

        scheduler.Context.Add("key1", "value1");
        scheduler.Context.Add("key2", "value2");
        scheduler.Context.Add("key3", "value3");

        scheduler.Start();

        var job = new TestJob();
        var schedulerJob = JobBuilder.Create(job.GetType())
            .WithIdentity(new JobKey("JobKeyName", "JobKeyGroup"))
            .WithDescription("Test")
            .RequestRecovery()
            .Build();

        var schedulerTrigger = (ISimpleTrigger)TriggerBuilder.Create()
            .WithIdentity("triggerKey")
            .WithDescription("TestTrigger")
            .StartAt(DateTime.Now)
            .Build();

        var mc = new MonthlyCalendar();
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
}