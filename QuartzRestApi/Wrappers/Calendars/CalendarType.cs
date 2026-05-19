using System.Runtime.Serialization;

namespace QuartzRestApi.Wrappers.Calendars;

public enum CalendarType
{
    /// <summary>
    ///     It is a cron calendar
    /// </summary>
    [DataMember(Name = "Cron")] 
    Cron,

    /// <summary>
    ///     It is a daily calendar
    /// </summary>
    [DataMember(Name = "Daily")] 
    Daily,

    /// <summary>
    ///     It is a week calendar
    /// </summary>
    [DataMember(Name = "Weekly")] 
    Weekly,

    /// <summary>
    ///     It is a monthly calendar
    /// </summary>
    [DataMember(Name = "Monthly")] 
    Monthly,

    /// <summary>
    ///     It is an annual calendar
    /// </summary>
    [DataMember(Name = "Annual")] 
    Annual,

    /// <summary>
    ///     It is a holiday calendar
    /// </summary>
    [DataMember(Name = "Holiday")] 
    Holiday
}