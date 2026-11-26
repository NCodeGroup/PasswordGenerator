# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Public API tracking with `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`; the public API of v1.0.1 is recorded as shipped and is unchanged.
- Every pack validates the package against the last published version, 1.0.1, so a change that breaks its public API fails the build.

### Changed

- `AssemblyVersion` no longer changes with each release: every later 1.x release keeps `1.0.1.0`, the version 1.0.1 shipped with, so a minor or patch upgrade is a drop-in replacement; `FileVersion` and `InformationalVersion` keep the exact version. Releases up to 1.0.1 had an `AssemblyVersion` equal to the package version.
- Updated the project to project-template v0.61.2: stricter analyzers and banned APIs, a Definition of Done build script, and CI that runs it.
- Moved the library project to `src/NCode.PasswordGenerator/` and the test project to `tests/NCode.PasswordGenerator.Tests/`, the layout the project template uses; the package and its public API are unchanged.
- Updated the project to project-template v0.114.1: the version of every assembly and package comes from `version.json` through Nerdbank.GitVersioning, so a release's tag must name the version `version.json` holds, and the release workflow fails when it doesn't; NCode Review approves a clean pull request, and pull requests open as drafts; the project gains a code of conduct, governance, roadmap, architecture and assurance case documents.

[Unreleased]: https://github.com/NCodeGroup/PasswordGenerator/compare/v1.0.1...HEAD
