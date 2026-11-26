# 1000. The library targets the newest .NET LTS release only

- **Status:** Accepted
- **Date:** 2026-11-26
- **Deciders:** NCode Group maintainers

## Context

NCode.PasswordGenerator generates passwords into a `Span<char>` and checks them against character-class rules. .NET
releases add the span and cryptography APIs that do this work directly, such as `RandomNumberGenerator.GetItems`,
`RandomNumberGenerator.Shuffle` and `SearchValues<char>`, which .NET Standard lacks. A library that also targets .NET
Standard or an older runtime can use them only behind conditional compilation, with a second implementation for the
older target.

Microsoft ships a major .NET release every November. A Long Term Support (LTS) release gets three years of support and
a Standard Term Support (STS) release two ([.NET support policy](https://dotnet.microsoft.com/platform/support/policy/dotnet-core)).
A library built for an LTS release runs on every later release, LTS or STS. The consumers a .NET Standard target still
reaches are Unity, Mono, Xamarin and runtimes that are out of support.

## Decision

The library targets one framework, the newest .NET LTS release, and declares `IsAotCompatible`. Moving to a newer LTS
release drops the previous target, which breaks consumers still on it, so each move ships as a major version, and the
packages of the previous major version stay on nuget.org for those consumers. A move changes no public API by itself,
so an app on the new target upgrades without code changes.

The library's project file, `src/NCode.PasswordGenerator/NCode.PasswordGenerator.csproj`, names the current target,
and [README.md](../../README.md#requirements) states the runtime it needs.

## Options considered

- **The newest LTS release only (chosen).** Every API that release has is usable without conditional compilation, and
  the package is tested on the one runtime it ships for. Consumers on older runtimes keep the previous major version.
- **The newest LTS release and .NET Standard.** Keeps each version installable on older runtimes, but every use of a
  newer API needs an `#if` branch with a second implementation, tested on a runtime CI doesn't run.
- **The newest release, LTS or STS.** Reaches each API a year sooner, but forces a major version every year and cuts
  off apps that stay on the supported LTS release.

## Consequences

- The library calls its target's span and cryptography APIs directly, and later work can build on `Memory<char>` and
  pinned buffers.
- A consumer on an older runtime stays on the previous major version; the README says which.
- Moving to the next LTS release is a planned migration (`/plan-migration`) and a major version, which can wait until
  the release the library targets nears its end of support.
- Each move records the dropped target framework as an intended package-validation break in
  `CompatibilitySuppressions.xml`, which stays until the validation baseline reaches that major version.

## References

- [#24](https://github.com/NCodeGroup/PasswordGenerator/issues/24): the first move under this decision.
- [ADR-0131](project-template/0131-public-api-is-tracked-and-evolves-compatibly.md): the public API is tracked.
- [ADR-0136](project-template/0136-packages-follow-semantic-versioning-and-the-assembly-version-moves-only-with-the-major.md): a breaking change is a major version.
- [ADR-0147](project-template/0147-packages-are-validated-against-the-last-published-package.md): an intended break in a major version is recorded in `CompatibilitySuppressions.xml`.
