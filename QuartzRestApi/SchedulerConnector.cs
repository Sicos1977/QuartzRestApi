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
using System.Net.Http;
using System.Threading.Tasks;
using QuartzRestApi.Wrappers;
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
    public SchedulerConnector(string schedulerHostAddress)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(schedulerHostAddress);
    }
    #endregion

    #region IsJobGroupPaused
    /// <summary>
    ///     Returns <c>true</c> if the given JobGroup is paused
    /// </summary>
    /// <param name="groupName">The group name</param>
    /// <returns></returns>
    public async Task<bool> IsJobGroupPaused(string groupName)
    {
        var response = await _httpClient.GetAsync($"Scheduler/IsJobGroupPaused/{groupName}").ConfigureAwait(false);
        return bool.Parse(await response.Content.ReadAsStringAsync());
    }
    #endregion

    #region IsTriggerGroupPaused
    /// <summary>
    ///     Returns <c>true</c> if the given TriggerGroup is paused
    /// </summary>
    /// <param name="groupName">The group name</param>
    /// <returns></returns>
    public async Task<bool> IsTriggerGroupPaused(string groupName)
    {
        var response = await _httpClient.GetAsync($"Scheduler/IsTriggerGroupPaused/{groupName}").ConfigureAwait(false);
        return bool.Parse(await response.Content.ReadAsStringAsync());
    }
    #endregion

    #region SchedulerName
    /// <summary>
    ///     Returns the name of the scheduler
    /// </summary>
    public async Task<string> SchedulerName()
    {
        var response = await _httpClient.GetAsync("Scheduler/SchedulerName").ConfigureAwait(false);
        var result =  await response.Content.ReadAsStringAsync();
        return result.Trim('\"');
    }
    #endregion

    #region SchedulerInstanceId
    /// <summary>
    ///     Returns the instance id of the scheduler
    /// </summary>
    public async Task<string> SchedulerInstanceId()
    {
        var response = await _httpClient.GetAsync("Scheduler/SchedulerInstanceId").ConfigureAwait(false);
        var result =  await response.Content.ReadAsStringAsync();
        return result.Trim('\"');
    }
    #endregion

    #region Context
    /// <summary>
    ///     Returns the <see cref="SchedulerContext" /> of the <see cref="Quartz.IScheduler" />.
    /// </summary>
    public async Task<SchedulerContext> Context()
    {
        var response = await _httpClient.GetAsync("Scheduler/SchedulerContext").ConfigureAwait(false);
        var result =  await response.Content.ReadAsStringAsync();
        return SchedulerContext.FromJsonString(result);
    }
    #endregion

    #region GetMetaData
    /// <summary>
    ///     Get a <see cref="Quartz.SchedulerMetaData" /> object describing the settings
    ///     and capabilities of the scheduler instance.
    /// </summary>
    /// <remarks>
    ///     Note that the data returned is an 'instantaneous' snapshot, and that as
    ///     soon as it's returned, the meta-data values may be different.
    /// </remarks>
    public async Task<SchedulerMetaData> GetMetaData()
    {
        var response = await _httpClient.GetAsync("Scheduler/GetMetaData").ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync();
        return SchedulerMetaData.FromJsonString(json);
    }
    #endregion

    public async Task Start()
    {
        await _httpClient.PostAsync("Scheduler/Start", null).ConfigureAwait(false);
    }

    public async Task StartDelayed(int delay)
    {
        var response = await _httpClient.PostAsync($"Scheduler/StartDelayed/{delay}", null);
        // Handle response if necessary
    }
}