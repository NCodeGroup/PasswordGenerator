// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.Options;

namespace NCode.PasswordGenerator.Tests;

public class PasswordGeneratorOptionsValidatorTests : BaseTests
{
    private static ValidateOptionsResult Validate(PasswordGeneratorOptions options) =>
        new PasswordGeneratorOptionsValidator().Validate(null, options);

    #region Validate Tests

    [Fact]
    public void Validate_GivenDefaultOptions_ThenSucceeds()
    {
        var options = new PasswordGeneratorOptions();

        var result = Validate(options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_WhenAllMinimumsZero_ThenFails()
    {
        const string expected = "At least one character set must have a minimum greater than zero.";
        var options = new PasswordGeneratorOptions
        {
            MinLowercaseCharacters = 0,
            MinUppercaseCharacters = 0,
            MinNumericCharacters = 0,
            MinSpecialCharacters = 0,
        };

        var result = Validate(options);

        Assert.True(result.Failed);
        Assert.NotNull(result.Failures);
        Assert.Contains(expected, result.Failures);
    }

    [Fact]
    public void Validate_WhenMinLengthZero_ThenFails()
    {
        const string expected = "MinLength must be at least 1, but is 0.";
        var options = new PasswordGeneratorOptions().SetLengthRange(0, 64);

        var result = Validate(options);

        Assert.True(result.Failed);
        Assert.NotNull(result.Failures);
        Assert.Contains(expected, result.Failures);
    }

    [Fact]
    public void Validate_WhenMinimumsExceedMaxLength_ThenFails()
    {
        const string expected = "The minimum character counts add up to 5, more than MaxLength (4).";
        var options = new PasswordGeneratorOptions().SetLengthRange(4, 4);
        options.MinNumericCharacters = 2;

        var result = Validate(options);

        Assert.True(result.Failed);
        Assert.NotNull(result.Failures);
        Assert.Contains(expected, result.Failures);
    }

    [Fact]
    public void Validate_WhenMaxAttemptsZero_ThenFails()
    {
        const string expected = "MaxAttempts must be at least 1, but is 0.";
        var options = new PasswordGeneratorOptions { MaxAttempts = 0 };

        var result = Validate(options);

        Assert.True(result.Failed);
        Assert.NotNull(result.Failures);
        Assert.Contains(expected, result.Failures);
    }

    [Fact]
    public void Validate_WhenMaxConsecutiveIdenticalCharactersZero_ThenFails()
    {
        const string expected = "MaxConsecutiveIdenticalCharacters must be at least 1, but is 0.";
        var options = new PasswordGeneratorOptions { MaxConsecutiveIdenticalCharacters = 0 };

        var result = Validate(options);

        Assert.True(result.Failed);
        Assert.NotNull(result.Failures);
        Assert.Contains(expected, result.Failures);
    }

    [Fact]
    public void Validate_WhenNumericFullyExcluded_ThenFails()
    {
        const string expected = "MinNumericCharacters is 1, but no numeric character is left to choose from.";
        var options = new PasswordGeneratorOptions().SetExcludedCharacters("0123456789");

        var result = Validate(options);

        Assert.True(result.Failed);
        Assert.NotNull(result.Failures);
        Assert.Contains(expected, result.Failures);
    }

    [Fact]
    public void Validate_WhenSeveralRulesBroken_ThenReportsEach()
    {
        const string maxAttemptsMessage = "MaxAttempts must be at least 1, but is 0.";
        const string maxConsecutiveMessage = "MaxConsecutiveIdenticalCharacters must be at least 1, but is 0.";
        var options = new PasswordGeneratorOptions
        {
            MaxAttempts = 0,
            MaxConsecutiveIdenticalCharacters = 0,
        };

        var result = Validate(options);

        Assert.True(result.Failed);
        Assert.NotNull(result.Failures);
        var failureCount = result.Failures.Count();
        Assert.Equal(2, failureCount);
        Assert.Contains(maxAttemptsMessage, result.Failures);
        Assert.Contains(maxConsecutiveMessage, result.Failures);
    }

    #endregion
}
