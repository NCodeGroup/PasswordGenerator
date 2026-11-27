// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NCode.PasswordGenerator.Tests;

public class ServiceCollectionExtensionsTests : BaseTests
{
    private const string MaxAttemptsMessage = "MaxAttempts must be at least 1, but is 0.";

    #region AddPasswordGenerator Tests

    [Fact]
    public async Task AddPasswordGenerator_GivenConfiguredOptions_ThenGeneratorUsesThem()
    {
        await using var provider = new ServiceCollection()
            .AddPasswordGenerator(options => options.SetExactLength(20))
            .BuildServiceProvider();
        var generator = provider.GetRequiredService<IPasswordGenerator>();

        var password = generator.Generate();

        Assert.Equal(20, password.Length);
    }

    [Fact]
    public async Task AddPasswordGenerator_GivenDefaultOptions_ThenStartupValidationSucceeds()
    {
        await using var provider = new ServiceCollection()
            .AddPasswordGenerator()
            .BuildServiceProvider();
        var startupValidator = provider.GetRequiredService<IAsyncStartupValidator>();

        var exception = await Record.ExceptionAsync(() => startupValidator.ValidateAsync(CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task AddPasswordGenerator_WhenOptionsInvalid_ThenStartupValidationThrows()
    {
        await using var provider = new ServiceCollection()
            .AddPasswordGenerator(options => options.MaxAttempts = 0)
            .BuildServiceProvider();
        var startupValidator = provider.GetRequiredService<IAsyncStartupValidator>();

        var exception = await Assert.ThrowsAsync<OptionsValidationException>(
            () => startupValidator.ValidateAsync(CancellationToken.None));

        Assert.Contains(MaxAttemptsMessage, exception.Failures);
    }

    [Fact]
    public async Task AddPasswordGenerator_WhenOptionsInvalid_ThenReadingOptionsThrows()
    {
        await using var provider = new ServiceCollection()
            .AddPasswordGenerator(options => options.MaxAttempts = 0)
            .BuildServiceProvider();
        var accessor = provider.GetRequiredService<IOptions<PasswordGeneratorOptions>>();

        var exception = Assert.Throws<OptionsValidationException>(() => accessor.Value);

        Assert.Contains(MaxAttemptsMessage, exception.Failures);
    }

    [Fact]
    public async Task AddPasswordGenerator_CalledTwice_ThenRegistersValidatorOnce()
    {
        await using var provider = new ServiceCollection()
            .AddPasswordGenerator()
            .AddPasswordGenerator()
            .BuildServiceProvider();

        var validators = provider.GetServices<IValidateOptions<PasswordGeneratorOptions>>();
        Assert.Single(validators);
    }

    #endregion
}
