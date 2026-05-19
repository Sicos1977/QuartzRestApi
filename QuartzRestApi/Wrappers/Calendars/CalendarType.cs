//
// CalendarType.cs
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