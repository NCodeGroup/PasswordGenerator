// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.Options;

namespace NCode.PasswordGenerator;

/// <summary>
/// Validates <see cref="PasswordGeneratorOptions"/> and reports every rule that makes generating a password fail.
/// </summary>
internal class PasswordGeneratorOptionsValidator : IValidateOptions<PasswordGeneratorOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, PasswordGeneratorOptions options)
    {
        var errors = options.GetConfigurationErrors();
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
