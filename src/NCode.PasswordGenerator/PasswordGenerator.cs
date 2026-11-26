// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace NCode.PasswordGenerator;

/// <summary>
/// Provides the ability to generate random passwords.
/// </summary>
/// <remarks>
/// Be aware that the <see cref="IEnumerable{T}"/> implementation is an infinite sequence.
/// </remarks>
public interface IPasswordGenerator : IEnumerable<string>
{
    /// <summary>
    /// Generates a random password using the default options.
    /// </summary>
    /// <returns>The newly generated random password.</returns>
    string Generate();

    /// <summary>
    /// Generates a random password using the rules from the specified <paramref name="optionsConfigurator"/>.
    /// </summary>
    /// <param name="optionsConfigurator">A callback method that is invoked to configure the <see cref="PasswordGeneratorOptions"/>.</param>
    /// <returns>The newly generated random password.</returns>
    string Generate(Action<PasswordGeneratorOptions> optionsConfigurator);

    /// <summary>
    /// Generates a random password using the rules from the specified <paramref name="options"/>.
    /// </summary>
    /// <param name="options">The <see cref="PasswordGeneratorOptions"/> that defines the rules for generating passwords.</param>
    /// <returns>The newly generated random password.</returns>
    string Generate(PasswordGeneratorOptions options);

    /// <summary>
    /// Generates a random password using the default options.
    /// </summary>
    /// <param name="password">The destination buffer for the newly generated password.</param>
    void Generate(Span<char> password);

    /// <summary>
    /// Generates a random password using the rules from the specified <paramref name="optionsConfigurator"/>.
    /// </summary>
    /// <param name="optionsConfigurator">A callback method that is invoked to configure the <see cref="PasswordGeneratorOptions"/>.</param>
    /// <param name="password">The destination buffer for the newly generated password.</param>
    void Generate(Action<PasswordGeneratorOptions> optionsConfigurator, Span<char> password);

    /// <summary>
    /// Generates a random password using the rules from the specified <paramref name="options"/>.
    /// </summary>
    /// <param name="options">The <see cref="PasswordGeneratorOptions"/> that defines the rules for generating passwords.</param>
    /// <param name="password">The destination buffer for the newly generated password.</param>
    void Generate(PasswordGeneratorOptions options, Span<char> password);

    /// <summary>
    /// Generates a random password using the default options, into a pinned buffer that is zeroed when disposed.
    /// </summary>
    /// <returns>
    /// The owner of the buffer that holds the newly generated password. Dispose it as soon as the password is no
    /// longer needed; its <see cref="IMemoryOwner{T}.Memory"/> throws <see cref="ObjectDisposedException"/> afterwards.
    /// </returns>
    /// <exception cref="NotSupportedException">The implementation doesn't override this member.</exception>
    IMemoryOwner<char> GeneratePinned() =>
        throw new NotSupportedException(PinnedNotSupportedMessage);

    /// <summary>
    /// Generates a random password using the rules from the specified <paramref name="optionsConfigurator"/>, into a pinned buffer that is zeroed when disposed.
    /// </summary>
    /// <param name="optionsConfigurator">A callback method that is invoked to configure the <see cref="PasswordGeneratorOptions"/>.</param>
    /// <returns>
    /// The owner of the buffer that holds the newly generated password. Dispose it as soon as the password is no
    /// longer needed; its <see cref="IMemoryOwner{T}.Memory"/> throws <see cref="ObjectDisposedException"/> afterwards.
    /// </returns>
    /// <exception cref="NotSupportedException">The implementation doesn't override this member.</exception>
    IMemoryOwner<char> GeneratePinned(Action<PasswordGeneratorOptions> optionsConfigurator) =>
        throw new NotSupportedException(PinnedNotSupportedMessage);

    /// <summary>
    /// Generates a random password using the rules from the specified <paramref name="options"/>, into a pinned buffer that is zeroed when disposed.
    /// </summary>
    /// <param name="options">The <see cref="PasswordGeneratorOptions"/> that defines the rules for generating passwords.</param>
    /// <returns>
    /// The owner of the buffer that holds the newly generated password. Dispose it as soon as the password is no
    /// longer needed; its <see cref="IMemoryOwner{T}.Memory"/> throws <see cref="ObjectDisposedException"/> afterwards.
    /// </returns>
    /// <exception cref="NotSupportedException">The implementation doesn't override this member.</exception>
    IMemoryOwner<char> GeneratePinned(PasswordGeneratorOptions options) =>
        throw new NotSupportedException(PinnedNotSupportedMessage);

    private const string PinnedNotSupportedMessage =
        "This IPasswordGenerator implementation doesn't generate passwords into pinned buffers.";
}

/// <summary>
/// Provides a default implementation of the <see cref="IPasswordGenerator"/> interface.
/// </summary>
public class PasswordGenerator : IPasswordGenerator
{
    /// <summary>
    /// Gets a singleton instance for <see cref="IPasswordGenerator"/> that uses the default options.
    /// </summary>
    public static IPasswordGenerator Default { get; } = new PasswordGenerator();

    /// <summary>
    /// Fills the start of <paramref name="destination"/> with characters chosen at random from <paramref name="choices"/>.
    /// </summary>
    /// <param name="choices">The characters to choose from.</param>
    /// <param name="count">
    /// How many characters to fill: none when zero or less, and all of <paramref name="destination"/> when larger.
    /// </param>
    /// <param name="destination">The buffer to fill.</param>
    /// <returns>The part of <paramref name="destination"/> after the filled characters.</returns>
    private static Span<char> FillRandom(ReadOnlySpan<char> choices, int count, Span<char> destination)
    {
        var take = Math.Min(count, destination.Length);
        if (take <= 0)
            return destination;

        RandomNumberGenerator.GetItems(choices, destination[..take]);
        return destination[take..];
    }

    private PasswordGeneratorOptions DefaultOptions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordGenerator"/> class with default options.
    /// </summary>
    public PasswordGenerator()
        : this(new PasswordGeneratorOptions())
    {
        // nothing
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordGenerator"/> class with the specified options.
    /// </summary>
    /// <param name="options">The <see cref="PasswordGeneratorOptions"/> that defines the rules for generating passwords.</param>
    public PasswordGenerator(PasswordGeneratorOptions options)
    {
        DefaultOptions = options;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordGenerator"/> class with the specified options.
    /// </summary>
    /// <param name="optionsAccessor">An accessor for the <see cref="PasswordGeneratorOptions"/> that defines the rules for generating passwords.</param>
    public PasswordGenerator(IOptions<PasswordGeneratorOptions> optionsAccessor)
        : this(optionsAccessor.Value)
    {
        // nothing
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual int GetRandomInt32(int fromInclusive, int toExclusive) =>
        RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);

    private PasswordGeneratorOptions Configure(Action<PasswordGeneratorOptions> optionsConfigurator)
    {
        var options = DefaultOptions.Clone();
        optionsConfigurator(options);
        return options;
    }

    /// <inheritdoc />
    public string Generate() =>
        Generate(DefaultOptions);

    /// <inheritdoc />
    public string Generate(Action<PasswordGeneratorOptions> optionsConfigurator) =>
        Generate(Configure(optionsConfigurator));

    /// <inheritdoc />
    public virtual string Generate(PasswordGeneratorOptions options) =>
        string.Create(GetLength(options), options, (span, localOptions) => Generate(localOptions, span));

    /// <inheritdoc />
    public void Generate(Span<char> password) =>
        Generate(DefaultOptions, password);

    /// <inheritdoc />
    public void Generate(Action<PasswordGeneratorOptions> optionsConfigurator, Span<char> password) =>
        Generate(Configure(optionsConfigurator), password);

    /// <inheritdoc />
    public void Generate(PasswordGeneratorOptions options, Span<char> password)
    {
        if (password.Length < options.MinLength)
            throw new InvalidOperationException("The requested password length is too small.");

        if (password.Length > options.MaxLength)
            throw new InvalidOperationException("The requested password length is too large.");

        var minCharacters = options.MinSpecialCharacters +
                            options.MinNumericCharacters +
                            options.MinLowercaseCharacters +
                            options.MinUppercaseCharacters;

        if (minCharacters == 0)
            throw new InvalidOperationException("At least one character set must have a minimum greater than zero.");

        if (minCharacters > password.Length)
            throw new InvalidOperationException(
                "The provided destination buffer is too small to hold the minimum amount of required characters.");

        var maxAttempts = options.MaxAttempts;
        var characterSet = FisherYatesShuffle(options.CharacterSet);

        for (var attempt = 0; attempt < maxAttempts; ++attempt)
        {
            GenerateCore(options, characterSet, password);
            if (IsValid(options, password)) return;
        }

        throw new InvalidOperationException("Too many attempts.");
    }

    internal virtual string FisherYatesShuffle(string value) =>
        string.Create(value.Length, value, static (dst, src) =>
        {
            src.AsSpan().CopyTo(dst);
            RandomNumberGenerator.Shuffle(dst);
        });

    internal int GetLength(PasswordGeneratorOptions options) =>
        options.MinLength == options.MaxLength
            ? options.MinLength
            : GetRandomInt32(options.MinLength, options.MaxLength + 1);

    // for unit tests
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual bool SkipIsValid([NotNullWhen(true)] out bool? result)
    {
        result = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsValid(PasswordGeneratorOptions options, ReadOnlySpan<char> password) =>
        SkipIsValid(out var result) ? result.Value : options.IsValid(password);

    // for unit tests
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal virtual bool SkipGenerateCore([NotNullWhen(true)] out string? result)
    {
        result = null;
        return false;
    }

    private void GenerateCore(PasswordGeneratorOptions options, string characterSet, Span<char> password)
    {
        if (SkipGenerateCore(out var result))
        {
            result.AsSpan().CopyTo(password);
            return;
        }

        var remaining = password;
        remaining = FillRandom(options.SpecialCharacters, options.MinSpecialCharacters, remaining);
        remaining = FillRandom(PasswordGeneratorOptions.NumericCharacters, options.MinNumericCharacters, remaining);
        remaining = FillRandom(PasswordGeneratorOptions.LowercaseCharacters, options.MinLowercaseCharacters, remaining);
        remaining = FillRandom(PasswordGeneratorOptions.UppercaseCharacters, options.MinUppercaseCharacters, remaining);
        FillRandom(characterSet, remaining.Length, remaining);

        RandomNumberGenerator.Shuffle(password);
    }

    /// <inheritdoc />
    public IMemoryOwner<char> GeneratePinned() =>
        GeneratePinned(DefaultOptions);

    /// <inheritdoc />
    public IMemoryOwner<char> GeneratePinned(Action<PasswordGeneratorOptions> optionsConfigurator) =>
        GeneratePinned(Configure(optionsConfigurator));

    /// <inheritdoc />
    public IMemoryOwner<char> GeneratePinned(PasswordGeneratorOptions options)
    {
        var buffer = new PinnedPasswordBuffer(GetLength(options));
        try
        {
            Generate(options, buffer.Memory.Span);
            return buffer;
        }
        catch
        {
            buffer.Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public IEnumerator<string> GetEnumerator()
    {
        var count = DefaultOptions.MaxEnumerations;
        while (--count >= 0) yield return Generate();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}