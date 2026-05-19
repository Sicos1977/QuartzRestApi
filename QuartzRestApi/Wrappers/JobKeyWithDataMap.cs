using System.Text.Json;
using System.Text.Json.Serialization;
using Quartz;

namespace QuartzRestApi.Wrappers;

/// <summary>
///     Json wrapper to create a <see cref="Quartz.JobKey" /> with a <see cref="Quartz.JobDataMap" />
/// </summary>
public class JobKeyWithDataMap
{
    #region Properties
    /// <summary>
    ///     The <see cref="JobKey" />
    /// </summary>
    [JsonPropertyName("JobKey")]
    public JobKey JobKey { get; private set; }

    /// <summary>
    ///     The <see cref="JobDataMap" />
    /// </summary>
    [JsonPropertyName("JobDataMap")]
    public JobDataMap JobDataMap { get; private set; }
    #endregion

    #region Constructor
    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="jobKey">The <see cref="JobKey" /></param>
    /// <param name="jobDataMap">The <see cref="JobDataMap" /></param>
    public JobKeyWithDataMap(JobKey jobKey, JobDataMap jobDataMap)
    {
        JobKey = jobKey;
        JobDataMap = jobDataMap;
    }

    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    public JobKeyWithDataMap()
    {
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
    ///     Returns the <see cref="JobKeyWithDataMap" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="Trigger" />
    /// </returns>
    public static JobKeyWithDataMap FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<JobKeyWithDataMap>(json);
    }
    #endregion
}