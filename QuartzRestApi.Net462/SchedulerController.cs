//
// SchedulerController.cs
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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Quartz;
using QuartzRestApi.Models.Calendars;
using QuartzRestApi.Models.Groups;
using QuartzRestApi.Models.Jobs;
using QuartzRestApi.Models.Triggers;
using JobKey = QuartzRestApi.Models.Jobs.JobKey;
using TriggerKey = QuartzRestApi.Models.Triggers.TriggerKey;
using SchedulerMetaData = QuartzRestApi.Models.Scheduler.SchedulerMetaData;
using ScheduleJobsRequest = QuartzRestApi.Models.Jobs.ScheduleJobs;
using SchedulerContext = QuartzRestApi.Models.Scheduler.SchedulerContext;

namespace QuartzRestApi;
/// <summary>
///     Web API 2 controller that exposes the Quartz.NET scheduler over HTTP.
/// </summary>
[RoutePrefix("")]
public class SchedulerController : ApiController
{
    #region Fields
    /// <summary>
    ///    The Quartz.NET scheduler instance that this controller exposes over HTTP.
    /// </summary>
    private readonly IScheduler _scheduler;
    #endregion

    #region Constructor
    /// <summary>Creates a new instance of <see cref="SchedulerController"/>.</summary>
    public SchedulerController(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }
    #endregion

    #region RawJson
    /// <summary>Creates an <see cref="HttpResponseMessage"/> with raw JSON content.</summary>
    private HttpResponseMessage RawJson(string json)
    {
        var response = Request.CreateResponse(HttpStatusCode.OK);
        response.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return response;
    }
    #endregion

    #region IsJobGroupPaused
    [HttpGet, Route("Scheduler/IsJobGroupPaused/{groupName}")]
    public Task<bool> IsJobGroupPaused(string groupName) =>
        _scheduler.IsJobGroupPaused(groupName);
    #endregion

    #region IsTriggerGroupPaused
    [HttpGet, Route("Scheduler/IsTriggerGroupPaused/{groupName}")]
    public Task<bool> IsTriggerGroupPaused(string groupName) =>
        _scheduler.IsTriggerGroupPaused(groupName);
    #endregion

    #region SchedulerName
    [HttpGet, Route("Scheduler/SchedulerName")]
    public IHttpActionResult SchedulerName() => Ok(_scheduler.SchedulerName);
    #endregion

    #region SchedulerInstanceId
    [HttpGet, Route("Scheduler/SchedulerInstanceId")]
    public IHttpActionResult SchedulerInstanceId() => Ok(_scheduler.SchedulerInstanceId);
    #endregion

    #region Context
    [HttpGet, Route("Scheduler/SchedulerContext")]
    public HttpResponseMessage Context() =>
        RawJson(new SchedulerContext(_scheduler.Context).ToJsonString());
    #endregion

    #region InStandbyMode
    [HttpGet, Route("Scheduler/InStandbyMode")]
    public IHttpActionResult InStandbyMode() => Ok(_scheduler.InStandbyMode);
    #endregion

    #region IsShutdown
    [HttpGet, Route("Scheduler/IsShutdown")]
    public IHttpActionResult IsShutdown() => Ok(_scheduler.IsShutdown);
    #endregion

    #region GetMetaData
    [HttpGet, Route("Scheduler/GetMetaData")]
    public async Task<HttpResponseMessage> GetMetaData()
    {
        var meta = await _scheduler.GetMetaData();
        return RawJson(new SchedulerMetaData(meta).ToJsonString());
    }
    #endregion

    #region GetCurrentlyExecutingJobs
    [HttpGet, Route("Scheduler/GetCurrentlyExecutingJobs")]
    public async Task<HttpResponseMessage> GetCurrentlyExecutingJobs()
    {
        var jobs = await _scheduler.GetCurrentlyExecutingJobs();
        return RawJson(new JobExecutionContexts(jobs).ToJsonString());
    }
    #endregion

    #region GetJobGroupNames
    [HttpGet, Route("Scheduler/GetJobGroupNames")]
    public Task<IReadOnlyCollection<string>> GetJobGroupNames() =>
        _scheduler.GetJobGroupNames();
    #endregion

    #region GetTriggerGroupNames
    [HttpGet, Route("Scheduler/GetTriggerGroupNames")]
    public Task<IReadOnlyCollection<string>> GetTriggerGroupNames() =>
        _scheduler.GetTriggerGroupNames();
    #endregion

    #region GetPausedTriggerGroups
    [HttpGet, Route("Scheduler/GetPausedTriggerGroups")]
    public Task<IReadOnlyCollection<string>> GetPausedTriggerGroups() =>
        _scheduler.GetPausedTriggerGroups();
    #endregion

    #region Start
    [HttpPost, Route("Scheduler/Start")]
    public Task Start() => _scheduler.Start();
    #endregion

    #region StartDelayed
    [HttpPost, Route("Scheduler/StartDelayed/{delay}")]
    public Task StartDelayed(int delay) =>
        _scheduler.StartDelayed(new TimeSpan(0, 0, delay));
    #endregion

    #region IsStarted
    [HttpGet, Route("Scheduler/IsStarted")]
    public IHttpActionResult IsStarted() => Ok(_scheduler.IsStarted);
    #endregion

    #region Standby
    [HttpPost, Route("Scheduler/Standby")]
    public Task Standby() => _scheduler.Standby();
    #endregion

    #region Shutdown
    [HttpPost, Route("Scheduler/Shutdown")]
    public Task Shutdown() => _scheduler.Shutdown();

    [HttpPost, Route("Scheduler/Shutdown/{waitForJobsToComplete}")]
    public Task Shutdown(bool waitForJobsToComplete) =>
        _scheduler.Shutdown(waitForJobsToComplete);
    #endregion

    #region ScheduleJob
    [HttpPost, Route("Scheduler/ScheduleJobWithJobDetailAndTrigger")]
    public Task<DateTimeOffset> ScheduleJob([FromBody] string json)
    {
        var jobDetailWithTrigger = JobDetailWithTrigger.FromJsonString(json);
        return _scheduler.ScheduleJob(
            jobDetailWithTrigger.JobDetail.ToJobDetail(),
            jobDetailWithTrigger.Trigger.ToTrigger());
    }
    #endregion

    #region ScheduleJobIdentifiedWithTrigger
    [HttpPost, Route("Scheduler/ScheduleJobIdentifiedWithTrigger")]
    public Task<DateTimeOffset> ScheduleJobIdentifiedWithTrigger([FromBody] string json)
    {
        var trigger = TriggerBase.FromJsonString(json).ToTrigger();
        return _scheduler.ScheduleJob(trigger);
    }
    #endregion

    #region ScheduleJobs
    [HttpPost, Route("Scheduler/ScheduleJobs")]
    public Task ScheduleJobs([FromBody] string json)
    {
        var scheduleJobs = ScheduleJobsRequest.FromJsonString(json);
        return _scheduler.ScheduleJobs(scheduleJobs.ToSchedulerDictionary(), scheduleJobs.Replace);
    }
    #endregion

    #region ScheduleJobWithTriggers
    [HttpPost, Route("Scheduler/ScheduleJobWithJobDetailAndTriggers")]
    public Task ScheduleJobWithTriggers([FromBody] string json)
    {
        var jobDetailWithTriggers = JobDetailWithTriggers.FromJsonString(json);
        return _scheduler.ScheduleJob(
            jobDetailWithTriggers.JobDetail.ToJobDetail(),
            jobDetailWithTriggers.ToReadOnlyTriggerCollection(),
            jobDetailWithTriggers.Replace);
    }
    #endregion

    #region UnscheduleJob
    [HttpPost, Route("Scheduler/UnscheduleJob")]
    public Task<bool> UnscheduleJob([FromBody] string json) =>
        _scheduler.UnscheduleJob(TriggerKey.FromJsonString(json).ToTriggerKey());
    #endregion

    #region UnscheduleJobs
    [HttpPost, Route("Scheduler/UnscheduleJobs")]
    public Task<bool> UnscheduleJobs([FromBody] string json) =>
        _scheduler.UnscheduleJobs(TriggerKeys.FromJsonString(json).ToTriggerKeys());
    #endregion

    #region RescheduleJob
    [HttpPost, Route("Scheduler/RescheduleJob")]
    public Task<DateTimeOffset?> RescheduleJob([FromBody] string json)
    {
        var rescheduleJob = Models.Jobs.RescheduleJob.FromJsonString(json);
        return _scheduler.RescheduleJob(
            rescheduleJob.CurrentTriggerKey.ToTriggerKey(),
            rescheduleJob.Trigger.ToTrigger());
    }
    #endregion

    #region AddJob
    [HttpPost, Route("Scheduler/AddJob")]
    public Task AddJob([FromBody] string json)
    {
        var addJob = JobDetail.FromJsonString(json);
        return _scheduler.AddJob(addJob.ToJobDetail(), addJob.Replace,
            addJob.StoreNonDurableWhileAwaitingScheduling);
    }
    #endregion

    #region DeleteJob
    [HttpDelete, Route("Scheduler/DeleteJob")]
    public Task<bool> DeleteJob([FromBody] string json) =>
        _scheduler.DeleteJob(JobKey.FromJsonString(json).ToJobKey());
    #endregion

    #region DeleteJobs
    [HttpDelete, Route("Scheduler/DeleteJobs")]
    public Task<bool> DeleteJobs([FromBody] string json) =>
        _scheduler.DeleteJobs(JobKeys.FromJsonString(json).ToJobKeys());
    #endregion

    #region TriggerJobWithJobKey
    [HttpPost, Route("Scheduler/TriggerJobWithJobkey")]
    public Task TriggerJobWithJobKey([FromBody] string json) =>
        _scheduler.TriggerJob(JobKey.FromJsonString(json).ToJobKey());
    #endregion

    #region TriggerJobWithDataMap
    [HttpPost, Route("Scheduler/TriggerJobWithDataMap")]
    public Task TriggerJobWithDataMap([FromBody] string json)
    {
        var jobKeyWithDataMap = JobKeyWithDataMap.FromJsonString(json);
        return _scheduler.TriggerJob(jobKeyWithDataMap.JobKey.ToJobKey(), jobKeyWithDataMap.JobDataMap);
    }
    #endregion

    #region PauseJob
    [HttpPost, Route("Scheduler/PauseJob")]
    public Task PauseJob([FromBody] string json) =>
        _scheduler.PauseJob(JobKey.FromJsonString(json).ToJobKey());
    #endregion

    #region PauseJobs
    [HttpPost, Route("Scheduler/PauseJobs")]
    public Task PauseJobs([FromBody] string json)
    {
        var groupMatcher = GroupMatcher<Quartz.JobKey>.FromJsonString(json);
        return _scheduler.PauseJobs(groupMatcher.ToGroupMatcher());
    }
    #endregion

    #region PauseTrigger
    [HttpPost, Route("Scheduler/PauseTrigger")]
    public Task PauseTrigger([FromBody] string json) =>
        _scheduler.PauseTrigger(TriggerKey.FromJsonString(json).ToTriggerKey());
    #endregion

    #region PauseTriggers
    [HttpPost, Route("Scheduler/PauseTriggers")]
    public Task PauseTriggers([FromBody] string json)
    {
        var groupMatcher = GroupMatcher<Quartz.TriggerKey>.FromJsonString(json);
        return _scheduler.PauseTriggers(groupMatcher.ToGroupMatcher());
    }
    #endregion

    #region ResumeJob
    [HttpPost, Route("Scheduler/ResumeJob")]
    public Task ResumeJob([FromBody] string json) =>
        _scheduler.ResumeJob(JobKey.FromJsonString(json).ToJobKey());
    #endregion

    #region ResumeJobs
    [HttpPost, Route("Scheduler/ResumeJobs")]
    public Task ResumeJobs([FromBody] string json)
    {
        var groupMatcher = GroupMatcher<Quartz.JobKey>.FromJsonString(json);
        return _scheduler.ResumeJobs(groupMatcher.ToGroupMatcher());
    }
    #endregion

    #region ResumeTrigger
    [HttpPost, Route("Scheduler/ResumeTrigger")]
    public Task ResumeTrigger([FromBody] string json) =>
        _scheduler.ResumeTrigger(TriggerKey.FromJsonString(json).ToTriggerKey());
    #endregion

    #region ResumeTriggers
    [HttpPost, Route("Scheduler/ResumeTriggers")]
    public Task ResumeTriggers([FromBody] string json)
    {
        var groupMatcher = GroupMatcher<Quartz.TriggerKey>.FromJsonString(json);
        return _scheduler.ResumeTriggers(groupMatcher.ToGroupMatcher());
    }
    #endregion

    #region PauseAll
    [HttpPost, Route("Scheduler/PauseAllTriggers")]
    public Task PauseAll() => _scheduler.PauseAll();
    #endregion

    #region ResumeAll
    [HttpPost, Route("Scheduler/ResumeAllTriggers")]
    public Task ResumeAll() => _scheduler.ResumeAll();
    #endregion

    #region GetJobKeys
    [HttpPost, Route("Scheduler/GetJobKeys")]
    public async Task<HttpResponseMessage> GetJobKeys([FromBody] string json)
    {
        var groupMatcher = GroupMatcher<Quartz.JobKey>.FromJsonString(json);
        var jobKeys = await _scheduler.GetJobKeys(groupMatcher.ToGroupMatcher());
        return RawJson(new JobKeys(jobKeys).ToJsonString());
    }
    #endregion

    #region GetTriggersOfJob
    [HttpPost, Route("Scheduler/GetTriggersOfJob")]
    public async Task<HttpResponseMessage> GetTriggersOfJob([FromBody] string json)
    {
        var jobKey = JobKey.FromJsonString(json);
        var triggers = await _scheduler.GetTriggersOfJob(jobKey.ToJobKey());
        return RawJson(new Triggers(triggers).ToJsonString());
    }
    #endregion

    #region GetTriggerKeys
    [HttpPost, Route("Scheduler/GetTriggerKeys")]
    public async Task<HttpResponseMessage> GetTriggerKeys([FromBody] string json)
    {
        var groupMatcher = GroupMatcher<Quartz.TriggerKey>.FromJsonString(json);
        var triggerKeys = await _scheduler.GetTriggerKeys(groupMatcher.ToGroupMatcher());
        return RawJson(new TriggerKeys(triggerKeys).ToJsonString());
    }
    #endregion

    #region GetJobDetail
    [HttpPost, Route("Scheduler/GetJobDetail")]
    public async Task<HttpResponseMessage> GetJobDetail([FromBody] string json)
    {
        var jobKey = JobKey.FromJsonString(json);
        var jobDetail = await _scheduler.GetJobDetail(jobKey.ToJobKey());
        return RawJson(jobDetail == null ? "null" : new JobDetail(jobDetail).ToJsonString());
    }
    #endregion

    #region GetTrigger
    [HttpPost, Route("Scheduler/GetTrigger")]
    public async Task<HttpResponseMessage> GetTrigger([FromBody] string json)
    {
        var triggerKey = TriggerKey.FromJsonString(json);
        var quartzTrigger = await _scheduler.GetTrigger(triggerKey.ToTriggerKey());

        TriggerBase trigger = quartzTrigger switch
        {
            ISimpleTrigger simpleTrigger => new SimpleTrigger(simpleTrigger),
            ICalendarIntervalTrigger calendarIntervalTrigger => new CalendarIntervalTrigger(calendarIntervalTrigger),
            ICronTrigger cronTrigger => new CronTrigger(cronTrigger),
            IDailyTimeIntervalTrigger dailyTimeIntervalTrigger => new DailyTimeIntervalTrigger(dailyTimeIntervalTrigger),
            IRecurrenceTrigger recurrenceTrigger => new RecurrenceTrigger(recurrenceTrigger),
            _ => throw new ArgumentOutOfRangeException(nameof(quartzTrigger), quartzTrigger, "Unsupported trigger type")
        };

        return RawJson(trigger.ToJsonString());
    }
    #endregion

    #region GetTriggerState
    [HttpPost, Route("Scheduler/GetTriggerState")]
    public async Task<IHttpActionResult> GetTriggerState([FromBody] string json)
    {
        var triggerKey = TriggerKey.FromJsonString(json);
        var triggerState = await _scheduler.GetTriggerState(triggerKey.ToTriggerKey());
        return Ok(triggerState.ToString());
    }
    #endregion

    #region AddCalendar
    [HttpPost, Route("Scheduler/AddCalendar")]
    public async Task AddCalendar([FromBody] string json)
    {
        var baseCalendar = BaseCalendar.FromJsonString(json);
        var calendar = baseCalendar.ToCalendar();
        if (!string.IsNullOrEmpty(baseCalendar.CalendarBase))
            calendar.CalendarBase = await _scheduler.GetCalendar(baseCalendar.CalendarBase);
        await _scheduler.AddCalendar(baseCalendar.Name, calendar, baseCalendar.Replace, baseCalendar.UpdateTriggers);
    }
    #endregion

    #region DeleteCalendar
    [HttpDelete, Route("Scheduler/DeleteCalendar/{calName}")]
    public Task<bool> DeleteCalendar(string calName) =>
        _scheduler.DeleteCalendar(calName);
    #endregion

    #region GetCalendar
    [HttpGet, Route("Scheduler/GetCalendar/{calName}")]
    public async Task<HttpResponseMessage> GetCalendar(string calName)
    {
        var calendar = await _scheduler.GetCalendar(calName);
        if (calendar == null) return RawJson("null");

        string result;
        switch (calendar)
        {
            case Quartz.Impl.Calendar.CronCalendar cronCalendar:
                result = new CronCalendar(cronCalendar).ToJsonString(); break;
            case Quartz.Impl.Calendar.DailyCalendar dailyCalendar:
                result = new DailyCalendar(dailyCalendar).ToJsonString(); break;
            case Quartz.Impl.Calendar.WeeklyCalendar weeklyCalendar:
                result = new WeeklyCalendar(weeklyCalendar).ToJsonString(); break;
            case Quartz.Impl.Calendar.MonthlyCalendar monthlyCalendar:
                result = new MonthlyCalendar(monthlyCalendar).ToJsonString(); break;
            case Quartz.Impl.Calendar.AnnualCalendar annualCalendar:
                result = new AnnualCalendar(annualCalendar).ToJsonString(); break;
            case Quartz.Impl.Calendar.HolidayCalendar holidayCalendar:
                result = new HolidayCalendar(holidayCalendar).ToJsonString(); break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return RawJson(result);
    }
    #endregion

    #region GetCalendarNames
    [HttpGet, Route("Scheduler/GetCalendarNames")]
    public Task<IReadOnlyCollection<string>> GetCalendarNames() =>
        _scheduler.GetCalendarNames();
    #endregion

    #region InterruptJobKey
    [HttpPost, Route("Scheduler/InterruptJobKey")]
    public async Task<IHttpActionResult> InterruptJobKey([FromBody] string json)
    {
        var jobKey = JobKey.FromJsonString(json);
        var result = await _scheduler.Interrupt(jobKey.ToJobKey());
        return Ok(result);
    }
    #endregion

    #region InterruptFireInstanceId
    [HttpGet, Route("Scheduler/InterruptFireInstanceId/{fireInstanceId}")]
    public async Task<IHttpActionResult> InterruptFireInstanceId(string fireInstanceId)
    {
        var result = await _scheduler.Interrupt(fireInstanceId);
        return Ok(result);
    }
    #endregion

    #region CheckExistsJobKey
    [HttpPost, Route("Scheduler/CheckExistsJobKey")]
    public async Task<IHttpActionResult> CheckExistsJobKey([FromBody] string json)
    {
        var jobKey = JobKey.FromJsonString(json);
        var result = await _scheduler.CheckExists(jobKey.ToJobKey());
        return Ok(result);
    }
    #endregion

    #region CheckExistsTriggerKey
    [HttpPost, Route("Scheduler/CheckExistsTriggerKey")]
    public async Task<IHttpActionResult> CheckExistsTriggerKey([FromBody] string json)
    {
        var triggerKey = TriggerKey.FromJsonString(json);
        var result = await _scheduler.CheckExists(triggerKey.ToTriggerKey());
        return Ok(result);
    }
    #endregion

    #region Clear
    [HttpPost, Route("Scheduler/Clear")]
    public Task Clear() => _scheduler.Clear();
    #endregion

    #region ResetTriggerFromErrorState
    [HttpPost, Route("Scheduler/ResetTriggerFromErrorState")]
    public Task ResetTriggerFromErrorState([FromBody] string json) =>
        _scheduler.ResetTriggerFromErrorState(TriggerKey.FromJsonString(json).ToTriggerKey());
    #endregion
}

