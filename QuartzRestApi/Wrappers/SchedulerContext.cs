using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuartzRestApi.Wrappers;

/// <summary>
///     A json wrapper for the <see cref="Quartz.SchedulerContext" />
/// </summary>
public sealed class SchedulerContext : Quartz.SchedulerContext
{
    #region Constructor
    /// <summary>
    ///     Needed for json de-serialization
    /// </summary>
    [JsonConstructor]
    internal SchedulerContext()
    {

    }

    internal SchedulerContext(Quartz.SchedulerContext schedulerContext)
    {
        foreach (var item in schedulerContext)
            Add(item.Key, item.Value);
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
    ///     Returns the <see cref="SchedulerContext" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="Trigger" />
    /// </returns>
    public static SchedulerContext FromJsonString(string json)
    {
        var str = JsonSerializer.Deserialize<string>(json);
        var result = JsonSerializer.Deserialize<SchedulerContext>(str);
        return new SchedulerContext(result);
    }
    #endregion
}