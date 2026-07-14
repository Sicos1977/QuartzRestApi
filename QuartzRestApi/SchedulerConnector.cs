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
using System.Threading;
using System.Threading.Tasks;
using QuartzRestApi.Exceptions;
using QuartzRestApi.Security;
using QuartzRestApi.Models.Calendars;
using QuartzRestApi.Models.Triggers;
using QuartzRestApi.Models.Jobs;
using QuartzRestApi.Models.Groups;
using QuartzRestApi.Models.Scheduler;
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
    /// <param name="timeout">
    ///     Optional request timeout. When provided, this value is applied to <see cref="HttpClient.Timeout"/>.
    ///     When not provided, the default timeout of <see cref="HttpClient"/> is used.
    /// </param>
    /// <param name="apiKey">
    ///     Optional API key. When supplied it is sent as the <c>X-Api-Key</c> header on
    ///     every request. Must match the key configured on the <see cref="SchedulerHost"/>.
    ///     Pass <see langword="null"/> or an empty string when the host has no key configured.
    /// </param>
    public SchedulerConnector(string schedulerHostAddress, TimeSpan? timeout = null, string apiKey = null)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(schedulerHostAddress);

        if (timeout.HasValue)
            _httpClient.Timeout = timeout.Value;

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
    ///     Reads the response body and throws an <see cref="InvalidOperationException"/> when
    ///     the server returns a non-success HTTP status code.
    /// </summary>
    /// <param name="response">The HTTP response message to read.</param>
    /// <returns>The raw response body string.</returns>
    /// <exception cref="SchedulerConnectorException">
    ///     Thrown when the server returns a non-success HTTP status code.
    /// </exception>
    private static async Task<string> ReadBodyAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new SchedulerConnectorException($"The scheduler host returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {body}");

        return body;
    }

    /// <summary>
    ///     Reads the HTTP response content as a string and removes any leading and trailing double-quote characters.
    /// </summary>
    /// <param name="response">The HTTP response message containing the content to read.</param>
    /// <returns>A string containing the response content with surrounding double quotes removed.</returns>
    private static async Task<string> ReadString(HttpResponseMessage response) => (await ReadBodyAsync(response).ConfigureAwait(false)).Trim('"');

    /// <summary>
    ///     Reads the HTTP response content as a string and parses it as a boolean value.
    /// </summary>
    /// <param name="response">The HTTP response message containing the content to read.</param>
    /// <returns>A boolean value parsed from the response content.</returns>
    private static async Task<bool> ReadBool(HttpResponseMessage response) => bool.Parse(await ReadBodyAsync(response).ConfigureAwait(false));

    /// <summary>
    ///     Reads the HTTP response content and deserializes it as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="response">The HTTP response message containing the content to read.</param>
    /// <returns>The deserialized value.</returns>
    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var body = await ReadBodyAsync(response).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(body);
    }
    #endregion

    #region IsJobGroupPaused
    /// <summary>
    ///     Returns <c>true</c> if the given JobGroup is paused
    /// </summary>
    /// <param name="groupName">The name of the job group.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> IsJobGroupPaused(string groupName, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"Scheduler/IsJobGroupPaused/{groupName}", cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region IsTriggerGroupPaused
    /// <summary>
    ///     Returns <c>true</c> if the given TriggerGroup is paused
    /// </summary>
    /// <param name="groupName">The name of the trigger group.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> IsTriggerGroupPaused(string groupName, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"Scheduler/IsTriggerGroupPaused/{groupName}", cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region SchedulerName
    /// <summary>
    ///     Returns the name of the scheduler
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<string> SchedulerName(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("Scheduler/SchedulerName", cancellationToken).ConfigureAwait(false);
        return await ReadString(response);
    }
    #endregion

    #region SchedulerInstanceId
    /// <summary>
    ///     Returns the instance id of the scheduler
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<string> SchedulerInstanceId(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("Scheduler/SchedulerInstanceId", cancellationToken).ConfigureAwait(false);
        return await ReadString(response);
    }
    #endregion

    #region Context
    /// <summary>
    ///     Returns the <see cref="SchedulerContext" /> of the scheduler
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<SchedulerContext> Context(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("Scheduler/SchedulerContext", cancellationToken).ConfigureAwait(false);
        var json = await ReadBodyAsync(response).ConfigureAwait(false);
        return SchedulerContext.FromJsonString(json);
    }
    #endregion

    #region InStandbyMode
    /// <summary>
    ///     Returns <c>true</c> if the scheduler is in standby mode
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> InStandbyMode(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("Scheduler/InStandbyMode", cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region IsShutdown
    /// <summary>
    ///     Returns <c>true</c> if the scheduler is shutdown
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> IsShutdown(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("Scheduler/Isshutdown", cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region IsStarted
    /// <summary>
    ///     Returns <c>true</c> if the scheduler has been started
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> IsStarted(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("Scheduler/IsStarted", cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region GetMetaData
    /// <summary>
    ///     Returns a <see cref="SchedulerMetaData"/> object describing the settings and capabilities of the scheduler
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<SchedulerMetaData> GetMetaData(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("Scheduler/GetMetaData", cancellationToken).ConfigureAwait(false);
        var json = await ReadBodyAsync(response).ConfigureAwait(false);
        return SchedulerMetaData.FromJsonString(json);
    }
    #endregion

    #region GetCurrentlyExecutingJobs
    /// <summary>
    ///     Returns the list of currently executing jobs
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<JobExecutionContexts> GetCurrentlyExecutingJobs(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("Scheduler/GetCurrentlyExecutingJobs", cancellationToken).ConfigureAwait(false);
        var json = await ReadBodyAsync(response).ConfigureAwait(false);
        return JobExecutionContexts.FromJsonString(json);
    }
    #endregion

    #region GetJobGroupNames
    /// <summary>
    ///     Returns the names of all known job groups
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<IReadOnlyCollection<string>> GetJobGroupNames(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("Scheduler/GetJobGroupNames", cancellationToken).ConfigureAwait(false);
        return await ReadJsonAsync<List<string>>(response).ConfigureAwait(false);
    }
    #endregion

    #region GetTriggerGroupNames
    /// <summary>
    ///     Returns the names of all known trigger groups
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<IReadOnlyCollection<string>> GetTriggerGroupNames(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("Scheduler/GetTriggerGroupNames", cancellationToken).ConfigureAwait(false);
        return await ReadJsonAsync<List<string>>(response).ConfigureAwait(false);
    }
    #endregion

    #region GetPausedTriggerGroups
    /// <summary>
    ///     Returns the names of all paused trigger groups
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<IReadOnlyCollection<string>> GetPausedTriggerGroups(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("Scheduler/GetPausedTriggerGroups", cancellationToken).ConfigureAwait(false);
        return await ReadJsonAsync<List<string>>(response).ConfigureAwait(false);
    }
    #endregion

    #region Start
    /// <summary>
    ///     Starts the scheduler
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task Start(CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/Start", null, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region StartDelayed
    /// <summary>
    ///     Starts the scheduler after the given delay in seconds
    /// </summary>
    /// <param name="delaySeconds">The delay in seconds.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task StartDelayed(int delaySeconds, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync($"Scheduler/StartDelayed/{delaySeconds}", null, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region Standby
    /// <summary>
    ///     Puts the scheduler in standby mode
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task Standby(CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/Standby", null, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region Shutdown
    /// <summary>
    ///     Shuts down the scheduler
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task Shutdown(CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/Shutdown", null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Shuts down the scheduler, optionally waiting for running jobs to complete
    /// </summary>
    /// <param name="waitForJobsToComplete">Whether to wait for jobs to complete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task Shutdown(bool waitForJobsToComplete, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync($"Scheduler/Shutdown/{waitForJobsToComplete}", null, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region ScheduleJob
    /// <summary>
    ///     Schedules a job with a job detail and a single trigger
    /// </summary>
    /// <param name="jobDetailWithTrigger">The job detail with trigger.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<DateTimeOffset> ScheduleJob(JobDetailWithTrigger jobDetailWithTrigger, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync("Scheduler/ScheduleJobWithJobDetailAndTrigger", JsonBody(jobDetailWithTrigger.ToJsonString()), cancellationToken).ConfigureAwait(false);
        return await ReadJsonAsync<DateTimeOffset>(response).ConfigureAwait(false);
    }

    /// <summary>
    ///     Schedules the trigger with the job identified by the trigger
    /// </summary>
    /// <param name="trigger">The trigger.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<DateTimeOffset> ScheduleJob(TriggerBase trigger, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync("Scheduler/ScheduleJobIdentifiedWithTrigger", JsonBody(trigger.ToJsonString()), cancellationToken).ConfigureAwait(false);
        return await ReadJsonAsync<DateTimeOffset>(response).ConfigureAwait(false);
    }

    /// <summary>
    ///     Schedules a job with a job detail and multiple triggers
    /// </summary>
    /// <param name="jobDetailWithTriggers">The job detail with triggers.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task ScheduleJobWithTriggers(JobDetailWithTriggers jobDetailWithTriggers, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/ScheduleJobWithJobDetailAndTriggers", JsonBody(jobDetailWithTriggers.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Schedules multiple jobs, each with their own set of triggers
    /// </summary>
    /// <param name="scheduleJobs">The jobs to schedule.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task ScheduleJobs(ScheduleJobs scheduleJobs, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/ScheduleJobs", JsonBody(scheduleJobs.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region UnscheduleJob
    /// <summary>
    ///     Removes the trigger with the given key from the scheduler
    /// </summary>
    /// <param name="triggerKey">The trigger key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> UnscheduleJob(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync("Scheduler/UnscheduleJob", JsonBody(triggerKey.ToJsonString()), cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }

    /// <summary>
    ///     Removes all triggers with the given keys from the scheduler
    /// </summary>
    /// <param name="triggerKeys">The trigger keys.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> UnscheduleJobs(TriggerKeys triggerKeys, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync("Scheduler/UnscheduleJobs", JsonBody(triggerKeys.ToJsonString()), cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region RescheduleJob
    /// <summary>
    ///     Replaces the current trigger with a new trigger for the same job
    /// </summary>
    /// <param name="rescheduleJob">The reschedule job details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<DateTimeOffset?> RescheduleJob(RescheduleJob rescheduleJob, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync("Scheduler/RescheduleJob", JsonBody(rescheduleJob.ToJsonString()), cancellationToken).ConfigureAwait(false);
        var json = await ReadBodyAsync(response).ConfigureAwait(false);
        if (json == "null") return null;
        return JsonSerializer.Deserialize<DateTimeOffset>(json);
    }
    #endregion

    #region AddJob
    /// <summary>
    ///     Adds a job with no associated trigger
    /// </summary>
    /// <param name="jobDetail">The job detail.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task AddJob(JobDetail jobDetail, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/AddJob", JsonBody(jobDetail.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region DeleteJob
    /// <summary>
    ///     Deletes the job with the given key and all associated triggers
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> DeleteJob(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "Scheduler/DeleteJob")
        {
            Content = JsonBody(jobKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }

    /// <summary>
    ///     Deletes all jobs with the given keys and their associated triggers
    /// </summary>
    /// <param name="jobKeys">The job keys.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> DeleteJobs(JobKeys jobKeys, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "Scheduler/DeleteJobs")
        {
            Content = JsonBody(jobKeys.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region TriggerJob
    /// <summary>
    ///     Triggers the identified job to execute immediately
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task TriggerJob(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/TriggerJobWithJobkey", JsonBody(jobKey.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Triggers the identified job to execute immediately with a data map
    /// </summary>
    /// <param name="jobKeyWithDataMap">The job key with data map.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task TriggerJob(JobKeyWithDataMap jobKeyWithDataMap, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/TriggerJobWithDataMap", JsonBody(jobKeyWithDataMap.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region PauseJob
    /// <summary>
    ///     Pauses the job with the given key
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task PauseJob(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/PauseJob", JsonBody(jobKey.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Pauses all jobs matching the given group matcher
    /// </summary>
    /// <param name="groupMatcher">The group matcher.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task PauseJobs(GroupMatcher<Quartz.JobKey> groupMatcher, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/PauseJobs", JsonBody(groupMatcher.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region PauseTrigger
    /// <summary>
    ///     Pauses the trigger with the given key
    /// </summary>
    /// <param name="triggerKey">The trigger key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task PauseTrigger(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/PauseTrigger", JsonBody(triggerKey.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Pauses all triggers matching the given group matcher
    /// </summary>
    /// <param name="groupMatcher">The group matcher.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task PauseTriggers(GroupMatcher<Quartz.TriggerKey> groupMatcher, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/PauseTriggers", JsonBody(groupMatcher.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Pauses all triggers
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task PauseAllTriggers(CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/PauseAllTriggers", null, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region ResumeJob
    /// <summary>
    ///     Resumes the job with the given key
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task ResumeJob(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/ResumeJob",
            JsonBody(jobKey.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Resumes all jobs matching the given group matcher
    /// </summary>
    /// <param name="groupMatcher">The group matcher.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task ResumeJobs(GroupMatcher<Quartz.JobKey> groupMatcher, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/ResumeJobs",
            JsonBody(groupMatcher.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region ResumeTrigger
    /// <summary>
    ///     Resumes the trigger with the given key
    /// </summary>
    /// <param name="triggerKey">The trigger key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task ResumeTrigger(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/ResumeTrigger",
            JsonBody(triggerKey.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Resumes all triggers matching the given group matcher
    /// </summary>
    /// <param name="groupMatcher">The group matcher.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task ResumeTriggers(GroupMatcher<Quartz.TriggerKey> groupMatcher, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/ResumeTriggers",
            JsonBody(groupMatcher.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Resumes all triggers
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task ResumeAllTriggers(CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/ResumeAllTriggers", null, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region GetJobKeys
    /// <summary>
    ///     Returns all job keys matching the given group matcher
    /// </summary>
    /// <param name="groupMatcher">The group matcher.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<JobKeys> GetJobKeys(GroupMatcher<Quartz.JobKey> groupMatcher, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/GetJobKeys")
        {
            Content = JsonBody(groupMatcher.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var json = await ReadBodyAsync(response).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(json) ? JobKeys.FromJsonString("[]") : JobKeys.FromJsonString(json);
    }
    #endregion

    #region GetTriggerKeys
    /// <summary>
    ///     Returns all trigger keys matching the given group matcher
    /// </summary>
    /// <param name="groupMatcher">The group matcher.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<TriggerKeys> GetTriggerKeys(GroupMatcher<Quartz.TriggerKey> groupMatcher, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/GetTriggerKeys")
        {
            Content = JsonBody(groupMatcher.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var json = await ReadBodyAsync(response).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(json) ? TriggerKeys.FromJsonString("[]") : TriggerKeys.FromJsonString(json);
    }
    #endregion

    #region GetTriggersOfJob
    /// <summary>
    ///     Returns all triggers associated with the given job key
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<Triggers> GetTriggersOfJob(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/GetTriggersOfJob")
        {
            Content = JsonBody(jobKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var json = await ReadBodyAsync(response).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(json) ? Triggers.FromJsonString("[]") : Triggers.FromJsonString(json);
    }
    #endregion

    #region GetJobDetail
    /// <summary>
    ///     Returns the job detail for the given job key
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<JobDetail> GetJobDetail(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/GetJobDetail")
        {
            Content = JsonBody(jobKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var json = await ReadBodyAsync(response).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(json) ? null : JobDetail.FromJsonString(json);
    }
    #endregion

    #region GetTrigger
    /// <summary>
    ///     Returns the trigger for the given trigger key
    /// </summary>
    /// <param name="triggerKey">The trigger key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<TriggerBase> GetTrigger(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/GetTrigger")
        {
            Content = JsonBody(triggerKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var json = await ReadBodyAsync(response).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(json) ? null : TriggerBase.FromJsonString(json);
    }
    #endregion

    #region GetTriggerState
    /// <summary>
    ///     Returns the current state of the given trigger
    /// </summary>
    /// <param name="triggerKey">The trigger key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<string> GetTriggerState(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/GetTriggerState")
        {
            Content = JsonBody(triggerKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return await ReadString(response);
    }
    #endregion

    #region AddCalendar
    /// <summary>
    ///     Adds a calendar to the scheduler
    /// </summary>
    /// <param name="calendar">The calendar to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task AddCalendar(BaseCalendar calendar, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/AddCalendar", JsonBody(calendar.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region DeleteCalendar
    /// <summary>
    ///     Deletes the calendar with the given name. Returns <c>true</c> when deleted.
    /// </summary>
    /// <param name="calendarName">The calendar name.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> DeleteCalendar(string calendarName, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"Scheduler/DeleteCalendar/{calendarName}");
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region GetCalendar
    /// <summary>
    ///     Returns the calendar with the given name as a JSON string
    /// </summary>
    /// <param name="calendarName">The calendar name.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<string> GetCalendar(string calendarName, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"Scheduler/Getcalendar/{calendarName}", cancellationToken).ConfigureAwait(false);
        return await ReadBodyAsync(response).ConfigureAwait(false);
    }
    #endregion

    #region GetCalendarNames
    /// <summary>
    ///     Returns the names of all registered calendars
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<IReadOnlyCollection<string>> GetCalendarNames(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("Scheduler/GetCalendarNames", cancellationToken).ConfigureAwait(false);
        return await ReadJsonAsync<List<string>>(response).ConfigureAwait(false);
    }
    #endregion

    #region Interrupt
    /// <summary>
    ///     Requests cancellation of all executing instances of the given job
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> InterruptJob(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/InterruptJobKey")
        {
            Content = JsonBody(jobKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }

    /// <summary>
    ///     Requests cancellation of the executing job instance with the given fire instance id
    /// </summary>
    /// <param name="fireInstanceId">The fire instance id.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> InterruptJob(string fireInstanceId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"Scheduler/interruptfireinstanceid/{fireInstanceId}", cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region CheckExists
    /// <summary>
    ///     Returns <c>true</c> if a job with the given key exists
    /// </summary>
    /// <param name="jobKey">The job key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> CheckExists(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/CheckExistsJobkey")
        {
            Content = JsonBody(jobKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }

    /// <summary>
    ///     Returns <c>true</c> if a trigger with the given key exists
    /// </summary>
    /// <param name="triggerKey">The trigger key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="Exceptions.SchedulerConnectorException">Thrown when the scheduler host returns a non-success HTTP status code.</exception>
    public async Task<bool> CheckExists(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Scheduler/CheckExistsTriggerkey")
        {
            Content = JsonBody(triggerKey.ToJsonString())
        };
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return await ReadBool(response);
    }
    #endregion

    #region Clear
    /// <summary>
    ///     Clears all scheduling data — all jobs, triggers and calendars
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task Clear(CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/Clear", null, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region ResetTriggerFromErrorState
    /// <summary>
    ///     Resets the given trigger from error state back to normal or paused
    /// </summary>
    /// <param name="triggerKey">The trigger key.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task ResetTriggerFromErrorState(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync("Scheduler/ResetTriggerFromErrorState",
            JsonBody(triggerKey.ToJsonString()), cancellationToken).ConfigureAwait(false);
    }
    #endregion
}