# 1002. Password generator options are validated when the host starts

- **Status:** Accepted
- **Date:** 2026-11-27
- **Deciders:** NCode Group maintainers

## Context

Some options can't generate a password: every minimum count is zero, `MinLength` is below 1, the minimum counts add up
to more than `MaxLength`, `MaxAttempts` or `MaxConsecutiveIdenticalCharacters` is below 1, or a character class with a
minimum above zero has no character left to choose from. Without a check at startup, such a host fails only on the first
`Generate` call, possibly long after deployment. A `MinLength` below 1 is worse, because it fails at random, depending on
the length a call draws, so a test run can pass and production still fail.

`Generate` checks its options itself, and its exception types and messages are part of the public behavior. A rule that
the DI path enforces and the direct `Generate(options)` path lacks would make the same options valid in one place and
invalid in the other.

## Decision

`AddPasswordGenerator` registers an internal `IValidateOptions<PasswordGeneratorOptions>` and calls `ValidateOnStart`, so
a misconfigured host stops when it starts, and `IOptions<PasswordGeneratorOptions>.Value` throws on its first read when
nothing validated it before. The rules are one internal method on the options, `GetConfigurationErrors`, which the
validator calls.

The validator refuses only options with which `Generate` always fails or fails at random, so the DI path adds no rule the
direct path lacks. A negative individual minimum passes, because `Generate` accepts it. The validator reports every broken
rule in one `OptionsValidationException`, not only the first, so one run shows everything to fix.

The validator and the method stay internal to keep the public surface small, and `Generate` keeps its own checks for
options passed to it directly.

## Options considered

- **Validate on start (chosen).** The host fails at startup with every problem named, and the rules sit next to the
  options they describe.
- **Validate only in `Generate`.** The failure comes late, on the first call, and a `MinLength` below 1 shows up only on
  some calls.
- **A public `Validate()` method.** It adds public surface that no caller needs, since the registration already runs
  the rules and `Generate` runs its own.
- **Data annotations.** They can't express rules across fields, such as minimum counts against `MaxLength`, and reflection
  over attributes isn't trim-safe.

## Consequences

- A misconfigured host stops at start with `OptionsValidationException`, which lists each broken rule.
- `Generate` keeps its own checks and messages for options passed to it directly.
- A host on a plain `ServiceProvider`, which doesn't run startup validation, sees the failure on the first read of
  `IOptions<PasswordGeneratorOptions>.Value`.
- A new rule belongs in `GetConfigurationErrors` only when `Generate` always or randomly fails for the options it
  refuses.

## References

- [PasswordGeneratorOptionsValidator.cs](../../src/NCode.PasswordGenerator/PasswordGeneratorOptionsValidator.cs): the
  validator that reports the rules.
- [ADR-0131](project-template/0131-public-api-is-tracked-and-evolves-compatibly.md): the public API is tracked and
  evolves compatibly.
