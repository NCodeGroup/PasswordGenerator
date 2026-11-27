# 1003. Options reject an out-of-range value with ArgumentOutOfRangeException

- **Status:** Accepted
- **Date:** 2026-11-26
- **Deciders:** NCode Group maintainers

## Context

`PasswordGeneratorOptions` rejects a length that contradicts the other bound in two places. The `MinLength` and
`MaxLength` setters throw `ArgumentOutOfRangeException` naming the parameter, with a message. `SetLengthRange(minLength,
maxLength)` threw an `InvalidOperationException` with no message for `minLength` greater than `maxLength`, so a caller
saw neither which argument was wrong nor why. The exception type is part of the public behavior: a caller that catches
one type misses another.

## Decision

A `PasswordGeneratorOptions` member that rejects a value passed to it throws `ArgumentOutOfRangeException` whose
`ParamName` is the parameter it rejects, whose `ActualValue` is that value, and whose message says what the value must
satisfy. `SetLengthRange` throws it for `minLength`, with the message "minLength cannot be greater than maxLength.", and
leaves the options unchanged. `InvalidOperationException` stays the exception of `Generate`, for options that can't
produce a password when it runs.

## Options considered

- **`ArgumentOutOfRangeException` naming the parameter (chosen).** It matches the setters and `EstimateEntropyBits`, and
  its `ParamName` and `ActualValue` give callers a machine-readable reason.
- **`InvalidOperationException` with a message.** It explains the error but keeps a type that signals a bad object state,
  not a bad argument, and differs from the setters.
- **`ArgumentException`.** It names the parameter but is less specific than the setters' type for a value outside its
  allowed range.

## Consequences

- Code that catches `InvalidOperationException` from `SetLengthRange` catches `ArgumentOutOfRangeException` instead, a
  breaking change released in 2.0.0.
- A new option member that rejects a value follows the same rule.

## References

- [PasswordGeneratorOptions.cs](../../src/NCode.PasswordGenerator/PasswordGeneratorOptions.cs): `SetLengthRange` and the `MinLength` and `MaxLength` setters.
- [#46](https://github.com/NCodeGroup/PasswordGenerator/issues/46): give `SetLengthRange` a proper exception.
- [ADR-1002](1002-password-generator-options-are-validated-when-the-host-starts.md): the options' rules checked when the host starts.
