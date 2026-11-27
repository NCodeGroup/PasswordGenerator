// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

using System.Buffers;
using System.Globalization;
using System.Text;

namespace NCode.PasswordGenerator;

/// <summary>
/// Contains the various rules and options for password generation.
/// </summary>
public class PasswordGeneratorOptions
{
    private const int DefaultMaxEnumerations = 100;
    private const int DefaultMaxAttempts = 10000;
    private const int DefaultMinLength = 16;
    private const int DefaultMaxLength = 64;
    private const int DefaultMaxConsecutiveIdenticalCharacters = 2;

    private const int DefaultMinSpecialCharacters = 1;
    private const int DefaultMinNumericCharacters = 1;
    private const int DefaultMinLowercaseCharacters = 1;
    private const int DefaultMinUppercaseCharacters = 1;

    internal const string LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz";
    internal const string UppercaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    internal const string NumericCharacters = "0123456789";
    private const string DefaultSpecialCharacters = "!#$%&()*+,-./:;<=>?@[]^_`{|}~";

    /// <summary>
    /// The look-alike characters of the default character sets, to pass to <see cref="SetExcludedCharacters"/>.
    /// </summary>
    public const string AmbiguousCharacters = "0O1Il|";

    private int _minLength = DefaultMinLength;
    private int _maxLength = DefaultMaxLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordGeneratorOptions"/> class.
    /// </summary>
    public PasswordGeneratorOptions()
    {
        Reset();
    }

    private PasswordGeneratorOptions(PasswordGeneratorOptions other)
    {
        _minLength = other._minLength;
        _maxLength = other._maxLength;
        MaxEnumerations = other.MaxEnumerations;
        MaxAttempts = other.MaxAttempts;
        MaxConsecutiveIdenticalCharacters = other.MaxConsecutiveIdenticalCharacters;
        MinLowercaseCharacters = other.MinLowercaseCharacters;
        MinUppercaseCharacters = other.MinUppercaseCharacters;
        MinNumericCharacters = other.MinNumericCharacters;
        MinSpecialCharacters = other.MinSpecialCharacters;
        SpecialOrNull = other.SpecialOrNull;
        ExcludedOrNull = other.ExcludedOrNull;
    }

    /// <summary>
    /// Resets all the properties to their default values.
    /// </summary>
    public PasswordGeneratorOptions Reset()
    {
        _minLength = DefaultMinLength;
        _maxLength = DefaultMaxLength;
        MaxEnumerations = DefaultMaxEnumerations;
        MaxAttempts = DefaultMaxAttempts;
        MaxConsecutiveIdenticalCharacters = DefaultMaxConsecutiveIdenticalCharacters;
        MinLowercaseCharacters = DefaultMinLowercaseCharacters;
        MinUppercaseCharacters = DefaultMinUppercaseCharacters;
        MinNumericCharacters = DefaultMinNumericCharacters;
        MinSpecialCharacters = DefaultMinSpecialCharacters;
        SpecialOrNull = null;
        ExcludedOrNull = null;
        return this;
    }

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public PasswordGeneratorOptions Clone() => new(this);

    /// <summary>
    /// Gets or sets the maximum number of passwords to generate when using <see cref="IEnumerable{T}"/>.
    /// </summary>
    public int MaxEnumerations { get; set; }

    /// <summary>
    /// Sets the maximum number of passwords to generate when using <see cref="IEnumerable{T}"/>.
    /// </summary>
    public PasswordGeneratorOptions SetMaxEnumerations(int value)
    {
        MaxEnumerations = value;
        return this;
    }

    /// <summary>
    /// Gets or sets the maximum number of iterations to perform when attempting to generate a valid password.
    /// </summary>
    public int MaxAttempts { get; set; }

    /// <summary>
    /// Sets the maximum number of iterations to perform when attempting to generate a valid password.
    /// </summary>
    public PasswordGeneratorOptions SetMaxAttempts(int value)
    {
        MaxAttempts = value;
        return this;
    }

    /// <summary>
    /// Gets or sets the minimum length for generated passwords.
    /// </summary>
    public int MinLength
    {
        get => _minLength;
        set
        {
            if (value > MaxLength)
                throw new ArgumentOutOfRangeException(nameof(value), value, "MinLength cannot be greater than MaxLength.");

            _minLength = value;
        }
    }

    /// <summary>
    /// Sets the minimum length for generated passwords.
    /// </summary>
    public PasswordGeneratorOptions SetMinLength(int value)
    {
        MinLength = value;
        return this;
    }

    /// <summary>
    /// Gets or sets the maximum length for generated passwords.
    /// </summary>
    public int MaxLength
    {
        get => _maxLength;
        set
        {
            if (value < MinLength)
                throw new ArgumentOutOfRangeException(nameof(value), value, "MaxLength cannot be less than MinLength.");

            _maxLength = value;
        }
    }

    /// <summary>
    /// Sets the maximum length for generated passwords.
    /// </summary>
    public PasswordGeneratorOptions SetMaxLength(int value)
    {
        MaxLength = value;
        return this;
    }

    /// <summary>
    /// Sets the exact length for generated passwords.
    /// </summary>
    public int ExactLength
    {
        set => SetLengthRange(value, value);
    }

    /// <summary>
    /// Sets the exact length for generated passwords.
    /// </summary>
    public PasswordGeneratorOptions SetExactLength(int value)
    {
        ExactLength = value;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="MinLength"/> and <see cref="MaxLength"/> properties at the same time.
    /// </summary>
    /// <param name="minLength">The minimum length for generated passwords.</param>
    /// <param name="maxLength">The maximum length for generated passwords.</param>
    public PasswordGeneratorOptions SetLengthRange(int minLength, int maxLength)
    {
        if (minLength > maxLength)
            throw new InvalidOperationException();

        _minLength = minLength;
        _maxLength = maxLength;

        return this;
    }

    /// <summary>
    /// Gets or sets the maximum number of consecutive identical characters allowed in the generated password.
    /// </summary>
    public int MaxConsecutiveIdenticalCharacters { get; set; }

    /// <summary>
    /// Sets the maximum number of consecutive identical characters allowed in the generated password.
    /// </summary>
    public PasswordGeneratorOptions SetMaxConsecutiveIdenticalCharacters(int value)
    {
        MaxConsecutiveIdenticalCharacters = value;
        return this;
    }

    /// <summary>
    /// Gets or sets the minimum amount of lower-case characters that are to be included in the generated password.
    /// </summary>
    public int MinLowercaseCharacters { get; set; }

    /// <summary>
    /// Sets the minimum amount of lower-case characters that are to be included in the generated password.
    /// </summary>
    public PasswordGeneratorOptions SetMinLowercaseCharacters(int value)
    {
        MinLowercaseCharacters = value;
        return this;
    }

    /// <summary>
    /// Gets or sets the minimum amount of upper-case characters that are to be included in the generated password.
    /// </summary>
    public int MinUppercaseCharacters { get; set; }

    /// <summary>
    /// Sets the minimum amount of upper-case characters that are to be included in the generated password.
    /// </summary>
    public PasswordGeneratorOptions SetMinUppercaseCharacters(int value)
    {
        MinUppercaseCharacters = value;
        return this;
    }

    /// <summary>
    /// Gets or sets the minimum amount of numeric characters that are to be included in the generated password.
    /// </summary>
    public int MinNumericCharacters { get; set; }

    /// <summary>
    /// Sets the minimum amount of numeric characters that are to be included in the generated password.
    /// </summary>
    public PasswordGeneratorOptions SetMinNumericCharacters(int value)
    {
        MinNumericCharacters = value;
        return this;
    }

    /// <summary>
    /// Gets or sets the minimum amount of special characters that are to be included in the generated password.
    /// </summary>
    public int MinSpecialCharacters { get; set; }

    /// <summary>
    /// Sets the minimum amount of special characters that are to be included in the generated password.
    /// </summary>
    public PasswordGeneratorOptions SetMinSpecialCharacters(int value)
    {
        MinSpecialCharacters = value;
        return this;
    }

    /// <summary>
    /// Gets the sum of <see cref="MinLowercaseCharacters"/>, <see cref="MinUppercaseCharacters"/>,
    /// <see cref="MinNumericCharacters"/> and <see cref="MinSpecialCharacters"/>.
    /// </summary>
    internal int MinimumCharacterCount =>
        MinSpecialCharacters + MinNumericCharacters + MinLowercaseCharacters + MinUppercaseCharacters;

    private string? SpecialOrNull { get; set; }

    /// <summary>
    /// Gets or sets the special-character search values last built, or <see langword="null"/> before the first use.
    /// </summary>
    private SpecialCharacterSearch? SpecialSearchOrNull { get; set; }

    /// <summary>
    /// Gets the search values for the current <see cref="SpecialCharacters"/>, built again when that string differs
    /// from the one the cached values were built from.
    /// </summary>
    private SearchValues<char> SpecialSearchValues
    {
        get
        {
            // Keyed by the source string rather than reset by the setter, so a read that races a write can't keep stale values.
            var special = SpecialCharacters;
            var search = SpecialSearchOrNull;
            if (search is not null && ReferenceEquals(search.Characters, special))
                return search.Values;

            var values = SearchValues.Create(special);
            SpecialSearchOrNull = new SpecialCharacterSearch(special, values);
            return values;
        }
    }

    /// <summary>
    /// Gets or sets the list of special characters such as punctuations and symbols that are to be included in the generated password.
    /// </summary>
    public string SpecialCharacters
    {
        get => SpecialOrNull ?? DefaultSpecialCharacters;
        set => SpecialOrNull = value;
    }

    /// <summary>
    /// Sets the list of special characters such as punctuations and symbols that are to be included in the generated password.
    /// </summary>
    public PasswordGeneratorOptions SetSpecialCharacters(string value)
    {
        SpecialCharacters = value;
        return this;
    }

    private string? ExcludedOrNull { get; set; }

    /// <summary>
    /// Gets or sets the characters that are left out of every character class and never appear in a generated password.
    /// </summary>
    public string ExcludedCharacters
    {
        get => ExcludedOrNull ?? string.Empty;
        set => ExcludedOrNull = value;
    }

    /// <summary>
    /// Sets the characters that are left out of every character class and never appear in a generated password.
    /// </summary>
    public PasswordGeneratorOptions SetExcludedCharacters(string value)
    {
        ExcludedCharacters = value;
        return this;
    }

    private string RemoveExcluded(string characters)
    {
        var excluded = ExcludedCharacters;
        if (excluded.Length == 0)
            return characters;

        var builder = new StringBuilder(characters.Length);
        foreach (var c in characters)
        {
            if (!excluded.Contains(c))
                builder.Append(c);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the lower-case characters that are left after removing <see cref="ExcludedCharacters"/>.
    /// </summary>
    internal string AllowedLowercaseCharacters => RemoveExcluded(LowercaseCharacters);

    /// <summary>
    /// Gets the upper-case characters that are left after removing <see cref="ExcludedCharacters"/>.
    /// </summary>
    internal string AllowedUppercaseCharacters => RemoveExcluded(UppercaseCharacters);

    /// <summary>
    /// Gets the numeric characters that are left after removing <see cref="ExcludedCharacters"/>.
    /// </summary>
    internal string AllowedNumericCharacters => RemoveExcluded(NumericCharacters);

    /// <summary>
    /// Gets the special characters that are left after removing <see cref="ExcludedCharacters"/>.
    /// </summary>
    internal string AllowedSpecialCharacters => RemoveExcluded(SpecialCharacters);

    /// <summary>
    /// Gets a <see cref="string"/> containing the allowable characters in a password, without the
    /// <see cref="ExcludedCharacters"/>.
    /// </summary>
    public string CharacterSet
    {
        get
        {
            var lowercase = MinLowercaseCharacters > 0 ? AllowedLowercaseCharacters : string.Empty;
            var uppercase = MinUppercaseCharacters > 0 ? AllowedUppercaseCharacters : string.Empty;
            var numeric = MinNumericCharacters > 0 ? AllowedNumericCharacters : string.Empty;
            var special = MinSpecialCharacters > 0 ? AllowedSpecialCharacters : string.Empty;
            return string.Concat(lowercase, uppercase, numeric, special);
        }
    }

    /// <summary>
    /// Estimates the entropy, in bits, of a password of <paramref name="length"/> characters generated with these options,
    /// as the length times the base-2 logarithm of the number of distinct characters in <see cref="CharacterSet"/>.
    /// </summary>
    /// <remarks>
    /// The estimate is an upper bound, because the minimum counts and the limit on consecutive identical characters
    /// narrow the passwords that can be generated.
    /// </remarks>
    /// <param name="length">The number of characters in the password.</param>
    /// <returns>The estimated entropy in bits, or <c>0</c> when <see cref="CharacterSet"/> is empty.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public double EstimateEntropyBits(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        var count = new HashSet<char>(CharacterSet).Count;
        return count == 0 ? 0 : length * Math.Log2(count);
    }

    internal bool AreIdentical(int i, ReadOnlySpan<char> password)
    {
        var maxConsecutive = MaxConsecutiveIdenticalCharacters;
        return i >= maxConsecutive && AreIdentical(password.Slice(i - maxConsecutive, maxConsecutive + 1));
    }

    private static bool AreIdentical(ReadOnlySpan<char> slice)
    {
        var prev = slice[0];

        foreach (var c in slice[1..])
        {
            if (c != prev) return false;
            prev = c;
        }

        return true;
    }

    /// <summary>
    /// Returns <c>true</c> or <c>false</c> whether the specified <paramref name="password"/> is valid according to configured
    /// rules on the current <see cref="PasswordGeneratorOptions"/> instance.
    /// A password that contains an excluded character (see <see cref="ExcludedCharacters"/>) is invalid.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <returns><c>true</c> is the password is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid(ReadOnlySpan<char> password)
    {
        if (password.Length < MinLength) return false;
        if (password.Length > MaxLength) return false;
        if (password.ContainsAny(ExcludedCharacters)) return false;

        var countSpecial = 0;
        var countNumeric = 0;
        var countLowercase = 0;
        var countUppercase = 0;
        var special = SpecialSearchValues;

        for (var i = 0; i < password.Length; ++i)
        {
            if (AreIdentical(i, password))
            {
                return false;
            }

            var c = password[i];

            // FYI, special may overlap with numeric and alpha
            if (special.Contains(c))
            {
                ++countSpecial;
            }

            if (char.IsNumber(c))
            {
                ++countNumeric;
            }
            else if (char.IsLower(c))
            {
                ++countLowercase;
            }
            else if (char.IsUpper(c))
            {
                ++countUppercase;
            }
        }

        return countSpecial >= MinSpecialCharacters &&
               countNumeric >= MinNumericCharacters &&
               countLowercase >= MinLowercaseCharacters &&
               countUppercase >= MinUppercaseCharacters;
    }

    /// <summary>
    /// Gets a message for each rule these options break that makes generating a password always or randomly fail.
    /// </summary>
    /// <returns>The messages in rule order, or an empty list when the options can generate a password.</returns>
    internal IReadOnlyList<string> GetConfigurationErrors()
    {
        var errors = new List<string>();
        var sum = MinimumCharacterCount;

        if (sum == 0)
            errors.Add("At least one character set must have a minimum greater than zero.");

        if (MinLength < 1)
            AddError(errors, $"MinLength must be at least 1, but is {MinLength}.");

        if (sum > MaxLength)
            AddError(errors, $"The minimum character counts add up to {sum}, more than MaxLength ({MaxLength}).");

        if (MaxAttempts < 1)
            AddError(errors, $"MaxAttempts must be at least 1, but is {MaxAttempts}.");

        if (MaxConsecutiveIdenticalCharacters < 1)
            AddError(errors, $"MaxConsecutiveIdenticalCharacters must be at least 1, but is {MaxConsecutiveIdenticalCharacters}.");

        AddEmptyClassError(errors, nameof(MinLowercaseCharacters), MinLowercaseCharacters, AllowedLowercaseCharacters, "lowercase");
        AddEmptyClassError(errors, nameof(MinUppercaseCharacters), MinUppercaseCharacters, AllowedUppercaseCharacters, "uppercase");
        AddEmptyClassError(errors, nameof(MinNumericCharacters), MinNumericCharacters, AllowedNumericCharacters, "numeric");
        AddEmptyClassError(errors, nameof(MinSpecialCharacters), MinSpecialCharacters, AllowedSpecialCharacters, "special");

        return errors;
    }

    private static void AddEmptyClassError(List<string> errors, string name, int minimum, string allowed, string kind)
    {
        if (minimum > 0 && allowed.Length == 0)
            AddError(errors, $"{name} is {minimum}, but no {kind} character is left to choose from.");
    }

    /// <summary>
    /// Adds a message to the list, formatted with the invariant culture.
    /// </summary>
    /// <param name="errors">The list to add the message to.</param>
    /// <param name="message">The message to format and add.</param>
    private static void AddError(List<string> errors, FormattableString message)
    {
        var text = message.ToString(CultureInfo.InvariantCulture);
        errors.Add(text);
    }
}
