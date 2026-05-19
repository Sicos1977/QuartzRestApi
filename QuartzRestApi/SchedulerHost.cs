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