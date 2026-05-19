using System.Runtime.Serialization;

namespace QuartzRestApi.Wrappers;

/// <summary>
///     The matching type
/// </summary>
public enum GroupMatcherType
{
    /// <summary>
    ///     Should contain
    /// </summary>
    [DataMember(Name = "Contains")] 
    Contains,

    /// <summary>
    ///     Should end with
    /// </summary>
    [DataMember(Name = "EndsWith")] 
    EndsWith,

    /// <summary>
    ///     Should equal
    /// </summary>
    [DataMember(Name = "Equals")] 
    Equals,

    /// <summary>
    ///     Should start with
    /// </summary>
    [DataMember(Name = "StartsWith")] 
    StartsWith
}