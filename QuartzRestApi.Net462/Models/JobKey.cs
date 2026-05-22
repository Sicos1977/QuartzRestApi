//
// JobKey.cs
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

using Newtonsoft.Json;

namespace QuartzRestApi.Models;
/// <summary>JSON wrapper for <see cref="Quartz.JobKey"/>.</summary>
public class JobKey : Key
{
    [JsonConstructor]
    public JobKey(string name) : base(name) { }
    public JobKey(string name, string group) : base(name, group) { }
    public JobKey(Quartz.JobKey key) : base(key.Name, key.Group) { }

    public Quartz.JobKey ToJobKey() => new Quartz.JobKey(Name, Group);

    public string ToJsonString() => JsonConvert.SerializeObject(this, Formatting.Indented);

    public static JobKey FromJsonString(string json) => JsonConvert.DeserializeObject<JobKey>(json);
}

