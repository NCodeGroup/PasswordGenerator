// Copyright © NCode Group
// SPDX-License-Identifier: Apache-2.0

using System.Collections;

namespace NCode.PasswordGenerator.Tests;

public class PasswordGeneratorTests : BaseTests
{
    private PasswordGeneratorOptions DefaultOptions { get; } = new();
    private Mock<PasswordGenerator> MockPasswordGenerator { get; }
    private PasswordGenerator PasswordGenerator { get; }

    public PasswordGeneratorTests()
    {
        MockPasswordGenerator = CreatePartialMock<PasswordGenerator>(DefaultOptions);
        PasswordGenerator = MockPasswordGenerator.Object;
    }

    [Fact]
    public void Generate_GivenMinCharacters_Valid()
    {
        var options = new PasswordGeneratorOptions
        {
            MinSpecialCharacters = 3,
            MinNumericCharacters = 3,
            MinLowercaseCharacters = 3,
            MinUppercaseCharacters = 3
        };
        options.SetLengthRange(16, 32);
        var password = PasswordGenerator.Generate(options);
        var isValid = options.IsValid(password);
        Assert.True(isValid);
    }

    [Fact]
    public void Generate_Valid()
    {
        const string password = nameof(password);

        MockPasswordGenerator
            .Setup(_ => _.Generate(DefaultOptions))
            .Returns(password)
            .Verifiable();

        var result = PasswordGenerator.Generate();
        Assert.Equal(password, result);
    }

    [Fact]
    public void Generate_GivenSpan_WhenTooSmall_ThenThrows()
    {
        DefaultOptions.SetLengthRange(5, 7);

        PasswordGenerator.Generate(DefaultOptions, new char[6]);

        var password = new char[2];
        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordGenerator.Generate(DefaultOptions, password));

        Assert.Equal("The requested password length is too small.", exception.Message);
    }

    [Fact]
    public void Generate_GivenSpan_WhenTooLarge_ThenThrows()
    {
        DefaultOptions.SetLengthRange(5, 7);

        PasswordGenerator.Generate(DefaultOptions, new char[6]);

        var password = new char[9];
        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordGenerator.Generate(DefaultOptions, password));

        Assert.Equal("The requested password length is too large.", exception.Message);
    }

    [Fact]
    public void Generate_GivenSpan_EmptyCharacterSet_ThenThrows()
    {
        var options = new PasswordGeneratorOptions
        {
            MinSpecialCharacters = 0,
            MinNumericCharacters = 0,
            MinLowercaseCharacters = 0,
            MinUppercaseCharacters = 0
        };

        var password = new char[options.MinLength];
        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordGenerator.Generate(options, password));

        Assert.Equal("At least one character set must have a minimum greater than zero.", exception.Message);
    }

    [Fact]
    public void Generate_GivenSpan_LargeCharacterSet_ThenThrows()
    {
        var options = new PasswordGeneratorOptions
        {
            MinSpecialCharacters = 10,
            MinNumericCharacters = 10,
            MinLowercaseCharacters = 10,
            MinUppercaseCharacters = 10
        };

        options.SetExactLength(10);

        var password = new char[options.MinLength];
        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordGenerator.Generate(options, password));

        Assert.Equal("The provided destination buffer is too small to hold the minimum amount of required characters.", exception.Message);
    }

    [Fact]
    public void Generate_GivenOptions_ThenValid()
    {
        const string password = nameof(password);

        DefaultOptions.ExactLength = password.Length;

        var characterSet = DefaultOptions.CharacterSet;

        MockPasswordGenerator
            .Setup(_ => _.FisherYatesShuffle(characterSet))
            .Returns(characterSet)
            .Verifiable();

        var skipResult = password;
        MockPasswordGenerator
            .Setup(_ => _.SkipGenerateCore(out skipResult))
            .Returns(true)
            .Verifiable();

        bool? isValidResult = true;
        MockPasswordGenerator
            .Setup(_ => _.SkipIsValid(out isValidResult))
            .Returns(true)
            .Verifiable();

        var result = PasswordGenerator.Generate(DefaultOptions);
        Assert.Equal(password, result);
    }

    [Fact]
    public void Generate_GivenOptions_WhenTooManyAttempts_ThenThrows()
    {
        const string password = nameof(password);

        DefaultOptions.MaxAttempts = 10;

        var characterSet = DefaultOptions.CharacterSet;

        MockPasswordGenerator
            .Setup(_ => _.FisherYatesShuffle(characterSet))
            .Returns(characterSet)
            .Verifiable();

        var passwordResult = password;
        MockPasswordGenerator
            .Setup(_ => _.SkipGenerateCore(out passwordResult))
            .Returns(true)
            .Verifiable();

        bool? isValidResult = false;
        MockPasswordGenerator
            .Setup(_ => _.SkipIsValid(out isValidResult))
            .Returns(true)
            .Verifiable();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordGenerator.Generate(DefaultOptions)
        );

        Assert.Equal("Too many attempts.", exception.Message);
    }

    [Fact]
    public void Generate_GivenEmptySpecialCharactersAndNoMinSpecial_ThenValid()
    {
        var options = new PasswordGeneratorOptions
        {
            MinSpecialCharacters = 0,
            SpecialCharacters = string.Empty,
        };

        var password = PasswordGenerator.Generate(options);

        var isValid = options.IsValid(password);
        Assert.True(isValid);
    }

    [Fact]
    public void Generate_GivenEmptySpecialCharactersAndMinSpecial_ThenThrowsArgumentException()
    {
        var options = new PasswordGeneratorOptions
        {
            MinSpecialCharacters = 1,
            SpecialCharacters = string.Empty,
        };

        var exception = Assert.Throws<ArgumentException>(() =>
            PasswordGenerator.Generate(options));

        Assert.Equal("choices", exception.ParamName);
    }

    [Fact]
    public void Generate_WhenAmbiguousCharactersExcluded_ThenEveryAttemptOmitsThem()
    {
        var options = new PasswordGeneratorOptions
        {
            MaxAttempts = 1,
            MaxConsecutiveIdenticalCharacters = 64,
        };

        options.SetExcludedCharacters(PasswordGeneratorOptions.AmbiguousCharacters);

        for (var i = 0; i < 100; ++i)
        {
            var password = PasswordGenerator.Default.Generate(options);

            var ambiguous = password.Intersect(PasswordGeneratorOptions.AmbiguousCharacters);

            Assert.Empty(ambiguous);
        }
    }

    [Fact]
    public void Generate_WhenRequiredClassFullyExcluded_ThenThrowsArgumentException()
    {
        var options = new PasswordGeneratorOptions
        {
            MaxAttempts = 1,
        };

        options.SetExcludedCharacters("abcdefghijklmnopqrstuvwxyz");

        var exception = Assert.Throws<ArgumentException>(() =>
            PasswordGenerator.Default.Generate(options));

        Assert.Equal("choices", exception.ParamName);
    }

    [Fact]
    public void Generate_GivenNegativeMinimumAndUnreachableMinimum_ThenTooManyAttempts()
    {
        var options = new PasswordGeneratorOptions
        {
            MaxAttempts = 1,
            MinSpecialCharacters = -5,
            MinNumericCharacters = 20,
            MinLowercaseCharacters = 0,
            MinUppercaseCharacters = 0,
        };

        options.SetExactLength(16);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordGenerator.Generate(options));

        Assert.Equal("Too many attempts.", exception.Message);
    }

    [Fact]
    public void Generate_GivenExactLengthZero_ThenThrows()
    {
        DefaultOptions.SetExactLength(0);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordGenerator.Generate(DefaultOptions));

        Assert.Equal("The requested password length is too small.", exception.Message);
    }

    [Fact]
    public void Generate_GivenRangeFromZero_WhenZeroDrawn_ThenThrows()
    {
        DefaultOptions.SetLengthRange(0, 10);

        MockPasswordGenerator
            .Setup(x => x.GetRandomInt32(0, 11))
            .Returns(0)
            .Verifiable();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordGenerator.Generate(DefaultOptions));

        Assert.Equal("The requested password length is too small.", exception.Message);
    }

    [Fact]
    public void Generate_GivenNegativeExactLength_ThenThrows()
    {
        DefaultOptions.SetExactLength(-1);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordGenerator.Generate(DefaultOptions));

        Assert.Equal("The requested password length is too small.", exception.Message);
    }

    [Fact]
    public void Generate_GivenSpan_WhenTooManyAttempts_ThenCleared()
    {
        DefaultOptions.MaxAttempts = 3;
        DefaultOptions.SetExactLength(8);

        bool? isValidResult = false;
        MockPasswordGenerator
            .Setup(x => x.SkipIsValid(out isValidResult))
            .Returns(true)
            .Verifiable();

        var password = new char[8];
        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordGenerator.Generate(DefaultOptions, password));

        Assert.Equal("Too many attempts.", exception.Message);
        Assert.All(password, c => Assert.Equal('\0', c));
    }

    [Fact]
    public void GetLength_GivenEqualMinMax_ThenSame()
    {
        const int length = 7;

        var options = new PasswordGeneratorOptions
        {
            MinLength = length,
            MaxLength = length
        };

        var result = PasswordGenerator.GetLength(options);
        Assert.Equal(length, result);
    }

    [Fact]
    public void GetLength_GivenNonEqualMinMax_ThenRandom()
    {
        const int min = 3;
        const int max = 7;
        const int length = 5;

        var options = new PasswordGeneratorOptions
        {
            MinLength = min,
            MaxLength = max
        };

        MockPasswordGenerator
            .Setup(_ => _.GetRandomInt32(min, max + 1))
            .Returns(length)
            .Verifiable();

        var result = PasswordGenerator.GetLength(options);
        Assert.Equal(length, result);
    }

    [Fact]
    public void Enumerate_Valid()
    {
        const int count = 5;
        var passwords = PasswordGenerator.Take(count).ToHashSet();
        Assert.Equal(count, passwords.Count);
    }

    [Fact]
    public void Enumerate_GivenMax_ThenValid()
    {
        const int count = 10;
        DefaultOptions.MaxEnumerations = count;
        var passwords = PasswordGenerator.ToList();
        Assert.Equal(count, passwords.Count);
    }

    #region GeneratePinned Tests

    [Fact]
    public void GeneratePinned_Valid()
    {
        using var password = PasswordGenerator.GeneratePinned();

        var isValid = DefaultOptions.IsValid(password.Memory.Span);
        Assert.True(isValid);
    }

    [Fact]
    public void GeneratePinned_GivenConfigurator_ThenConfiguredLength()
    {
        using var password = PasswordGenerator.GeneratePinned(options => options.SetExactLength(20));

        Assert.Equal(20, password.Memory.Length);
    }

    [Fact]
    public void GeneratePinned_GivenOptions_ThenValid()
    {
        DefaultOptions.SetLengthRange(16, 32);

        using var password = PasswordGenerator.GeneratePinned(DefaultOptions);

        Assert.InRange(password.Memory.Length, 16, 32);

        var isValid = DefaultOptions.IsValid(password.Memory.Span);
        Assert.True(isValid);
    }

    [Fact]
    public void GeneratePinned_GivenExactLengthZero_ThenThrows()
    {
        DefaultOptions.SetExactLength(0);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordGenerator.GeneratePinned(DefaultOptions));

        Assert.Equal("The requested password length is too small.", exception.Message);
    }

    [Fact]
    public void GeneratePinned_GivenOptions_WhenTooManyAttempts_ThenDisposesBuffer()
    {
        DefaultOptions.SetExactLength(16);
        DefaultOptions.MaxAttempts = 10;

        var buffer = new PinnedPasswordBuffer(16);
        MockPasswordGenerator
            .Setup(x => x.CreatePinnedBuffer(16))
            .Returns(buffer)
            .Verifiable();

        bool? isValidResult = false;
        MockPasswordGenerator
            .Setup(x => x.SkipIsValid(out isValidResult))
            .Returns(true)
            .Verifiable();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            PasswordGenerator.GeneratePinned(DefaultOptions));

        Assert.Equal("Too many attempts.", exception.Message);
        Assert.Throws<ObjectDisposedException>(() => buffer.Memory);
    }

    [Fact]
    public void GeneratePinned_GivenImplementationWithoutOverride_ThenThrows()
    {
        IPasswordGenerator generator = new LegacyPasswordGenerator();

        Assert.Throws<NotSupportedException>(() => generator.GeneratePinned());
        Assert.Throws<NotSupportedException>(() => generator.GeneratePinned(_ => { }));
        Assert.Throws<NotSupportedException>(() => generator.GeneratePinned(DefaultOptions));
    }

    #endregion

    private sealed class LegacyPasswordGenerator : IPasswordGenerator
    {
        public string Generate() => throw new NotImplementedException();

        public string Generate(Action<PasswordGeneratorOptions> optionsConfigurator) => throw new NotImplementedException();

        public string Generate(PasswordGeneratorOptions options) => throw new NotImplementedException();

        public void Generate(Span<char> password) => throw new NotImplementedException();

        public void Generate(Action<PasswordGeneratorOptions> optionsConfigurator, Span<char> password) => throw new NotImplementedException();

        public void Generate(PasswordGeneratorOptions options, Span<char> password) => throw new NotImplementedException();

        public IEnumerator<string> GetEnumerator() => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
