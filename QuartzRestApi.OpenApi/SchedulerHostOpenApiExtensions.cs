//
// SchedulerHostOpenApiExtensions.cs
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace QuartzRestApi.OpenApi;

/// <summary>
///     Extension methods for adding OpenAPI (Swagger) and Scalar API documentation to a QuartzRestApi host.
/// </summary>
/// <remarks>
///     This package is optional. Add a reference to <c>QuartzRestApi.OpenApi</c> and call these extension methods
///     only if you want to expose interactive OpenAPI documentation for your scheduler REST API.
///     Without this package, the scheduler host remains fully functional but does not generate OpenAPI documents.
/// </remarks>
public static class SchedulerHostOpenApiExtensions
{
    #region AddQuartzOpenApi
    /// <summary>
    ///     Adds OpenAPI document generation services to the service collection for QuartzRestApi.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> for fluent chaining.</returns>
    /// <remarks>
    ///     Call this method before building the <see cref="WebApplication"/> to register OpenAPI document generation.
    ///     The document will include metadata for all registered QuartzRestApi controllers.
    ///     By default, the document is titled "QuartzRestApi" with version "v1", MIT license, and contact information
    ///     for Kees van Spelde.
    /// </remarks>
    public static IServiceCollection AddQuartzOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
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

        return services;
    }
    #endregion

    #region MapQuartzOpenApi
    /// <summary>
    ///     Maps OpenAPI endpoints to the application pipeline and enables the Scalar interactive API reference UI.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The configured <see cref="WebApplication"/> for fluent chaining.</returns>
    /// <remarks>
    ///     Call this method after building the <see cref="WebApplication"/> to expose the OpenAPI JSON document
    ///     and the Scalar UI for browsing and testing the QuartzRestApi endpoints.
    ///     The OpenAPI document is typically available at <c>/openapi/v1.json</c>, and the Scalar UI at <c>/scalar/v1</c>.
    ///     Ensure <see cref="AddQuartzOpenApi"/> was called during service registration.
    /// </remarks>
    public static WebApplication MapQuartzOpenApi(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        return app;
    }
    #endregion
}