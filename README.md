[DI]: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
[.NET Standard 2.1]: https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1

![ci](https://github.com/NCodeGroup/PasswordGenerator/actions/workflows/main.yml/badge.svg)

# Overview
This [DI] friendly library provides the ability to generate strong cryptographically random passwords that meet the OWASP requirements (length, lower case, upper case, numeric, and special characters).

# Requirements
- [.NET Standard 2.1] - for Span API, which does NOT allow usage from .NET Framework, sorry

# Usage
```csharp
void ConfigureServices(IServiceCollection services)
{
    services.AddPasswordGenerator();

    // or configure options...
    services.AddPasswordGenerator(options => options
        // values shown here are the defaults
        .SetMaxEnumerations(100)
        .SetMaxAttempts(10000)
        .SetLengthRange(16, 64)
        .SetMaxConsecutiveIdenticalCharacters(2)
        .SetMinLowercaseCharacters(1)
        .SetMinUppercaseCharacters(1)
        .SetMinNumericCharacters(1)
        .SetMinSpecialCharacters(1)
        .SetSpecialCharacters("!;#$%&()*+,-./:;<=>?@[]^_`{|}~")
    );

    // or configure from appsettings...
    services.Configure<PasswordGeneratorOptions>(
        configuration.GetSection("PasswordGenerator"));
}

void SimpleUsage(IPasswordGenerator generator)
{
    var password = generator.Generate();
}

void UsageWithSpan(IPasswordGenerator generator)
{
    Span<char> password = stackalloc char[16];
    generator.Generate(password);
}

void UsageWithCustomOptions(IPasswordGenerator generator)
{
    var options = new PasswordGeneratorOptions
    {
        // ...
    };
    var password = generator.Generate(options);
}

void UsageWithOptionsConfigurator(IPasswordGenerator generator)
{
    var password = generator.Generate(options =>
    {
        // ...
    });
}

void UsageWithEnumerable(IPasswordGenerator generator)
{
    var passwords = generator.Take(5);
}

void UsageWithoutDI()
{
    // all other variants (span, options, enumerable, etc) are available
    var password = PasswordGenerator.Default.Generate();
}
```

## Project

- [CONTRIBUTING.md](CONTRIBUTING.md): how to set up, build, test and propose a change.
- [SECURITY.md](SECURITY.md): how to report a vulnerability privately.
- [docs/decisions/](docs/decisions/README.md): the decisions behind the design, the project's and the template's.
- [CHANGELOG.md](CHANGELOG.md): what changed in each release, in the Keep a Changelog format.
- [LICENSE.txt](LICENSE.txt): the license.

## License

NCode.PasswordGenerator is licensed under the [Apache License 2.0](LICENSE.txt).
