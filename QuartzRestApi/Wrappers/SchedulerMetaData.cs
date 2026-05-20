//
// SchedulerMetaData.cs
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

namespace QuartzRestApi.Wrappers;

/// <summary>
///     Class used to read or create json to get the schedulers meta-data
/// </summary>
public class SchedulerMetaData
{
    #region Properties
    /// <summary>
    ///     Returns <c>true</c> when in standby mode
    /// </summary>
    [JsonPropertyName("InStandbyMode")]
    public bool InStandbyMode { get; init; }

    /// <summary>
    ///     Returns the job store type
    /// </summary>
    [JsonPropertyName("JobStoreType")]
    public string JobStoreType { get; init; }

    /// <summary>
    ///     Returns <c>true</c> when the job store is clustered
    /// </summary>
    [JsonPropertyName("JobStoreClustered")]
    public bool JobStoreClustered { get; init; }

    /// <summary>
    ///     Returns <c>true</c> when the job store supports persistence
    /// </summary>
    [JsonPropertyName("JobsStoreSupportsPersistence")]
    public bool JobStoreSupportsPersistence { get; init; }

    /// <summary>
    ///     Returns the numbers of jobs executed
    /// </summary>
    [JsonPropertyName("NumbersOfJobsExecuted")]
    public int NumbersOfJobsExecuted { get; init; }

    /// <summary>
    ///     Returns the date time since the scheduler is running
    /// </summary>
    [JsonPropertyName("RunningSince")]
    public DateTimeOffset? RunningSince { get; init; }

    /// <summary>
    ///     Returns the scheduler instance id
    /// </summary>
    [JsonPropertyName("SchedulerInstanceId")]
    public string SchedulerInstanceId { get; init; }

    /// <summary>
    ///     Returns the scheduler name
    /// </summary>
    [JsonPropertyName("SchedulerName")]
    public string SchedulerName { get; init; }

    /// <summary>
    ///     Returns <c>true</c> when the scheduler is remote
    /// </summary>
    [JsonPropertyName("SchedulerRemote")]
    public bool SchedulerRemote { get; init; }

    /// <summary>
    ///     Returns the scheduler type
    /// </summary>
    [JsonPropertyName("SchedulerType")]
    public string SchedulerType { get; init; }

    /// <summary>
    ///     Returns <c>true</c> when the scheduler is shutdown
    /// </summary>
    [JsonPropertyName("Shutdown")]
    public bool Shutdown { get; init; }

    /// <summary>
    ///     Returns <c>true</c> when the scheduler is started
    /// </summary>
    [JsonPropertyName("Started")]
    public bool Started { get; init; }

    /// <summary>
    ///     Returns the thread pool size
    /// </summary>
    [JsonPropertyName("ThreadPoolSize")]
    public int ThreadPoolSize { get; init; }

    /// <summary>
    ///     Returns the thread pool type
    /// </summary>
    [JsonPropertyName("ThreadPoolType")]
    public string ThreadPoolType { get; init; }

    /// <summary>
    ///     Returns the scheduler version
    /// </summary>
    [JsonPropertyName("Version")]
    public string Version { get; init; }

    /// <summary>
    ///     Returns the summary of the scheduler meta data
    /// </summary>
    [JsonPropertyName("Summary")]
    public string Summary { get; init; }
    #endregion

    #region Constructor
    /// <summary>
    ///     Needed for json de-serialization
    /// </summary>
    [JsonConstructor]
    internal SchedulerMetaData()
    {
    }

    /// <summary>
    ///     Creates this object and sets all it's needed properties
    /// </summary>
    /// <param name="metaData"></param>
    public SchedulerMetaData(Quartz.SchedulerMetaData metaData)
    {
        InStandbyMode = metaData.InStandbyMode;
        JobStoreType = metaData.JobStoreType?.FullName;
        JobStoreClustered = metaData.JobStoreClustered;
        JobStoreSupportsPersistence = metaData.JobStoreSupportsPersistence;
        NumbersOfJobsExecuted = metaData.NumberOfJobsExecuted;
        RunningSince = metaData.RunningSince;
        SchedulerInstanceId = metaData.SchedulerInstanceId;
        SchedulerName = metaData.SchedulerName;
        SchedulerRemote = metaData.SchedulerRemote;
        SchedulerType = metaData.SchedulerType.FullName;
        Shutdown = metaData.Shutdown;
        Started = metaData.Started;
        ThreadPoolSize = metaData.ThreadPoolSize;
        ThreadPoolType = metaData.ThreadPoolType.FullName;
        Version = metaData.Version;
        Summary = metaData.GetSummary();
    }
    #endregion

    #region ToJsonString
    /// <summary>
    ///     Returns this object as a json string
    /// </summary>
    /// <returns></returns>
    public string ToJsonString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
    #endregion

    #region FromJsonString
    /// <summary>
    ///     Returns the <see cref="SchedulerMetaData" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="Trigger" />
    /// </returns>
    public static SchedulerMetaData FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<SchedulerMetaData>(json);
    }
    #endregion
}