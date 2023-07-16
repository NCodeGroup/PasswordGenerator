﻿#region Copyright Preamble
// 
//    Copyright @ 2023 NCode Group
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
#endregion

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NCode.PasswordGenerator;

/// <summary>
/// Provides extension methods for adding <see cref="IPasswordGenerator"/> to the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IPasswordGenerator"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddPasswordGenerator(this IServiceCollection services) =>
        AddPasswordGenerator(services, _ => { });

    /// <summary>
    /// Adds <see cref="IPasswordGenerator"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configureOptions">The action used to configure the <see cref="PasswordGeneratorOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddPasswordGenerator(this IServiceCollection services, Action<PasswordGeneratorOptions> configureOptions)
    {
        services.TryAdd(ServiceDescriptor.Singleton<IPasswordGenerator, PasswordGenerator>());
        services.Configure(configureOptions);
        return services;
    }
}
