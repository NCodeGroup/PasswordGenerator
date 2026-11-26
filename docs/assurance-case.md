# Assurance Case

This page states NCode.PasswordGenerator's security requirements, the threats it considers, and why the design and the
way it is built meet them. [architecture.md](architecture.md) describes the components; [README.md](../README.md) is
the user-facing summary.

## Security Requirements

[//]: # "Maintainers: list what the project is expected to do as R1, R2, ..., and what users must not rely on."

- **R1.** Every random choice, the password's length, each of its characters and their order, comes from
  `System.Security.Cryptography.RandomNumberGenerator`, the platform's cryptographically secure random number
  generator.
- **R2.** A password `Generate` returns or writes meets the options it was generated with: its length is within
  `MinLength` and `MaxLength`, it holds at least the minimum count of lowercase, uppercase, numeric and special
  characters, and no character repeats more than `MaxConsecutiveIdenticalCharacters` times in a row.
- **R3.** When the options can't be met, because the buffer's length is outside the length range, the minimum counts
  add up to zero or to more than the length, or no attempt within `MaxAttempts` passes `IsValid`, `Generate` throws
  `InvalidOperationException` instead of returning a weaker password.
- **R4.** The library keeps no copy of a password it generates, and logs, stores and sends nothing.

Users must not rely on the library for these:

- **Strength of the options.** The library enforces the options it is given and sets no floor of its own. The defaults
  ask for 16 to 64 characters with at least one of each character class, but options such as `SetLengthRange(4, 4)`
  produce four-character passwords.
- **Clearing memory.** A password returned as a `string` stays in memory until the garbage collector reclaims it. To
  clear a password after use, generate it into a `Span<char>` the application owns and clear that.
- **A length of zero.** When `MinLength` is 0, a `Generate` overload that returns a `string` can draw a length of 0,
  and `string.Create` then returns an empty string without calling the generator, so nothing validates it. Keep
  `MinLength` at 1 or more.

## Threat Model

[//]: # "Maintainers: add a table (Threat, In scope, Countered by) and state the trust boundary."

The trust boundary is the calling application's process. The library trusts the application, which supplies the
options and the destination buffer, and the operating system's random number generator behind `RandomNumberGenerator`.
How the application stores, sends or shows a password is outside the boundary.

| Threat | In scope | Countered by |
| --- | --- | --- |
| Passwords are predictable because they come from a seeded or non-cryptographic generator | Yes | R1 |
| The required characters sit at predictable positions, because each class's minimum count is placed first | Yes | R1: `RandomNumberGenerator.Shuffle`, a Fisher–Yates shuffle of the whole password |
| Some characters are likelier than others because a random number is reduced with a modulo | Yes | R1: `RandomNumberGenerator.GetItems` picks each character and `RandomNumberGenerator.GetInt32` the length, each an unbiased value within its range |
| A password weaker than the options ask for is returned | Yes | R2: `PasswordGeneratorOptions.IsValid` checks each password before it is returned, except a length of 0; see A length of zero above |
| Options that can't be met yield a shorter or weaker password instead of an error | Yes | R3, except a length of 0; see A length of zero above |
| The application chooses weak options | No | The application owns its options; see Strength of the options above |
| A password is read from the process's memory or a crash dump | No | The application owns its process; see Clearing memory above |
| A tampered package or dependency reaches users | Yes | The dependency and release integrity controls under Evidence |

## Secure Design Principles Applied

[//]: # "Maintainers: say how the design applies economy of mechanism, fail-safe defaults, complete mediation, least
privilege and open design."

- **Economy of mechanism.** Four public types, two direct package dependencies and one source of randomness
  ([architecture.md](architecture.md#components)).
- **Fail-safe defaults.** The default options ask for 16 to 64 characters with at least one lowercase, uppercase,
  numeric and special character, and a password that fails its options is never returned (R2, R3), except the empty
  string a length of 0 yields (see A length of zero above).
- **Complete mediation.** Every `Generate` overload ends in `Generate(PasswordGeneratorOptions, Span<char>)`, which
  checks the length and the minimum counts and validates each attempt before it returns. The one gap is a `string`
  overload that draws a length of 0 (see A length of zero above).
- **Least privilege.** The library opens no file, network connection or configuration source and needs no permission;
  the application passes it the options.
- **Open design.** The strength rests on the random number generator, not on a secret algorithm: the source is public
  under Apache-2.0.

## Common Weaknesses Countered

[//]: # "Maintainers: list the CWE ids the design counters, each with the requirement that counters it."

- **CWE-330** (Use of Insufficiently Random Values) and **CWE-338** (Use of Cryptographically Weak Pseudo-Random Number
  Generator): R1.
- **CWE-521** (Weak Password Requirements): R2, with the defaults under Fail-safe defaults.
- **CWE-636** (Not Failing Securely): R3.

## Evidence

[//]: # "Maintainers: add the tests and the coverage the project keeps."

- **Tests:** `tests/NCode.PasswordGenerator.Tests` (xUnit) checks the options' length rules, character set and
  `IsValid` (`PasswordGeneratorOptionsTests`), and that `Generate` meets the options, throws when they can't be met or
  `MaxAttempts` runs out, and enumerates `MaxEnumerations` passwords (`PasswordGeneratorTests`). Coverage isn't
  measured.
- **Gate:** `./build/dod.ps1` builds in Release with warnings as errors
  ([ADR-0100](decisions/project-template/0100-warnings-are-errors-in-every-project.md)), runs the analyzers and the
  banned-API list in `build/BannedSymbols.txt`
  ([ADR-0102](decisions/project-template/0102-disallowed-apis-are-banned-at-build-time.md)), and runs the tests; CI runs
  the commands on every pull request.
- **Static analysis:** with CodeQL's default setup configured and analyzing at least one language, the repository
  ruleset blocks a pull request that adds a CodeQL alert at warning level or above, or a security alert of any severity
  ([ADR-0005](decisions/project-template/0005-the-default-branch-accepts-changes-only-through-pull-requests.md),
  [ADR-0030](decisions/project-template/0030-the-code-scanning-rule-is-added-only-where-codeql-analyzes-a-language.md)).
- **Review:** every change reaches the default branch through a pull request with passing CI and every conversation
  resolved, and the ruleset refuses force pushes and deletion
  ([ADR-0005](decisions/project-template/0005-the-default-branch-accepts-changes-only-through-pull-requests.md),
  [ADR-0014](decisions/project-template/0014-every-pull-request-discussion-is-remediated-and-replied-to-before-it-merges.md)).
  NCode Review reviews each pull request and approves a clean one.
- **Dependencies:** pinned, and moved by reviewed updates
  ([ADR-0008](decisions/project-template/0008-external-inputs-are-pinned-and-kept-current.md)) that wait three days for
  a new version ([ADR-0009](decisions/project-template/0009-new-dependency-versions-wait-a-cooldown.md)). Dependabot
  proposes the updates. Central package management keeps one version of each package
  ([ADR-0101](decisions/project-template/0101-central-package-management.md)).
- **Release integrity:** packages are deterministic and source-linked
  ([ADR-0130](decisions/project-template/0130-source-link-and-symbol-packages.md)). The project's own workflow,
  `.github/workflows/main.yml`, publishes them to nuget.org. [SECURITY.md](../SECURITY.md#verify-a-package) says how to
  verify a package.
- **Response:** [SECURITY.md](../SECURITY.md) defines private reporting, coordinated disclosure and credit.

## Security Reviews

[//]: # "Maintainers: record each security review, newest first: its date, who did it, what it covered (the security
requirements and the trust boundary above) and what it found. The OpenSSF Best Practices gold level asks for one in the
last five years."

None recorded yet.
