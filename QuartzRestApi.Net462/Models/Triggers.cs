//
// Triggers.cs
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
using Newtonsoft.Json;
using Quartz;

namespace QuartzRestApi.Models;
/// <summary>A list of <see cref="Trigger"/>s.</summary>
public class Triggers : List<Trigger>
{
    public Triggers() { }

    public Triggers(IEnumerable<ITrigger> triggers)
    {
        foreach (var t in triggers)
            Add(new Trigger(t));
    }

    public IReadOnlyCollection<ITrigger> ToReadOnlyTriggerCollection()
        => new ReadOnlyCollection<ITrigger>(ConvertAll(t => t.ToTrigger()));

    public string ToJsonString() => JsonConvert.SerializeObject(this, Formatting.Indented);

    public static Triggers FromJsonString(string json) => JsonConvert.DeserializeObject<Triggers>(json);
}

