using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuartzRestApi.Wrappers;

/// <summary>
///     A json wrapper for the <see cref="Quartz.TriggerKey" />
/// </summary>
public class TriggerKey : Key
{
    #region Constructor
    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="name">The name of the trigger</param>
    [JsonConstructor]
    public TriggerKey(string name) : base(name)
    {
    }

    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="name">The name of the trigger</param>
    /// <param name="group">The group of the trigger</param>
    public TriggerKey(string name, string group) : base(name, group)
    {
    }

    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="key">The <see cref="Quartz.TriggerKey" /></param>
    public TriggerKey(Quartz.TriggerKey key) : base(key.Name, key.Group)
    {
    }
    #endregion

    #region ToTriggerKey
    /// <summary>
    ///     Returns this object as a Quartz <see cref="Quartz.TriggerKey" />
    /// </summary>
    /// <returns></returns>
    public Quartz.TriggerKey ToTriggerKey()
    {
        return new Quartz.TriggerKey(Name, Group);
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
    ///     Returns the <see cref="TriggerKey" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="Trigger" />
    /// </returns>
    public static TriggerKey FromJsonString(string json)
    {
        return JsonSerializer.Deserialize<TriggerKey>(json);
    }
    #endregion
}