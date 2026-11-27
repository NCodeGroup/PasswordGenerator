# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Public API tracking with `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`; the public API of v1.0.1 is recorded as shipped and is unchanged.
- Every pack validates the package against the last published version, 1.0.1, so a change that breaks its public API fails the build.
- The package declares itself trim- and Native AOT-compatible (`IsAotCompatible`), so the build reports any change that would break a trimmed or AOT-compiled app. ([#24](https://github.com/NCodeGroup/PasswordGenerator/issues/24))
- `IPasswordGenerator.GeneratePinned` returns a password in a buffer on the pinned object heap, which the garbage collector never copies, and zeroes it when disposed, so an application can clear a password after use; the overloads take the default options, a callback that adjusts them, or options passed in. An `IPasswordGenerator` implemented outside the library throws `NotSupportedException` from them until it overrides them. ([#37](https://github.com/NCodeGroup/PasswordGenerator/issues/37), [ADR-1001](docs/decisions/1001-pinned-password-buffers-are-default-interface-members.md))
- `PasswordGeneratorOptions.ExcludedCharacters` and `SetExcludedCharacters` keep the characters they list out of every character class, so no generated password contains them, and `IsValid` rejects a password that holds one; `PasswordGeneratorOptions.AmbiguousCharacters` is `0O1Il|`, the look-alike characters to pass to it. Generating with a minimum above zero for a class whose characters are all excluded throws `ArgumentException`.
- `PasswordGeneratorOptions.EstimateEntropyBits(int length)` estimates the strength, in bits, of a password of that length generated with the options: the length times the base-2 logarithm of the number of distinct characters in `CharacterSet`, an upper bound, since the minimum counts and the run limit narrow the choices.

### Changed

- `AssemblyVersion` moves only with the major version: 2.0.0 and every later 2.x release carry `2.0.0.0`, so a minor or patch upgrade is a drop-in replacement; `FileVersion` and `InformationalVersion` keep the exact version. Releases up to 1.0.1 had an `AssemblyVersion` equal to the package version. ([#24](https://github.com/NCodeGroup/PasswordGenerator/issues/24))
- Updated the project to project-template v0.61.2: stricter analyzers and banned APIs, a Definition of Done build script, and CI that runs it.
- Moved the library project to `src/NCode.PasswordGenerator/` and the test project to `tests/NCode.PasswordGenerator.Tests/`, the layout the project template uses; the package and its public API are unchanged.
- Updated the project to project-template v0.114.1: the version of every assembly and package comes from `version.json` through Nerdbank.GitVersioning, so a release's tag must name the version `version.json` holds, and the release workflow fails when it doesn't; NCode Review approves a clean pull request, and pull requests open as drafts; the project gains a code of conduct, governance, roadmap, architecture and assurance case documents.
- Passwords are filled and shuffled with `RandomNumberGenerator.GetItems` and `RandomNumberGenerator.Shuffle`, and `PasswordGeneratorOptions.IsValid` looks up special characters with `SearchValues<char>`; the rules a generated password meets are unchanged. ([#24](https://github.com/NCodeGroup/PasswordGenerator/issues/24))
- **Breaking:** Generating a password with `MinSpecialCharacters` above zero and an empty `SpecialCharacters` throws `ArgumentException` instead of its subclass `ArgumentOutOfRangeException`; code that catches `ArgumentOutOfRangeException` for this case must catch `ArgumentException` instead. ([#24](https://github.com/NCodeGroup/PasswordGenerator/issues/24))
- Packages are published to nuget.org by `.github/workflows/publish.yml` through nuget.org trusted publishing, from the protected `nuget` environment and with no long-lived API key, and each release carries build provenance and an SBOM; `.github/workflows/raise-baseline.yml` raises the package validation baseline after each release. ([#11](https://github.com/NCodeGroup/PasswordGenerator/issues/11), [#32](https://github.com/NCodeGroup/PasswordGenerator/issues/32), [#40](https://github.com/NCodeGroup/PasswordGenerator/issues/40))
- Each release is tagged, published as a GitHub release and pushed to nuget.org as its pull request merges into `main`: `AGENTS.md`'s Landing section sets `release: tag+release`, so `.github/workflows/tag-release.yml` tags the version of the changelog's newest section and starts `.github/workflows/publish.yml`.
- Every C# file starts with the two-line SPDX header, `// Copyright © NCode Group` and `// SPDX-License-Identifier: Apache-2.0`, and the Rider settings generate it. ([#38](https://github.com/NCodeGroup/PasswordGenerator/issues/38))
- The solution is `NCode.PasswordGenerator.slnx`, the build settings the project template already makes are no longer repeated, and no file starts with a UTF-8 byte order mark; the package's contents, Source Link and symbols are unchanged. ([#30](https://github.com/NCodeGroup/PasswordGenerator/issues/30))
- **Breaking:** `AddPasswordGenerator` validates the options when the host starts, and when `IOptions<PasswordGeneratorOptions>` is first read, and throws `OptionsValidationException` naming each problem for options that can't generate a password: every minimum count at zero, `MinLength` below 1, minimum counts that add up to more than `MaxLength`, `MaxAttempts` or `MaxConsecutiveIdenticalCharacters` below 1, or a character class with a minimum above zero and no character left to choose from. Fix the options it names; such a host otherwise failed when it generated a password. ([ADR-1002](docs/decisions/1002-password-generator-options-are-validated-when-the-host-starts.md))

### Removed

- **Breaking:** Support for .NET Standard 2.1. The package targets .NET 10 (`net10.0`) only and is released as 2.0.0; apps on .NET 6–9, Unity, Mono or Xamarin stay on the 1.x packages. The public API is unchanged, so a .NET 10 app upgrades without code changes. ([#24](https://github.com/NCodeGroup/PasswordGenerator/issues/24), [ADR-1000](docs/decisions/1000-the-library-targets-the-newest-dotnet-lts-release.md))
- `.github/workflows/main.yml` and its `build/Test-PackageVersion.ps1` checks: `.github/workflows/ci.yml` builds and tests every change, and the README's badge shows it. ([#32](https://github.com/NCodeGroup/PasswordGenerator/issues/32), [#11](https://github.com/NCodeGroup/PasswordGenerator/issues/11))

### Fixed

- The NuGet package has its description and tags, which were set in a file the build never read. ([#22](https://github.com/NCodeGroup/PasswordGenerator/issues/22))
- The `Span<char>` overloads of `Generate` clear the caller's buffer before they throw `InvalidOperationException` ("Too many attempts."), so the last rejected attempt isn't left in it. ([#36](https://github.com/NCodeGroup/PasswordGenerator/issues/36))
- The documentation of `IPasswordGenerator` says enumerating it yields `MaxEnumerations` passwords, 100 by default, instead of an infinite sequence. ([#29](https://github.com/NCodeGroup/PasswordGenerator/issues/29))
- **Breaking:** The `Generate` overloads that return a `string` throw `InvalidOperationException` ("The requested password length is too small.") when the length drawn from the options is 0 or less, instead of returning an empty string that was never validated (a length of 0) or throwing `ArgumentOutOfRangeException` (a negative length); `GeneratePinned` does the same. Keep `MinLength` at 1 or more, or catch `InvalidOperationException`. ([#28](https://github.com/NCodeGroup/PasswordGenerator/issues/28))
- The default special characters hold `;` once instead of twice, so `;` is no more likely than any other special character; the default set of characters is unchanged.

## [1.0.1] - 2023-07-16

### Changed

- The README's first heading is "Overview" instead of "PasswordGenerator".

## [1.0.0] - 2023-07-16

### Added

- Initial release: `IPasswordGenerator`, its implementation `PasswordGenerator`, `PasswordGeneratorOptions` and the `AddPasswordGenerator` registration, for .NET Standard 2.1.

[Unreleased]: https://github.com/NCodeGroup/PasswordGenerator/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/NCodeGroup/PasswordGenerator/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/NCodeGroup/PasswordGenerator/releases/tag/v1.0.0
