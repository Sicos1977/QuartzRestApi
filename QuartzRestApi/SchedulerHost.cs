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
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace QuartzRestApi;

/// <summary>
///     Acts as a host for the Web API.
/// </summary>
public class SchedulerHost
{
    #region Fields
    private readonly string _baseAddress;
    private readonly IScheduler _scheduler;
    private readonly ILogger _logger;
    private readonly ApiKeyOptions _apiKeyOptions;
    private IWebHost _webHost;
    #endregion

    #region Constructors
    /// <summary>
    ///     Creates a host without authentication (all endpoints publicly accessible).
    /// </summary>
    /// <param name="baseAddress">The base address, e.g. <c>http://localhost:45000</c>.</param>
    /// <param name="scheduler"><see cref="IScheduler"/></param>
    /// <param name="logger">Optional <see cref="ILogger"/>.</param>
    public SchedulerHost(string baseAddress, IScheduler scheduler, ILogger logger)
        : this(baseAddress, scheduler, logger, (IEnumerable<ApiKeyProfile>)null)
    { }

    /// <summary>
    ///     Creates a host with a single API key that grants full access to all endpoints.
    ///     Backwards-compatible with the previous single-key API.
    /// </summary>
    /// <param name="baseAddress">The base address, e.g. <c>http://localhost:45000</c>.</param>
    /// <param name="scheduler"><see cref="IScheduler"/></param>
    /// <param name="logger">Optional <see cref="ILogger"/>.</param>
    /// <param name="apiKey">
    ///     A single API key with unrestricted access.
    ///     Pass <see langword="null"/> or an empty string to disable authentication.
    /// </param>
    public SchedulerHost(string baseAddress, IScheduler scheduler, ILogger logger, string apiKey)
        : this(baseAddress, scheduler, logger,
              string.IsNullOrWhiteSpace(apiKey)
                  ? null
                  : [new ApiKeyProfile("Default", apiKey)])
    { }

    /// <summary>
    ///     Creates a host with multiple named API key profiles.
    ///     Each profile can optionally restrict which endpoints its bearer may call.
    /// </summary>
    /// <param name="baseAddress">The base address, e.g. <c>http://localhost:45000</c>.</param>
    /// <param name="scheduler"><see cref="IScheduler"/></param>
    /// <param name="logger">Optional <see cref="ILogger"/>.</param>
    /// <param name="profiles">
    ///     One or more <see cref="ApiKeyProfile"/> instances.
    ///     Pass <see langword="null"/> or an empty collection to disable authentication.
    /// </param>
    public SchedulerHost(string baseAddress, IScheduler scheduler, ILogger logger,
        IEnumerable<ApiKeyProfile> profiles)
    {
        _baseAddress = baseAddress;
        _scheduler = scheduler;
        _logger = logger;
        _apiKeyOptions = new ApiKeyOptions(profiles);
    }
    #endregion

    #region Start
    /// <summary>
    ///     Starts the Web API.
    /// </summary>
    public void Start()
    {
        var builder = new WebHostBuilder()
            .UseKestrel()
            .ConfigureServices(services =>
            {
                services.AddSingleton(_scheduler);
                services.AddSingleton(_apiKeyOptions);
            })
            .UseStartup<Startup>()
            .UseUrls(_baseAddress)
            .ConfigureLogging(logging => logging.AddConsole());

        if (_logger != null)
            builder.ConfigureServices(services => services.AddSingleton(_logger));

        _webHost = builder.Build();
        _webHost.Start();
    }
    #endregion

    #region Stop
    /// <summary>
    ///     Stops the Web API.
    /// </summary>
    public void Stop()
    {
        _webHost?.StopAsync();
    }
    #endregion
}