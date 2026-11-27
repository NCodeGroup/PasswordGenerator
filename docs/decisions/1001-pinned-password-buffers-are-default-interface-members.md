# 1001. Pinned password buffers are default interface members

- **Status:** Accepted
- **Date:** 2026-11-26
- **Deciders:** NCode Group maintainers

## Context

A `string` can't be cleared, and the garbage collector may copy it when it compacts the heap, so a password returned as
a `string` lingers in memory until the collector reuses the space. An application that wants to clear a password after
use needs it in a buffer it owns. The `Span<char>` overloads give the caller that buffer, but the caller must size it,
and the `string` overloads can't offer it.

[ADR-0112](project-template/0112-binary-data-uses-spans-pooled-buffers-and-cleared-secrets.md) says a long-lived secret
is held in a pinned array and cleared when released, and
[#37](https://github.com/NCodeGroup/PasswordGenerator/issues/37) asks for a password in such a buffer.

`IPasswordGenerator` is public and can be implemented outside the library. Adding an abstract member to it breaks those
implementers, and the public API evolves compatibly
([ADR-0131](project-template/0131-public-api-is-tracked-and-evolves-compatibly.md)).

## Decision

`IPasswordGenerator` has three `GeneratePinned` overloads that match the `string` overloads: one with no arguments, one
that takes an `Action<PasswordGeneratorOptions>` and one that takes a `PasswordGeneratorOptions`. Each returns an
`IMemoryOwner<char>` and is a default interface member that throws `NotSupportedException`, so an implementation
outside the library still compiles and fails closed instead of handing out a copy that can't be cleared.

`PasswordGenerator` overrides all three through the internal `PinnedPasswordBuffer`. The buffer allocates its own array
on each call with `GC.AllocateUninitializedArray<char>(length, pinned: true)`, so the collector never moves it, and
zeroes the array with `CryptographicOperations.ZeroMemory` on `Dispose`. A second `Dispose` does nothing, and `Memory`
throws `ObjectDisposedException` once the buffer is disposed. Leaving the array uninitialized is safe because `Generate`
overwrites every character before it returns, and `GeneratePinned` disposes, and so zeroes, the buffer when generation
fails.

The span overloads get no pinned counterpart, because a span's caller already owns its buffer.

## Options considered

- **Default interface members (chosen).** The change only adds members, so every implementer outside the library still
  compiles, and the members stay on the interface that callers already hold, with the generator's default options.
  An implementation that doesn't override them throws instead of returning a buffer that can't be cleared.
- **Abstract members.** Each implementer outside the library stops compiling, which makes a compatible addition a
  breaking one.
- **Extension methods.** They can't reach the generator's default options, and they can only build on the public
  overloads, which hand back a `string` or fill a buffer the extension has to size and own.
- **A separate interface.** Callers need a second registration or a cast to ask for a pinned buffer, and the
  generator's configured defaults must be shared between the two interfaces.
- **Returning a `string` or a copied array.** The result can't be cleared, or a copy outlives the one that is cleared.
- **Renting from `ArrayPool<char>`.** The pool hands the memory that held the secret to the next renter, and a pooled
  array isn't pinned.

## Consequences

- The caller must dispose the owner as soon as the password is no longer needed; `using` does it.
- Each call allocates its own pinned array, which costs more than renting and stays on the pinned object heap until the
  collector frees it.
- An implementation outside the library throws `NotSupportedException` until it overrides the members.
- Copies the caller makes, such as a `string` built from the buffer, aren't cleared.

## References

- [#37](https://github.com/NCodeGroup/PasswordGenerator/issues/37): the request for a pinned password buffer.
- [ADR-0112](project-template/0112-binary-data-uses-spans-pooled-buffers-and-cleared-secrets.md): a long-lived secret is
  held in a pinned array and cleared.
- [ADR-0131](project-template/0131-public-api-is-tracked-and-evolves-compatibly.md): the public API is tracked and
  evolves compatibly.
- [ADR-1000](1000-the-library-targets-the-newest-dotnet-lts-release.md): the library targets the newest .NET LTS
  release, so the buffer calls its pinned-array and cryptography APIs directly.
