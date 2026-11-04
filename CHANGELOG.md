# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Public API tracking with `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`; the public API of v1.0.1 is recorded as shipped and is unchanged.

### Changed

- `AssemblyVersion` now moves only with the major version (`1.0.0.0` for every 1.x release), so a minor or patch upgrade is a drop-in replacement; `FileVersion` and `InformationalVersion` keep the exact version. Releases up to 1.0.1 had an `AssemblyVersion` equal to the package version.
- Updated the project to project-template v0.61.2: stricter analyzers and banned APIs, a Definition of Done build script, and CI that runs it.

[Unreleased]: https://github.com/NCodeGroup/PasswordGenerator/compare/v1.0.1...HEAD
