//
// SchedulerHost.cs
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
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using Quartz;
using QuartzRestApi.Security;

namespace QuartzRestApi;
/// <summary>
///     Hosts a self-contained REST API for managing and monitoring a Quartz.NET scheduler
///     using OWIN/Web API 2 self-hosting on .NET Framework 4.6.2.
/// </summary>
public class SchedulerHost : IDisposable
{
    #region Fields
    private readonly string _baseAddress;
    private readonly IScheduler _scheduler;
    private readonly ApiKeyOptions _apiKeyOptions;
    private IDisposable _owinApp;
    #endregion

    #region Constructors
    /// <summary>Creates a host without authentication.</summary>
    public SchedulerHost(string baseAddress, IScheduler scheduler)
        : this(baseAddress, scheduler, (string)null) { }

    /// <summary>
    ///     Creates a host with a single API key.
    ///     Pass <see langword="null"/> or empty to disable authentication.
    /// </summary>
    public SchedulerHost(string baseAddress, IScheduler scheduler, string apiKey)
        : this(baseAddress, scheduler, new ApiKeyOptions(apiKey)) { }

    /// <summary>Creates a host with multiple named API key profiles.</summary>
    public SchedulerHost(string baseAddress, IScheduler scheduler, IEnumerable<ApiKeyProfile> profiles)
        : this(baseAddress, scheduler, new ApiKeyOptions(profiles)) { }

    private SchedulerHost(string baseAddress, IScheduler scheduler, ApiKeyOptions options)
    {
        _baseAddress = baseAddress;
        _scheduler = scheduler;
        _apiKeyOptions = options;
    }
    #endregion

    #region Start
    /// <summary>Starts the Web API.</summary>
    public Task Start()
    {
        _owinApp = WebApp.Start(_baseAddress, app =>
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            // API key authentication
            config.MessageHandlers.Add(new ApiKeyMessageHandler(_apiKeyOptions));

            // Register scheduler via a minimal dependency resolver
            config.DependencyResolver = new SchedulerDependencyResolver(_scheduler);

            app.UseWebApi(config);
        });

        return Task.FromResult(0);
    }
    #endregion

    #region Stop
    /// <summary>Stops the Web API.</summary>
    public Task Stop()
    {
        Dispose();
        return Task.FromResult(0);
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        _owinApp?.Dispose();
        _owinApp = null;
    }
    #endregion
}