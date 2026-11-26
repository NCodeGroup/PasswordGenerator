// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

using System.Buffers;

namespace NCode.PasswordGenerator;

/// <summary>
/// Holds the search values built from one string of special characters, together with that string.
/// </summary>
/// <param name="Characters">The special characters the search values were built from.</param>
/// <param name="Values">The search values for <paramref name="Characters"/>.</param>
internal sealed record SpecialCharacterSearch(string Characters, SearchValues<char> Values);
