using System;
using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace QuartzRestApi.Tests;

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