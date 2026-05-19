using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;
using QuartzRestApi;

namespace ConsoleAppNET;

internal class Program
{
    static void Main(string[] args)
    {
        var properties = new NameValueCollection
        {
            ["quartz.scheduler.exporter.bindName"] = "Scheduler",
            ["quartz.scheduler.exporter.channelName"] = "tcpQuartz",
            ["quartz.scheduler.exporter.channelType"] = "tcp",
            ["quartz.scheduler.exporter.port"] = "45000",
            ["quartz.scheduler.exporter.type"] = "Quartz.Simpl.RemotingSchedulerExporter, Quartz",
            ["quartz.scheduler.instanceName"] = Dns.GetHostEntry(Environment.MachineName).HostName.Replace(".", "_"),
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

        var host = new SchedulerHost("http://localhost:44344", scheduler, null);
        host.Start();

        var connector = new SchedulerConnector("http://localhost:44344");
        var paused = connector.IsJobGroupPaused("JobKeyGroup").Result;


        Console.ReadKey();
    }
}

public class TestJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        while (true)
        {
            context.JobDetail.JobDataMap.PutAsString("Timer", DateTime.Now);
            Thread.Sleep(10000);
        }
    }
}