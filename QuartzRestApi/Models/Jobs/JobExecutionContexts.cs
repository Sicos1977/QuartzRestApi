//
// JobExecutionContexts.cs
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Quartz;

namespace QuartzRestApi.Models.Jobs;

/// <summary>
///     Json wrapper for a list of Quartz <see cref="Quartz.IJobExecutionContext" />s
/// </summary>
public class JobExecutionContexts : List<JobExecutionContext>
{
    #region Constructor
    /// <summary>
    ///     Parameterless constructor
    /// </summary>
    public JobExecutionContexts() { }

    /// <summary>
    ///     Makes this object and sets it's needed properties
    /// </summary>
    /// <param name="jobExecutionContexts">
    ///     A <see cref="ReadOnlyCollection{T}" /> of <see cref="Quartz.IJobExecutionContext" />
    /// </param>
    public JobExecutionContexts(IReadOnlyCollection<IJobExecutionContext> jobExecutionContexts)
    {
        foreach (var jobExecutionContext in jobExecutionContexts)
            Add(new JobExecutionContext(jobExecutionContext));
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
    ///     Returns the <see cref="JobExecutionContexts" /> object from the given <paramref name="json" /> string
    /// </summary>
    /// <param name="json">The json string</param>
    /// <returns>
    ///     <see cref="Trigger" />
    /// </returns>
    public static JobExecutionContexts FromJsonString(string json)
    {
        var list = JsonSerializer.Deserialize<List<JobExecutionContext>>(json);
        var result = new JobExecutionContexts();
        if (list != null)
            result.AddRange(list);
        return result;
    }
    #endregion
}