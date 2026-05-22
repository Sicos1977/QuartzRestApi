//
// SchedulerDependencyResolver.cs
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

using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Quartz;

namespace QuartzRestApi;
/// <summary>
///     Minimal Web API 2 dependency resolver that supplies the <see cref="IScheduler"/>
///     to <see cref="SchedulerController"/>.
/// </summary>
internal sealed class SchedulerDependencyResolver : IDependencyResolver
{
    #region Fields
    private readonly IScheduler _scheduler;
    #endregion

    #region Constructor
    /// <summary>
    ///     Initializes a new instance of the <see cref="SchedulerDependencyResolver"/> class
    /// </summary>
    /// <param name="scheduler">The Quartz.NET scheduler instance.</param>
    internal SchedulerDependencyResolver(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }
    #endregion

    #region GetService
    public object GetService(Type serviceType)
    {
        return serviceType == typeof(SchedulerController) ? new SchedulerController(_scheduler) : null;
    }
    #endregion

    #region GetServices
    /// <summary>
    ///     Retrieves all services of the specified type.
    /// </summary>
    /// <param name="serviceType">The type of service objects to retrieve.</param>
    /// <returns>An empty collection of objects.</returns>
    public IEnumerable<object> GetServices(Type serviceType) => Array.Empty<object>();
    #endregion

    #region BeginScope
    /// <summary>
    ///     Begins a new dependency resolution scope.
    /// </summary>
    /// <returns>The current scope instance.</returns>
    public IDependencyScope BeginScope() => this;
    #endregion

    #region Dispose
    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() { }
    #endregion
}

