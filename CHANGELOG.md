# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Public API tracking with `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`; the public API of v1.0.1 is recorded as shipped and is unchanged.
- Every pack validates the package against the last published version, 1.0.1, so a change that breaks its public API fails the build.
- The package declares itself trim- and Native AOT-compatible (`IsAotCompatible`), so the build reports any change that would break a trimmed or AOT-compiled app. ([#24](https://github.com/NCodeGroup/PasswordGenerator/issues/24))

### Changed

- `AssemblyVersion` moves only with the major version: 2.0.0 and every later 2.x release carry `2.0.0.0`, so a minor or patch upgrade is a drop-in replacement; `FileVersion` and `InformationalVersion` keep the exact version. Releases up to 1.0.1 had an `AssemblyVersion` equal to the package version. ([#24](https://github.com/NCodeGroup/PasswordGenerator/issues/24))
- Updated the project to project-template v0.61.2: stricter analyzers and banned APIs, a Definition of Done build script, and CI that runs it.
- Moved the library project to `src/NCode.PasswordGenerator/` and the test project to `tests/NCode.PasswordGenerator.Tests/`, the layout the project template uses; the package and its public API are unchanged.
- Updated the project to project-template v0.114.1: the version of every assembly and package comes from `version.json` through Nerdbank.GitVersioning, so a release's tag must name the version `version.json` holds, and the release workflow fails when it doesn't; NCode Review approves a clean pull request, and pull requests open as drafts; the project gains a code of conduct, governance, roadmap, architecture and assurance case documents.
- Passwords are filled and shuffled with `RandomNumberGenerator.GetItems` and `RandomNumberGenerator.Shuffle`, and `PasswordGeneratorOptions.IsValid` looks up special characters with `SearchValues<char>`; the rules a generated password meets are unchanged. ([#24](https://github.com/NCodeGroup/PasswordGenerator/issues/24))
- Generating a password with `MinSpecialCharacters` above zero and an empty `SpecialCharacters` throws `ArgumentException` instead of its subclass `ArgumentOutOfRangeException`. ([#24](https://github.com/NCodeGroup/PasswordGenerator/issues/24))

### Removed

- **Breaking:** Support for .NET Standard 2.1. The package targets .NET 10 (`net10.0`) only and is released as 2.0.0; apps on .NET 6–9, Unity, Mono or Xamarin stay on the 1.x packages. The public API is unchanged, so a .NET 10 app upgrades without code changes. ([#24](https://github.com/NCodeGroup/PasswordGenerator/issues/24), [ADR-1000](docs/decisions/1000-the-library-targets-net10-only.md))

### Fixed

- The NuGet package has its description and tags, which were set in a file the build never read. ([#22](https://github.com/NCodeGroup/PasswordGenerator/issues/22))

[Unreleased]: https://github.com/NCodeGroup/PasswordGenerator/compare/v1.0.1...HEAD
