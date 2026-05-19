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
using Microsoft.Extensions.Logging;
using Quartz;

namespace QuartzRestApi;

/// <summary>
///     Acts as a host for the Web API
/// </summary>
/// <remarks>
///    Create a new instance of the <see cref="SchedulerHost"/> class
/// </remarks>
/// <param name="baseAddress">The base address, e.g. http://localhost:45000</param>
/// <param name="scheduler"><see cref="IScheduler"/></param>
/// <param name="logger">><see cref="ILogger"/></param>
public class SchedulerHost(string baseAddress, IScheduler scheduler, ILogger logger)
{
    #region Fields
#if NET48
    private IDisposable _webApp;
#endif
#if NET6_0
    private IWebHost _webHost;
#endif    
    #endregion

    #region Start
    /// <summary>
    ///     Start the Web API
    /// </summary>
    public void Start()
    {
#if NET48
        Startup.Scheduler = scheduler;
        Startup.Logger = logger;
        _webApp = WebApp.Start<Startup>(baseAddress);
#endif

#if NET6_0
        var builder = new WebHostBuilder()
            .UseKestrel()
            .ConfigureServices(services => services.AddSingleton(scheduler))
            .UseStartup<Startup>()
            .UseUrls(baseAddress)
            .ConfigureLogging(logging  => logging.AddConsole());

        if (logger != null)
            builder.ConfigureServices(services => services.AddSingleton(logger));

        _webHost = builder.Build();
        _webHost.Start();
#endif
    }
    #endregion

    #region Stop
    /// <summary>
    ///     Stops the Web API
    /// </summary>
    public void Stop()
    {
#if NET48
        _webApp?.Dispose();
#endif
#if NET6_0
       _webHost?.StopAsync();
#endif
    }
    #endregion
}