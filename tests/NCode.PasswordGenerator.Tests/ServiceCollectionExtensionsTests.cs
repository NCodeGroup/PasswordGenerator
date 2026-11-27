// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NCode.PasswordGenerator.Tests;

public class ServiceCollectionExtensionsTests : BaseTests
{
    private const string MaxAttemptsMessage = "MaxAttempts must be at least 1, but is 0.";

    [Fact]
    public async Task AddPasswordGenerator_GivenDefaultOptions_ThenGeneratesValidPassword()
    {
        await using var provider = new ServiceCollection()
            .AddPasswordGenerator()
            .BuildServiceProvider();
        var generator = provider.GetRequiredService<IPasswordGenerator>();

        var password = generator.Generate(new PasswordGeneratorOptions());

        Assert.True(new PasswordGeneratorOptions().IsValid(password));
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

        Assert.Throws<OptionsValidationException>(() => accessor.Value);
    }

    [Fact]
    public async Task AddPasswordGenerator_CalledTwice_ThenRegistersValidatorOnce()
    {
        await using var provider = new ServiceCollection()
            .AddPasswordGenerator()
            .AddPasswordGenerator()
            .BuildServiceProvider();

        Assert.Single(provider.GetServices<IValidateOptions<PasswordGeneratorOptions>>());
    }
}
