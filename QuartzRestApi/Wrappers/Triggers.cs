using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Quartz;

namespace QuartzRestApi.Wrappers;

/// <summary>
///    A list of <see cref="Quartz.ITrigger" />s
/// </summary>
public class Triggers : List<Trigger>
{
    #region Constructor
    /// <summary>
    ///     Makes this object and sets all it's needed properties
    /// </summary>
    /// <param name="triggers">A <see cref="ReadOnlyCollection{T}" /> of <see cref="Quartz.ITrigger" />s</param>
    public Triggers(IEnumerable<ITrigger> triggers)
    {
        foreach (var trigger in triggers)
            Add(new Trigger(trigger));
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
    ///     Returns the <see cref="Triggers" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="Trigger" />
    /// </returns>
    public static Triggers FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<Triggers>(json);
    }
    #endregion
}