// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace NCode.PasswordGenerator;

/// <summary>
/// Provides extension methods for adding <see cref="IPasswordGenerator"/> to the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IPasswordGenerator"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// The options are validated when the host starts and when they are first read. Options that can't generate a
    /// password throw an <see cref="OptionsValidationException"/> that lists each problem.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddPasswordGenerator(this IServiceCollection services) =>
        AddPasswordGenerator(services, _ => { });

    /// <summary>
    /// Adds <see cref="IPasswordGenerator"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// The options are validated when the host starts and when they are first read. Options that can't generate a
    /// password throw an <see cref="OptionsValidationException"/> that lists each problem.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configureOptions">The action used to configure the <see cref="PasswordGeneratorOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddPasswordGenerator(this IServiceCollection services, Action<PasswordGeneratorOptions> configureOptions)
    {
        services.TryAdd(ServiceDescriptor.Singleton<IPasswordGenerator, PasswordGenerator>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<PasswordGeneratorOptions>, PasswordGeneratorOptionsValidator>());
        services.AddOptions<PasswordGeneratorOptions>().Configure(configureOptions).ValidateOnStart();
        return services;
    }
}
