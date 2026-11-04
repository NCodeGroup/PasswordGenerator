# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Public API tracking with `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`; the public surface of v1.0.1 is recorded as shipped.
- `CONTRIBUTING.md`, `SECURITY.md`, decision records in `docs/decisions/`, and a Definition of Done script, `build/dod.ps1`.
- A publish workflow for NuGet trusted publishing and a weekly template-update workflow.

### Changed

- Updated the project from project-template v0.41.1 to v0.61.2: shared build settings and analyzers, CI running the Definition of Done, Dependabot cooldown and security groups, and the NCode Review workflow.

[Unreleased]: https://github.com/NCodeGroup/PasswordGenerator/compare/v1.0.1...HEAD
