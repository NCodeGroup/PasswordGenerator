// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

namespace NCode.PasswordGenerator;

/// <summary>
/// The characters that one call to generate a password draws from, worked out once for all its attempts.
/// </summary>
internal readonly record struct CharacterPools
{
    /// <summary>
    /// Gets the special characters that are left after the excluded characters are removed.
    /// </summary>
    public required string Special { get; init; }

    /// <summary>
    /// Gets the numeric characters that are left after the excluded characters are removed.
    /// </summary>
    public required string Numeric { get; init; }

    /// <summary>
    /// Gets the lower-case characters that are left after the excluded characters are removed.
    /// </summary>
    public required string Lowercase { get; init; }

    /// <summary>
    /// Gets the upper-case characters that are left after the excluded characters are removed.
    /// </summary>
    public required string Uppercase { get; init; }

    /// <summary>
    /// Gets the shuffled character set that fills the rest of the password.
    /// </summary>
    public required string All { get; init; }
}
