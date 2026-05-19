//
// Startup.cs
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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Scalar.AspNetCore;

namespace QuartzRestApi;

/// <summary>
///     The startup class for Kestrel
/// </summary>
internal class Startup
{
    internal static IScheduler Scheduler;
    internal static ILogger Logger;

    /// <summary>
    ///     Configure the service
    /// </summary>
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.AddRouting();
        services.AddControllers();
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new()
                {
                    Title = "QuartzRestApi",
                    Version = "v1",
                    Description = "A self-hosted REST API for Quartz.NET schedulers, built on .NET 10 with ASP.NET Core / Kestrel.",
                    Contact = new() { Name = "Kees van Spelde", Email = "sicos2002@hotmail.com" },
                    License = new() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
                };
                return System.Threading.Tasks.Task.CompletedTask;
            });
        });
        return services.BuildServiceProvider();
    }

    /// <summary>
    ///     Configure the application
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();

        app.UseMiddleware<ApiKeyMiddleware>();
        app.UseRouting();

        // OpenAPI document endpoint: /openapi/v1.json
        // Scalar interactive API reference: /scalar/v1
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapOpenApi();
            endpoints.MapScalarApiReference();
        });
    }
}
