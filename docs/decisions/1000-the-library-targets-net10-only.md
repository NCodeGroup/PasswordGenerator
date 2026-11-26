# 1000. The library targets .NET 10 only

- **Status:** Accepted
- **Date:** 2026-11-26
- **Deciders:** NCode Group maintainers

## Context

NCode.PasswordGenerator generates passwords into a `Span<char>` and checks them against character-class rules. .NET Standard 2.1 lacks the APIs that do this work directly: `RandomNumberGenerator.GetItems` and `RandomNumberGenerator.Shuffle` (.NET 8), `SearchValues<char>` (.NET 8) and the pinned object heap (.NET 5). On .NET Standard 2.1 the library draws one random index per character, keeps its own Fisher–Yates shuffle and scans the special-character string once per character.

.NET 8 and .NET 9 reached end of support on 2026-11-10. .NET 10 is the supported LTS release until 2028-11-14, and .NET 11, an STS release, runs .NET 10 libraries ([.NET support policy](https://dotnet.microsoft.com/platform/support/policy/dotnet-core), read 2026-11-26). The consumers .NET Standard 2.1 still reaches are Unity, Mono, Xamarin and the unsupported .NET Core 3.x and .NET 5–9 runtimes.

## Decision

The library targets `net10.0` only and declares `IsAotCompatible`. Dropping `netstandard2.1` breaks consumers on older runtimes, so it ships as major version 2.0.0, and the 1.x packages stay on nuget.org for those consumers. The public API stays as `PublicAPI.Shipped.txt` records it, so a .NET 10 app upgrades without code changes.

## Options considered

- **`net10.0` only (chosen).** Every .NET 10 API is usable without conditional compilation, and the package is tested on the runtime it ships for. Consumers on older runtimes keep 1.x.
- **`net10.0` and `netstandard2.1`.** Keeps 2.x installable on older runtimes, but every use of a .NET 8+ API needs an `#if` branch with a second implementation, tested on a runtime CI doesn't run, which defeats the purpose.
- **`net11.0`.** An STS release; it would cut off .NET 10 LTS apps for no API this library needs.

## Consequences

- The library calls `RandomNumberGenerator.GetItems`, `RandomNumberGenerator.Shuffle` and `SearchValues<char>` directly, and later work can build on `Memory<char>` and pinned buffers.
- A consumer on a runtime older than .NET 10 stays on 1.x; the README says so.
- Moving to the next LTS release is a planned migration (`/plan-migration`), and a major version when it drops a runtime consumers still use.

## References

- [#24](https://github.com/NCodeGroup/PasswordGenerator/issues/24): the upgrade.
- [ADR-0131](project-template/0131-public-api-is-tracked-and-evolves-compatibly.md): the public API is tracked.
- [ADR-0136](project-template/0136-packages-follow-semantic-versioning-and-the-assembly-version-moves-only-with-the-major.md): a breaking change is a major version.
