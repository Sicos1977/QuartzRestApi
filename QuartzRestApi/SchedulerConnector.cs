//
// SchedulerConnector.cs
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
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using QuartzRestApi.Security;
using QuartzRestApi.Wrappers;
using QuartzRestApi.Wrappers.Calendars;
// ReSharper disable UnusedMember.Global

namespace QuartzRestApi;

/// <summary>
///     Connects to the <see cref="SchedulerHost"/>
/// </summary>
public class SchedulerConnector
{
    #region Fields
    private readonly HttpClient _httpClient;
    #endregion

    #region Constructor
    /// <summary>
    ///     Create a new instance of the <see cref="SchedulerConnector"/>
    /// </summary>
    /// <param name="schedulerHostAddress">The host address of the scheduler</param>
    /// <param name="apiKey">
    ///     Optional API key. When supplied it is sent as the <c>X-Api-Key</c> header on
    ///     every request. Must match the key configured on the <see cref="SchedulerHost"/>.
    ///     Pass <see langword="null"/> or an empty string when the host has no key configured.
    /// </param>
    public SchedulerConnector(string schedulerHostAddress, string apiKey = null)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(schedulerHostAddress);

        if (!string.IsNullOrWhiteSpace(apiKey))
            _httpClient.DefaultRequestHeaders.Add(ApiKeyMiddleware.HeaderName, apiKey);
    }
    #endregion

    #region Helpers
    /// <summary>
    ///     Helper method to create a StringContent with the given JSON string and the correct encoding and media type
    /// </summary>
    /// <param name="json">The JSON string to include in the request body.</param>
    /// <returns>A <see cref="StringContent"/> instance with the specified JSON string.</returns>
    private static StringContent JsonBody(string json) => new(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json");

    /// <summary>
    ///     Reads the HTTP response content as a string and removes any leading and trailing double-quote characters.
    /// </summary>
    /// <remarks>
    ///     This method is useful when the response content is expected to be a JSON string value, which
    ///     may be enclosed in double quotes.
    /// </remarks>
    /// <param name="response">The HTTP response message containing the content to read. Cannot be null.</param>
    /// <returns>A string containing the response content with surrounding double quotes removed.</returns>
    private static async Task<string> ReadString(HttpResponseMessage response) => (await response.Content.ReadAsStringAsync().ConfigureAwait(false)).Trim('"');

    /// <summary>
    ///     Reads the HTTP response content as a string and parses it as a boolean value.
    /// </summary>
    /// <param name="response">The HTTP response message containing the content to read. Cannot be null.</param>
    /// <returns>A boolean value parsed from the response content.</returns>
    private static async Task<bool> ReadBool(HttpResponseMessage response) => bool.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    #endregion

    #region IsJobGroupPaused
    /// <summary>
    ///     Returns <c>true</c> if the given JobGroup is paused
    /// </summary>
    public async Task<bool> IsJobGroupPaused(string groupName)
    {
        var response = await _httpClient.GetAsync($"Scheduler/IsJobGroupPaused/{groupName}").ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region IsTriggerGroupPaused
    /// <summary>
    ///     Returns <c>true</c> if the given TriggerGroup is paused
    /// </summary>
    public async Task<bool> IsTriggerGroupPaused(string groupName)
    {
        var response = await _httpClient.GetAsync($"Scheduler/IsTriggerGroupPaused/{groupName}").ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region SchedulerName
    /// <summary>
    ///     Returns the name of the scheduler
    /// </summary>
    public async Task<string> SchedulerName()
    {
        var response = await _httpClient.GetAsync("Scheduler/SchedulerName").ConfigureAwait(false);
        return await ReadString(response);
    }
    #endregion

    #region SchedulerInstanceId
    /// <summary>
    ///     Returns the instance id of the scheduler
    /// </summary>
    public async Task<string> SchedulerInstanceId()
    {
        var response = await _httpClient.GetAsync("Scheduler/SchedulerInstanceId").ConfigureAwait(false);
        return await ReadString(response);
    }
    #endregion

    #region Context
    /// <summary>
    ///     Returns the <see cref="SchedulerContext" /> of the scheduler
    /// </summary>
    public async Task<SchedulerContext> Context()
    {
        var response = await _httpClient.GetAsync("Scheduler/SchedulerContext").ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return SchedulerContext.FromJsonString(json);
    }
    #endregion

    #region InStandbyMode
    /// <summary>
    ///     Returns <c>true</c> if the scheduler is in standby mode
    /// </summary>
    public async Task<bool> InStandbyMode()
    {
        var response = await _httpClient.GetAsync("Scheduler/InStandbyMode").ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region IsShutdown
    /// <summary>
    ///     Returns <c>true</c> if the scheduler is shutdown
    /// </summary>
    public async Task<bool> IsShutdown()
    {
        var response = await _httpClient.GetAsync("Scheduler/Isshutdown").ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region IsStarted
    /// <summary>
    ///     Returns <c>true</c> if the scheduler has been started
    /// </summary>
    public async Task<bool> IsStarted()
    {
        var response = await _httpClient.GetAsync("Scheduler/IsStarted").ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region GetMetaData
    /// <summary>
    ///     Returns a <see cref="SchedulerMetaData"/> object describing the settings and capabilities of the scheduler
    /// </summary>
    public async Task<SchedulerMetaData> GetMetaData()
    {
        var response = await _httpClient.GetAsync("Scheduler/GetMetaData").ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return SchedulerMetaData.FromJsonString(json);
    }
    #endregion

    #region GetCurrentlyExecutingJobs
    /// <summary>
    ///     Returns the list of currently executing jobs
    /// </summary>
    public async Task<JobExecutionContexts> GetCurrentlyExecutingJobs()
    {
        var response = await _httpClient.GetAsync("Scheduler/GetCurrentlyExecutingJobs").ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JobExecutionContexts.FromJsonString(json);
    }
    #endregion

    #region GetJobGroupNames
    /// <summary>
    ///     Returns the names of all known job groups
    /// </summary>
    public async Task<IReadOnlyCollection<string>> GetJobGroupNames()
    {
        var response = await _httpClient.GetAsync("Scheduler/GetJobGroupNames").ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<List<string>>(json);
    }
    #endregion

    #region GetTriggerGroupNames
    /// <summary>
    ///     Returns the names of all known trigger groups
    /// </summary>
    public async Task<IReadOnlyCollection<string>> GetTriggerGroupNames()
    {
        var response = await _httpClient.GetAsync("Scheduler/GetTriggerGroupNames").ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<List<string>>(json);
    }
    #endregion

    #region GetPausedTriggerGroups
    /// <summary>
    ///     Returns the names of all paused trigger groups
    /// </summary>
    public async Task<IReadOnlyCollection<string>> GetPausedTriggerGroups()
    {
        var response = await _httpClient.GetAsync("Scheduler/GetPausedTriggerGroups").ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<List<string>>(json);
    }
    #endregion

    #region Start
    /// <summary>
    ///     Starts the scheduler
    /// </summary>
    public async Task Start()
    {
        await _httpClient.PostAsync("Scheduler/Start", null).ConfigureAwait(false);
    }
    #endregion

    #region StartDelayed
    /// <summary>
    ///     Starts the scheduler after the given delay in seconds
    /// </summary>
    public async Task StartDelayed(int delaySeconds)
    {
        await _httpClient.PostAsync($"Scheduler/StartDelayed/{delaySeconds}", null).ConfigureAwait(false);
    }
    #endregion

    #region Standby
    /// <summary>
    ///     Puts the scheduler in standby mode
    /// </summary>
    public async Task Standby()
    {
        await _httpClient.PostAsync("Scheduler/Standby", null).ConfigureAwait(false);
    }
    #endregion

    #region Shutdown
    /// <summary>
    ///     Shuts down the scheduler
    /// </summary>
    public async Task Shutdown()
    {
        await _httpClient.PostAsync("Scheduler/Shutdown", null).ConfigureAwait(false);
    }

    /// <summary>
    ///     Shuts down the scheduler, optionally waiting for running jobs to complete
    /// </summary>
    public async Task Shutdown(bool waitForJobsToComplete)
    {
        await _httpClient.PostAsync($"Scheduler/Shutdown/{waitForJobsToComplete}", null).ConfigureAwait(false);
    }
    #endregion

    #region ScheduleJob
    /// <summary>
    ///     Schedules a job with a job detail and a single trigger
    /// </summary>
    public async Task<DateTimeOffset> ScheduleJob(JobDetailWithTrigger jobDetailWithTrigger)
    {
        var response = await _httpClient.PostAsync("Scheduler/ScheduleJobWithJobDetailAndTrigger", JsonBody(jobDetailWithTrigger.ToJsonString())).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<DateTimeOffset>(json);
    }

    /// <summary>
    ///     Schedules the trigger with the job identified by the trigger
    /// </summary>
    public async Task<DateTimeOffset> ScheduleJob(Trigger trigger)
    {
        var response = await _httpClient.PostAsync("Scheduler/ScheduleJobIdentifiedWithTrigger", JsonBody(trigger.ToJsonString())).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<DateTimeOffset>(json);
    }

    /// <summary>
    ///     Schedules a job with a job detail and multiple triggers
    /// </summary>
    public async Task ScheduleJobWithTriggers(JobDetailWithTriggers jobDetailWithTriggers)
    {
        await _httpClient.PostAsync("Scheduler/ScheduleJobWithJobDetailAndTriggers", JsonBody(jobDetailWithTriggers.ToJsonString())).ConfigureAwait(false);
    }

    /// <summary>
    ///     Schedules multiple jobs, each with their own set of triggers
    /// </summary>
    public async Task ScheduleJobs(ScheduleJobs scheduleJobs)
    {
        await _httpClient.PostAsync("Scheduler/ScheduleJobs", JsonBody(scheduleJobs.ToJsonString())).ConfigureAwait(false);
    }
    #endregion

    #region UnscheduleJob
    /// <summary>
    ///     Removes the trigger with the given key from the scheduler
    /// </summary>
    public async Task<bool> UnscheduleJob(TriggerKey triggerKey)
    {
        var response = await _httpClient.PostAsync("Scheduler/UnscheduleJob", JsonBody(triggerKey.ToJsonString())).ConfigureAwait(false);
        return await ReadBool(response);
    }

    /// <summary>
    ///     Removes all triggers with the given keys from the scheduler
    /// </summary>
    public async Task<bool> UnscheduleJobs(TriggerKeys triggerKeys)
    {
        var response = await _httpClient.PostAsync("Scheduler/UnscheduleJobs", JsonBody(triggerKeys.ToJsonString())).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region RescheduleJob
    /// <summary>
    ///     Replaces the current trigger with a new trigger for the same job
    /// </summary>
    public async Task<DateTimeOffset?> RescheduleJob(RescheduleJob rescheduleJob)
    {
        var response = await _httpClient.PostAsync("Scheduler/RescheduleJob", JsonBody(rescheduleJob.ToJsonString())).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (json == "null") return null;
        return JsonSerializer.Deserialize<DateTimeOffset>(json);
    }
    #endregion

    #region AddJob
    /// <summary>
    ///     Adds a job with no associated trigger
    /// </summary>
    public async Task AddJob(JobDetail jobDetail)
    {
        await _httpClient.PostAsync("Scheduler/AddJob", JsonBody(jobDetail.ToJsonString())).ConfigureAwait(false);
    }
    #endregion

    #region DeleteJob
    /// <summary>
    ///     Deletes the job with the given key and all associated triggers
    /// </summary>
    public async Task<bool> DeleteJob(JobKey jobKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "Scheduler/DeleteJob")
        {
            Content = JsonBody(jobKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        return await ReadBool(response);
    }

    /// <summary>
    ///     Deletes all jobs with the given keys and their associated triggers
    /// </summary>
    public async Task<bool> DeleteJobs(JobKeys jobKeys)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "Scheduler/DeleteJobs")
        {
            Content = JsonBody(jobKeys.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region TriggerJob
    /// <summary>
    ///     Triggers the identified job to execute immediately
    /// </summary>
    public async Task TriggerJob(JobKey jobKey)
    {
        await _httpClient.PostAsync("Scheduler/TriggerJobWithJobkey", JsonBody(jobKey.ToJsonString())).ConfigureAwait(false);
    }

    /// <summary>
    ///     Triggers the identified job to execute immediately with a data map
    /// </summary>
    public async Task TriggerJob(JobKeyWithDataMap jobKeyWithDataMap)
    {
        await _httpClient.PostAsync("Scheduler/TriggerJobWithDataMap", JsonBody(jobKeyWithDataMap.ToJsonString())).ConfigureAwait(false);
    }
    #endregion

    #region PauseJob
    /// <summary>
    ///     Pauses the job with the given key
    /// </summary>
    public async Task PauseJob(JobKey jobKey)
    {
        await _httpClient.PostAsync("Scheduler/PauseJob", JsonBody(jobKey.ToJsonString())).ConfigureAwait(false);
    }

    /// <summary>
    ///     Pauses all jobs matching the given group matcher
    /// </summary>
    public async Task PauseJobs(GroupMatcher<Quartz.JobKey> groupMatcher)
    {
        await _httpClient.PostAsync("Scheduler/PauseJobs", JsonBody(groupMatcher.ToJsonString())).ConfigureAwait(false);
    }
    #endregion

    #region PauseTrigger
    /// <summary>
    ///     Pauses the trigger with the given key
    /// </summary>
    public async Task PauseTrigger(TriggerKey triggerKey)
    {
        await _httpClient.PostAsync("Scheduler/PauseTrigger", JsonBody(triggerKey.ToJsonString())).ConfigureAwait(false);
    }

    /// <summary>
    ///     Pauses all triggers matching the given group matcher
    /// </summary>
    public async Task PauseTriggers(GroupMatcher<Quartz.TriggerKey> groupMatcher)
    {
        await _httpClient.PostAsync("Scheduler/PauseTriggers", JsonBody(groupMatcher.ToJsonString())).ConfigureAwait(false);
    }

    /// <summary>
    ///     Pauses all triggers
    /// </summary>
    public async Task PauseAllTriggers()
    {
        await _httpClient.PostAsync("Scheduler/PauseAllTriggers", null).ConfigureAwait(false);
    }
    #endregion

    #region ResumeJob
    /// <summary>
    ///     Resumes the job with the given key
    /// </summary>
    public async Task ResumeJob(JobKey jobKey)
    {
        await _httpClient.PostAsync("Scheduler/ResumeJob",
            JsonBody(jobKey.ToJsonString())).ConfigureAwait(false);
    }

    /// <summary>
    ///     Resumes all jobs matching the given group matcher
    /// </summary>
    public async Task ResumeJobs(GroupMatcher<Quartz.JobKey> groupMatcher)
    {
        await _httpClient.PostAsync("Scheduler/ResumeJobs",
            JsonBody(groupMatcher.ToJsonString())).ConfigureAwait(false);
    }
    #endregion

    #region ResumeTrigger
    /// <summary>
    ///     Resumes the trigger with the given key
    /// </summary>
    public async Task ResumeTrigger(TriggerKey triggerKey)
    {
        await _httpClient.PostAsync("Scheduler/ResumeTrigger",
            JsonBody(triggerKey.ToJsonString())).ConfigureAwait(false);
    }

    /// <summary>
    ///     Resumes all triggers matching the given group matcher
    /// </summary>
    public async Task ResumeTriggers(GroupMatcher<Quartz.TriggerKey> groupMatcher)
    {
        await _httpClient.PostAsync("Scheduler/ResumeTriggers",
            JsonBody(groupMatcher.ToJsonString())).ConfigureAwait(false);
    }

    /// <summary>
    ///     Resumes all triggers
    /// </summary>
    public async Task ResumeAllTriggers()
    {
        await _httpClient.PostAsync("Scheduler/ResumeAllTriggers", null).ConfigureAwait(false);
    }
    #endregion

    #region GetJobKeys
    /// <summary>
    ///     Returns all job keys matching the given group matcher
    /// </summary>
    public async Task<JobKeys> GetJobKeys(GroupMatcher<Quartz.JobKey> groupMatcher)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/GetJobKeys")
        {
            Content = JsonBody(groupMatcher.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(json) ? JobKeys.FromJsonString("[]") : JobKeys.FromJsonString(json);
    }
    #endregion

    #region GetTriggerKeys
    /// <summary>
    ///     Returns all trigger keys matching the given group matcher
    /// </summary>
    public async Task<TriggerKeys> GetTriggerKeys(GroupMatcher<Quartz.TriggerKey> groupMatcher)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/GetTriggerKeys")
        {
            Content = JsonBody(groupMatcher.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(json) ? TriggerKeys.FromJsonString("[]") : TriggerKeys.FromJsonString(json);
    }
    #endregion

    #region GetTriggersOfJob
    /// <summary>
    ///     Returns all triggers associated with the given job key
    /// </summary>
    public async Task<Triggers> GetTriggersOfJob(JobKey jobKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/GetTriggersOfJob")
        {
            Content = JsonBody(jobKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(json) ? Triggers.FromJsonString("[]") : Triggers.FromJsonString(json);
    }
    #endregion

    #region GetJobDetail
    /// <summary>
    ///     Returns the job detail for the given job key
    /// </summary>
    public async Task<JobDetail> GetJobDetail(JobKey jobKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/GetJobDetail")
        {
            Content = JsonBody(jobKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(json) ? null : JobDetail.FromJsonString(json);
    }
    #endregion

    #region GetTrigger
    /// <summary>
    ///     Returns the trigger for the given trigger key
    /// </summary>
    public async Task<Trigger> GetTrigger(TriggerKey triggerKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/GetTrigger")
        {
            Content = JsonBody(triggerKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(json) ? null : Trigger.FromJsonString(json);
    }
    #endregion

    #region GetTriggerState
    /// <summary>
    ///     Returns the current state of the given trigger
    /// </summary>
    public async Task<string> GetTriggerState(TriggerKey triggerKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/GetTriggerState")
        {
            Content = JsonBody(triggerKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        return await ReadString(response);
    }
    #endregion

    #region AddCalendar
    /// <summary>
    ///     Adds a calendar to the scheduler
    /// </summary>
    public async Task AddCalendar(BaseCalendar calendar)
    {
        await _httpClient.PostAsync("Scheduler/AddCalendar", JsonBody(calendar.ToJsonString())).ConfigureAwait(false);
    }
    #endregion

    #region DeleteCalendar
    /// <summary>
    ///     Deletes the calendar with the given name. Returns <c>true</c> when deleted.
    /// </summary>
    public async Task<bool> DeleteCalendar(string calendarName)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"Scheduler/DeleteCalendar/{calendarName}");
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region GetCalendar
    /// <summary>
    ///     Returns the calendar with the given name as a JSON string
    /// </summary>
    public async Task<string> GetCalendar(string calendarName)
    {
        var response = await _httpClient.GetAsync($"Scheduler/Getcalendar/{calendarName}").ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
    #endregion

    #region GetCalendarNames
    /// <summary>
    ///     Returns the names of all registered calendars
    /// </summary>
    public async Task<IReadOnlyCollection<string>> GetCalendarNames()
    {
        var response = await _httpClient.GetAsync("Scheduler/GetCalendarNames").ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<List<string>>(json);
    }
    #endregion

    #region Interrupt
    /// <summary>
    ///     Requests cancellation of all executing instances of the given job
    /// </summary>
    public async Task<bool> InterruptJob(JobKey jobKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/InterruptJobKey")
        {
            Content = JsonBody(jobKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        return await ReadBool(response);
    }

    /// <summary>
    ///     Requests cancellation of the executing job instance with the given fire instance id
    /// </summary>
    public async Task<bool> InterruptJob(string fireInstanceId)
    {
        var response = await _httpClient.GetAsync($"Scheduler/interruptfireinstanceid/{fireInstanceId}").ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region CheckExists
    /// <summary>
    ///     Returns <c>true</c> if a job with the given key exists
    /// </summary>
    public async Task<bool> CheckExists(JobKey jobKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/CheckExistsJobkey")
        {
            Content = JsonBody(jobKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        return await ReadBool(response);
    }

    /// <summary>
    ///     Returns <c>true</c> if a trigger with the given key exists
    /// </summary>
    public async Task<bool> CheckExists(TriggerKey triggerKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/CheckExistsTriggerkey")
        {
            Content = JsonBody(triggerKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region Clear
    /// <summary>
    ///     Clears all scheduling data — all jobs, triggers and calendars
    /// </summary>
    public async Task Clear()
    {
        await _httpClient.PostAsync("Scheduler/Clear", null).ConfigureAwait(false);
    }
    #endregion

    #region ResetTriggerFromErrorState
    /// <summary>
    ///     Resets the given trigger from error state back to normal or paused
    /// </summary>
    public async Task ResetTriggerFromErrorState(TriggerKey triggerKey)
    {
        await _httpClient.PostAsync("Scheduler/ResetTriggerFromErrorState",
            JsonBody(triggerKey.ToJsonString())).ConfigureAwait(false);
    }
    #endregion
}