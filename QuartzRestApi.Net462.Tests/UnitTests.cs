// Copyright (c) 2022 - 2026 Magic-Sessions. (www.magic-sessions.com)
// Licensed under the MIT licence.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quartz;
using Quartz.Impl;

namespace QuartzRestApi.Net462.Tests
{
    [TestClass]
    public class UnitTests
    {
        private const string HostAddress = "http://localhost:45001";

        private static IScheduler _scheduler;
        private static SchedulerHost _host;
        private static SchedulerConnector _connector;

        [ClassInitialize]
        public static async Task InitializeClass(TestContext context)
        {
            var factory = new StdSchedulerFactory();
            _scheduler = await factory.GetScheduler();
            await _scheduler.Start();

            _host = new SchedulerHost(HostAddress, _scheduler);
            await _host.Start();

            _connector = new SchedulerConnector(HostAddress);
        }

        [ClassCleanup]
        public static async Task CleanupClass()
        {
            await _host.Stop();
            await _scheduler.Shutdown();
        }

        [TestMethod]
        public async Task GetSchedulerName()
        {
            var name = await _connector.SchedulerName();
            Assert.IsFalse(string.IsNullOrEmpty(name));
        }

        [TestMethod]
        public async Task GetMetaData()
        {
            var metaData = await _connector.GetMetaData();
            Assert.IsNotNull(metaData);
        }

        [TestMethod]
        public async Task InStandbyMode()
        {
            var result = await _connector.InStandbyMode();
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task IsStarted()
        {
            var result = await _connector.IsStarted();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task IsShutdown()
        {
            var result = await _connector.IsShutdown();
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetCurrentlyExecutingJobs()
        {
            var jobs = await _connector.GetCurrentlyExecutingJobs();
            Assert.IsNotNull(jobs);
        }

        [TestMethod]
        public async Task GetJobGroupNames()
        {
            var names = await _connector.GetJobGroupNames();
            Assert.IsNotNull(names);
        }

        [TestMethod]
        public async Task GetTriggerGroupNames()
        {
            var names = await _connector.GetTriggerGroupNames();
            Assert.IsNotNull(names);
        }

        [TestMethod]
        public async Task GetCalendarNames()
        {
            var names = await _connector.GetCalendarNames();
            Assert.IsNotNull(names);
        }
    }
}
