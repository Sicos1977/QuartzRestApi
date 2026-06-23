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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Quartz;
using QuartzRestApi.Security;
using Scalar.AspNetCore;

namespace QuartzRestApi;

/// <summary>
///     Hosts a self-contained REST API for managing and monitoring a Quartz.NET scheduler, providing endpoints for
///     scheduling operations and status queries. Supports optional API key authentication and structured logging
///     integration.
/// </summary>
/// <remarks>
///     Use SchedulerHost to expose a Quartz.NET IScheduler instance over HTTP via a Web API. The host can be
///     configured with or without authentication, and supports both single and multiple API key profiles for access
///     control. Logging can be integrated by providing an ILogger instance. The API is self-hosted using ASP.NET Core and
///     Kestrel, and includes OpenAPI (Swagger) documentation for client discovery. Thread safety and lifecycle management
///     are handled internally; callers should use Start and Stop to control the API's availability.
/// </remarks>
public class SchedulerHost
{
    #region Fields
    /// <summary>
    ///    The base address to listen on, e.g. <c>http://localhost:45000</c>.
    /// </summary>
    private readonly string _baseAddress;

    /// <summary>
    ///    The Quartz.NET scheduler instance whose state will be exposed via the Web API.
    /// </summary>
    private readonly IScheduler _scheduler;

    /// <summary>
    ///     Optional logger.  If provided, it will be injected into the API controllers and
    ///     used for logging within the host.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    ///     Holds the configuration options for API key security.
    /// </summary>
    private readonly Security.ApiKeyOptions _apiKeyOptions;

    /// <summary>
    ///     Represents the current instance of the web application being configured or run.
    /// </summary>
    private WebApplication _app;
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
                  : [ApiKeyProfile.AllowAll("Default", apiKey)])
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
    public SchedulerHost(string baseAddress, IScheduler scheduler, ILogger logger, IEnumerable<ApiKeyProfile> profiles)
    {
        _baseAddress = baseAddress;
        _scheduler = scheduler;
        _logger = logger;
        _apiKeyOptions = new Security.ApiKeyOptions(profiles);
    }
    #endregion

    #region Start
    /// <summary>
    ///     Starts the Web API.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task Start(CancellationToken cancellationToken = default)
    {
        var builder = WebApplication.CreateBuilder();
        var uri = new Uri(_baseAddress);
        var listenUrl = $"{uri.Scheme}://{uri.Authority}";

        builder.WebHost.UseKestrel().UseUrls(listenUrl);
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(_scheduler);
        builder.Services.AddSingleton(_apiKeyOptions);
        builder.Services.AddRouting();
        builder.Services.AddControllers().AddApplicationPart(typeof(SchedulerHost).Assembly);
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "QuartzRestApi",
                    Version = "v1",
                    Description = "A self-hosted REST API for Quartz.NET schedulers, built on .NET 10 with ASP.NET Core / Kestrel.",
                    Contact = new OpenApiContact { Name = "Kees van Spelde", Email = "sicos2002@hotmail.com" },
                    License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
                };
                return Task.CompletedTask;
            });
        });

        if (_logger != null)
            builder.Services.AddSingleton(_logger);

        _app = builder.Build();

        _app.UseMiddleware<ApiKeyMiddleware>();
        _app.UseRouting();
        _app.MapControllers();
        _app.MapOpenApi();
        _app.MapScalarApiReference();

        await _app.StartAsync(cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region Stop
    /// <summary>
    ///     Stops the Web API.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task Stop(CancellationToken cancellationToken = default)
    {
        if (_app != null)
            await _app.StopAsync(cancellationToken).ConfigureAwait(false);
    }
    #endregion
}