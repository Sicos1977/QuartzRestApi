//
// ApiKeyProfile.cs
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
using System.Text.Json;
using System.Text.Json.Serialization;
using QuartzRestApi.Models;

namespace QuartzRestApi.Security;

/// <summary>
///     Represents a named API key profile that controls which Scheduler endpoints
///     the bearer of <see cref="ApiKey"/> is allowed to call.
/// </summary>
/// <remarks>
///     <para>
///         Every endpoint has a corresponding <see langword="bool"/> property on this
///         class. All properties default to <see langword="true"/> (full access).
///         Set a property to <see langword="false"/> to block that endpoint for this profile.
///     </para>
///     <para>
///         The profile can be persisted to JSON with <see cref="ToJson"/> and restored
///         with <see cref="FromJson"/>, making it easy to store profiles in a configuration
///         file or database.
///     </para>
/// </remarks>
/// <example>
/// <code>
/// // Admin profile — full access to all endpoints.
/// var admin = ApiKeyProfile.AllowAll("Admin", "key-admin-xyz789");
///
/// // Read-only profile — start with no access, then selectively enable query endpoints.
/// var readOnly = ApiKeyProfile.DenyAll("ReadOnly", "key-readonly-abc123");
/// readOnly.SchedulerName          = true;
/// readOnly.SchedulerInstanceId    = true;
/// readOnly.GetMetaData            = true;
/// readOnly.GetJobGroupNames       = true;
/// readOnly.GetTriggerGroupNames   = true;
/// readOnly.GetJobKeys             = true;
/// readOnly.GetJobDetail           = true;
/// readOnly.GetTrigger             = true;
/// readOnly.GetTriggerState        = true;
/// readOnly.GetCurrentlyExecutingJobs = true;
///
/// var host = new SchedulerHost("http://localhost:45000", scheduler, logger,
///     profiles: [admin, readOnly]);
///
/// // Persist a profile to disk and reload it:
/// File.WriteAllText("readOnly.json", readOnly.ToJson());
/// var loaded = ApiKeyProfile.FromJson(File.ReadAllText("readOnly.json"));
/// </code>
/// </example>
public sealed class ApiKeyProfile
{
    #region Properties
    /// <summary>
    ///     A human-readable name for this profile, used in log messages.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     The secret API key that identifies this profile.
    ///     Sent by the client in the <c>X-Api-Key</c> HTTP header.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    #endregion

    #region Scheduler status
    /// <summary>
    ///     Allows access to <c>GET Scheduler/IsJobGroupPaused/{groupName}</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.IsJobGroupPaused"/>.
    /// </summary>
    public bool IsJobGroupPaused { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/IsTriggerGroupPaused/{groupName}</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.IsTriggerGroupPaused"/>.
    /// </summary>
    public bool IsTriggerGroupPaused { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/SchedulerName</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.SchedulerName"/>.
    /// </summary>
    public bool SchedulerName { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/SchedulerInstanceId</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.SchedulerInstanceId"/>.
    /// </summary>
    public bool SchedulerInstanceId { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/SchedulerContext</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.Context"/>.
    /// </summary>
    public bool SchedulerContext { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/InStandbyMode</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.InStandbyMode"/>.
    /// </summary>
    public bool InStandbyMode { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/IsShutdown</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.IsShutdown"/>.
    /// </summary>
    public bool IsShutdown { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/IsStarted</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.IsStarted"/>.
    /// </summary>
    public bool IsStarted { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetMetaData</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetMetaData"/>.
    /// </summary>
    public bool GetMetaData { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetCurrentlyExecutingJobs</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetCurrentlyExecutingJobs"/>.
    /// </summary>
    public bool GetCurrentlyExecutingJobs { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetJobGroupNames</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetJobGroupNames"/>.
    /// </summary>
    public bool GetJobGroupNames { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetTriggerGroupNames</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetTriggerGroupNames"/>.
    /// </summary>
    public bool GetTriggerGroupNames { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetPausedTriggerGroups</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetPausedTriggerGroups"/>.
    /// </summary>
    public bool GetPausedTriggerGroups { get; set; }
    #endregion

    #region Scheduler lifecycle
    /// <summary>
    ///     Allows access to <c>POST Scheduler/Start</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.Start"/>.
    /// </summary>
    public bool Start { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/StartDelayed/{delay}</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.StartDelayed"/>.
    /// </summary>
    public bool StartDelayed { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/Standby</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.Standby"/>.
    /// </summary>
    public bool Standby { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/Shutdown</c> and
    ///     <c>POST Scheduler/Shutdown/{waitForJobsToComplete}</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.Shutdown()"/> and
    ///     <see cref="SchedulerConnector.Shutdown(bool)"/>.
    /// </summary>
    public bool Shutdown { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/Clear</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.Clear"/>.
    /// </summary>
    public bool Clear { get; set; }
    #endregion

    #region Scheduling
    /// <summary>
    ///     Allows access to <c>POST Scheduler/ScheduleJobWithJobDetailAndTrigger</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.ScheduleJob(JobDetailWithTrigger)"/>.
    /// </summary>
    public bool ScheduleJobWithJobDetailAndTrigger { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/ScheduleJobIdentifiedWithTrigger</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.ScheduleJob(Trigger)"/>.
    /// </summary>
    public bool ScheduleJobIdentifiedWithTrigger { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/ScheduleJobWithJobDetailAndTriggers</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.ScheduleJobWithTriggers"/>.
    /// </summary>
    public bool ScheduleJobWithJobDetailAndTriggers { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/ScheduleJobs</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.ScheduleJobs"/>.
    /// </summary>
    public bool ScheduleJobs { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/RescheduleJob</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.RescheduleJob"/>.
    /// </summary>
    public bool RescheduleJob { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/UnscheduleJob</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.UnscheduleJob"/>.
    /// </summary>
    public bool UnscheduleJob { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/UnscheduleJobs</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.UnscheduleJobs"/>.
    /// </summary>
    public bool UnscheduleJobs { get; set; }
    #endregion

    #region Jobs
    /// <summary>
    ///     Allows access to <c>POST Scheduler/AddJob</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.AddJob"/>.
    /// </summary>
    public bool AddJob { get; set; }

    /// <summary>
    ///     Allows access to <c>DELETE Scheduler/DeleteJob</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.DeleteJob(JobKey)"/>.
    /// </summary>
    public bool DeleteJob { get; set; }

    /// <summary>
    ///     Allows access to <c>DELETE Scheduler/DeleteJobs</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.DeleteJobs"/>.
    /// </summary>
    public bool DeleteJobs { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/TriggerJobWithJobkey</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.TriggerJob(JobKey)"/>.
    /// </summary>
    public bool TriggerJobWithJobkey { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/TriggerJobWithDataMap</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.TriggerJob(JobKeyWithDataMap)"/>.
    /// </summary>
    public bool TriggerJobWithDataMap { get; set; }
    #endregion

    #region Pause and resume
    /// <summary>
    ///     Allows access to <c>POST Scheduler/PauseJob</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.PauseJob"/>.
    /// </summary>
    public bool PauseJob { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/PauseJobs</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.PauseJobs"/>.
    /// </summary>
    public bool PauseJobs { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/PauseTrigger</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.PauseTrigger"/>.
    /// </summary>
    public bool PauseTrigger { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/PauseTriggers</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.PauseTriggers"/>.
    /// </summary>
    public bool PauseTriggers { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/PauseAllTriggers</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.PauseAllTriggers"/>.
    /// </summary>
    public bool PauseAllTriggers { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/ResumeJob</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.ResumeJob"/>.
    /// </summary>
    public bool ResumeJob { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/ResumeJobs</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.ResumeJobs"/>.
    /// </summary>
    public bool ResumeJobs { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/ResumeTrigger</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.ResumeTrigger"/>.
    /// </summary>
    public bool ResumeTrigger { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/ResumeTriggers</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.ResumeTriggers"/>.
    /// </summary>
    public bool ResumeTriggers { get; set; }

    /// <summary>
    ///     Allows access to <c>POST Scheduler/ResumeAllTriggers</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.ResumeAllTriggers"/>.
    /// </summary>
    public bool ResumeAllTriggers { get; set; }
    #endregion

    #region Query
    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetJobKeys</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetJobKeys"/>.
    /// </summary>
    public bool GetJobKeys { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetTriggerKeys</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetTriggerKeys"/>.
    /// </summary>
    public bool GetTriggerKeys { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetTriggersOfJob</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetTriggersOfJob"/>.
    /// </summary>
    public bool GetTriggersOfJob { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetJobDetail</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetJobDetail"/>.
    /// </summary>
    public bool GetJobDetail { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetTrigger</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetTrigger"/>.
    /// </summary>
    public bool GetTrigger { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetTriggerState</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetTriggerState"/>.
    /// </summary>
    public bool GetTriggerState { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/CheckExistsJobKey</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.CheckExists(JobKey)"/>.
    /// </summary>
    public bool CheckExistsJobKey { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/CheckExistsTriggerKey</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.CheckExists(TriggerKey)"/>.
    /// </summary>
    public bool CheckExistsTriggerKey { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/InterruptJobKey</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.InterruptJob(JobKey)"/>.
    /// </summary>
    public bool InterruptJobKey { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/InterruptFireInstanceId/{fireInstanceId}</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.InterruptJob(string)"/>.
    /// </summary>
    public bool InterruptFireInstanceId { get; set; }
    #endregion

    #region Calendars
    /// <summary>
    ///     Allows access to <c>POST Scheduler/AddCalendar</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.AddCalendar"/>.
    /// </summary>
    public bool AddCalendar { get; set; }

    /// <summary>
    ///     Allows access to <c>DELETE Scheduler/DeleteCalendar/{calName}</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.DeleteCalendar"/>.
    /// </summary>
    public bool DeleteCalendar { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetCalendar/{calName}</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetCalendar"/>.
    /// </summary>
    public bool GetCalendar { get; set; }

    /// <summary>
    ///     Allows access to <c>GET Scheduler/GetCalendarNames</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.GetCalendarNames"/>.
    /// </summary>
    public bool GetCalendarNames { get; set; }
    #endregion

    #region Error state
    /// <summary>
    ///     Allows access to <c>POST Scheduler/ResetTriggerFromErrorState</c>.
    ///     <br/>Corresponds to <see cref="SchedulerConnector.ResetTriggerFromErrorState"/>.
    /// </summary>
    public bool ResetTriggerFromErrorState { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    ///     Initializes a new instance of the ApiKeyProfile class with the specified name, API key, and access permission.
    /// </summary>
    /// <param name="name">The name that identifies the API key profile. Cannot be null or empty.</param>
    /// <param name="apiKey">The API key associated with this profile. Cannot be null or empty.</param>
    /// <param name="allow">A value indicating whether access is allowed for this profile. Set to <see langword="true"/> to allow access;
    /// otherwise, <see langword="false"/>.</param>
    private ApiKeyProfile(string name, string apiKey, bool allow)
    {
        Name = name;
        ApiKey = apiKey;
        SetAll(allow);
    }

    /// <summary>
    ///     Parameterless constructor used during JSON deserialization.
    /// </summary>
    [JsonConstructor]
    public ApiKeyProfile() { }
    #endregion

    #region Factory methods
    /// <summary>
    ///     Creates a new <see cref="ApiKeyProfile"/> with access to <b>all</b> endpoints enabled.
    ///     Use this as a starting point for an admin profile, or selectively set properties
    ///     to <see langword="false"/> to restrict access.
    /// </summary>
    /// <param name="name">A human-readable name for this profile, used in log messages.</param>
    /// <param name="apiKey">The secret API key for this profile, sent in the <c>X-Api-Key</c> header.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="name"/> or <paramref name="apiKey"/> is null or empty.
    /// </exception>
    public static ApiKeyProfile AllowAll(string name, string apiKey)
    {
        ValidateArguments(name, apiKey);
        return new ApiKeyProfile(name, apiKey, allow: true);
    }

    /// <summary>
    ///     Creates a new <see cref="ApiKeyProfile"/> with access to <b>all</b> endpoints denied.
    ///     Use this as a starting point for a restricted profile, then selectively set properties
    ///     to <see langword="true"/> to grant access to specific endpoints.
    /// </summary>
    /// <param name="name">A human-readable name for this profile, used in log messages.</param>
    /// <param name="apiKey">The secret API key for this profile, sent in the <c>X-Api-Key</c> header.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="name"/> or <paramref name="apiKey"/> is null or empty.
    /// </exception>
    public static ApiKeyProfile DenyAll(string name, string apiKey)
    {
        ValidateArguments(name, apiKey);
        return new ApiKeyProfile(name, apiKey, allow: false);
    }
    #endregion

    #region ValidateArguments
    /// <summary>
    ///    Validates the arguments for the factory methods, throwing an exception if either
    ///    <paramref name="name"/> or <paramref name="apiKey"/> is null or empty.
    /// </summary>
    /// <param name="name">The name of the profile.</param>
    /// <param name="apiKey">The API key for the profile.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> or <paramref name="apiKey"/> is null or empty.</exception>
    private static void ValidateArguments(string name, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Profile name must not be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key must not be empty.", nameof(apiKey));
    }
    #endregion

    #region SetAll
    /// <summary>
    ///     Sets the specified value for all related scheduler state and operation flags.
    /// </summary>
    /// <remarks>
    ///     This method is typically used to quickly initialize or reset all scheduler-related flags to a
    ///     consistent state. Use with caution, as it affects a wide range of scheduler properties and operations.
    /// </remarks>
    /// <param name="value">true to enable all scheduler states and operations; false to disable them.</param>
    private void SetAll(bool value)
    {
        IsJobGroupPaused = value;
        IsTriggerGroupPaused = value;
        SchedulerName = value;
        SchedulerInstanceId = value;
        SchedulerContext = value;
        InStandbyMode = value;
        IsShutdown = value;
        IsStarted = value;
        GetMetaData = value;
        GetCurrentlyExecutingJobs = value;
        GetJobGroupNames = value;
        GetTriggerGroupNames = value;
        GetPausedTriggerGroups = value;
        Start = value;
        StartDelayed = value;
        Standby = value;
        Shutdown = value;
        Clear = value;
        ScheduleJobWithJobDetailAndTrigger = value;
        ScheduleJobIdentifiedWithTrigger = value;
        ScheduleJobWithJobDetailAndTriggers = value;
        ScheduleJobs = value;
        RescheduleJob = value;
        UnscheduleJob = value;
        UnscheduleJobs = value;
        AddJob = value;
        DeleteJob = value;
        DeleteJobs = value;
        TriggerJobWithJobkey = value;
        TriggerJobWithDataMap = value;
        PauseJob = value;
        PauseJobs = value;
        PauseTrigger = value;
        PauseTriggers = value;
        PauseAllTriggers = value;
        ResumeJob = value;
        ResumeJobs = value;
        ResumeTrigger = value;
        ResumeTriggers = value;
        ResumeAllTriggers = value;
        GetJobKeys = value;
        GetTriggerKeys = value;
        GetTriggersOfJob = value;
        GetJobDetail = value;
        GetTrigger = value;
        GetTriggerState = value;
        CheckExistsJobKey = value;
        CheckExistsTriggerKey = value;
        InterruptJobKey = value;
        InterruptFireInstanceId = value;
        AddCalendar = value;
        DeleteCalendar = value;
        GetCalendar = value;
        GetCalendarNames = value;
        ResetTriggerFromErrorState = value;
    }
    #endregion

    #region IsRouteAllowed
    /// <summary>
    ///     Returns <see langword="true"/> when this profile grants access to
    ///     <paramref name="requestPath"/>.
    /// </summary>
    /// <param name="requestPath">
    ///     The HTTP request path without leading slash,
    ///     e.g. <c>"Scheduler/Start"</c>.
    /// </param>
    internal bool IsRouteAllowed(string requestPath) => requestPath.ToLowerInvariant() switch
    {
        "scheduler/isjobgrouppaused" or
        "scheduler/isjobgrouppaused/{groupname}"             => IsJobGroupPaused,
        "scheduler/istriggergrouppaused" or
        "scheduler/istriggergrouppaused/{groupname}"         => IsTriggerGroupPaused,
        "scheduler/schedulername"                            => SchedulerName,
        "scheduler/schedulerinstanceid"                      => SchedulerInstanceId,
        "scheduler/schedulercontext"                         => SchedulerContext,
        "scheduler/instandbymode"                            => InStandbyMode,
        "scheduler/isshutdown"                               => IsShutdown,
        "scheduler/isstarted"                                => IsStarted,
        "scheduler/getmetadata"                              => GetMetaData,
        "scheduler/getcurrentlyexecutingjobs"                => GetCurrentlyExecutingJobs,
        "scheduler/getjobgroupnames"                         => GetJobGroupNames,
        "scheduler/gettriggergroupnames"                     => GetTriggerGroupNames,
        "scheduler/getpausedtriggergroups"                   => GetPausedTriggerGroups,
        "scheduler/start"                                    => Start,
        "scheduler/startdelayed" or
        "scheduler/startdelayed/{delay}"                     => StartDelayed,
        "scheduler/standby"                                  => Standby,
        "scheduler/shutdown" or
        "scheduler/shutdown/{waitforjobstocomplete}"         => Shutdown,
        "scheduler/clear"                                    => Clear,
        "scheduler/schedulejobwithjobdetailandtrigger"       => ScheduleJobWithJobDetailAndTrigger,
        "scheduler/schedulejobidentifiedwithtrigger"         => ScheduleJobIdentifiedWithTrigger,
        "scheduler/schedulejobwithjobdetailandtriggers"      => ScheduleJobWithJobDetailAndTriggers,
        "scheduler/schedulejobs"                             => ScheduleJobs,
        "scheduler/reschedulejob"                            => RescheduleJob,
        "scheduler/unschedulejob"                            => UnscheduleJob,
        "scheduler/unschedulejobs"                           => UnscheduleJobs,
        "scheduler/addjob"                                   => AddJob,
        "scheduler/deletejob"                                => DeleteJob,
        "scheduler/deletejobs"                               => DeleteJobs,
        "scheduler/triggerjobwithjobkey"                     => TriggerJobWithJobkey,
        "scheduler/triggerjobwithdatamap"                    => TriggerJobWithDataMap,
        "scheduler/pausejob"                                 => PauseJob,
        "scheduler/pausejobs"                                => PauseJobs,
        "scheduler/pausetrigger"                             => PauseTrigger,
        "scheduler/pausetriggers"                            => PauseTriggers,
        "scheduler/pausealltriggers"                         => PauseAllTriggers,
        "scheduler/resumejob"                                => ResumeJob,
        "scheduler/resumejobs"                               => ResumeJobs,
        "scheduler/resumetrigger"                            => ResumeTrigger,
        "scheduler/resumetriggers"                           => ResumeTriggers,
        "scheduler/resumealltriggers"                        => ResumeAllTriggers,
        "scheduler/getjobkeys"                               => GetJobKeys,
        "scheduler/gettriggerkeys"                           => GetTriggerKeys,
        "scheduler/gettriggersofjob"                         => GetTriggersOfJob,
        "scheduler/getjobdetail"                             => GetJobDetail,
        "scheduler/gettrigger"                               => GetTrigger,
        "scheduler/gettriggerstate"                          => GetTriggerState,
        "scheduler/checkexistsjobkey"                        => CheckExistsJobKey,
        "scheduler/checkexiststriggerkey"                    => CheckExistsTriggerKey,
        "scheduler/interruptjobkey"                          => InterruptJobKey,
        "scheduler/interruptfireinstanceid" or
        "scheduler/interruptfireinstanceid/{fireinstanceid}" => InterruptFireInstanceId,
        "scheduler/addcalendar"                              => AddCalendar,
        "scheduler/deletecalendar" or
        "scheduler/deletecalendar/{calname}"                 => DeleteCalendar,
        "scheduler/getcalendar" or
        "scheduler/getcalendar/{calname}"                    => GetCalendar,
        "scheduler/getcalendarnames"                         => GetCalendarNames,
        "scheduler/resettriggerfromerrorstate"               => ResetTriggerFromErrorState,
        _                                                    => true  // unknown routes pass through
    };
    #endregion

    #region JSON serialization
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null   // keep PascalCase property names
    };

    /// <summary>
    ///     Serializes this profile to an indented JSON string.
    /// </summary>
    /// <returns>A JSON representation of this profile.</returns>
    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    /// <summary>
    ///     Deserializes an <see cref="ApiKeyProfile"/> from a JSON string
    ///     produced by <see cref="ToJson"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="ApiKeyProfile"/>.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="json"/> is null or whitespace.
    /// </exception>
    /// <exception cref="JsonException">
    ///     Thrown when <paramref name="json"/> is not valid JSON.
    /// </exception>
    public static ApiKeyProfile FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON must not be empty.", nameof(json));

        return JsonSerializer.Deserialize<ApiKeyProfile>(json, JsonOptions) ?? throw new JsonException("Deserialization returned null.");
    }
    #endregion
}
