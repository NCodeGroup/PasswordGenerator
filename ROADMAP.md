# Roadmap

What NCode.PasswordGenerator intends to do over the next year. Open issues hold the detail and the current state; this
page is the direction. It is reviewed at least once a year and when a release changes it.

[//]: # "Maintainers: replace the prompts with the project's own plans, and review this page at least once a year."

## Scope

NCode.PasswordGenerator covers what [README.md](README.md) describes. A feature outside that starts as an issue, where
the maintainers say whether it fits.

## Next Twelve Months

[//]: # "Maintainers: list what the project plans to do in the next twelve months, one bullet each."

- **Publishing.** Do the one-time setup `.github/workflows/publish.yml` describes in its header, so releases publish to
  nuget.org through trusted publishing from the protected `nuget` environment: the nuget.org trusted publishing policy
  for this repository and `publish.yml`, the `nuget` environment with required reviewers, and the `NUGET_USER` variable.
  Then publish a release through it and revoke any old nuget.org API key
  ([#11](https://github.com/NCodeGroup/PasswordGenerator/issues/11)).
- **Supply chain.** Keep external inputs pinned and current
  ([ADR-0008](docs/decisions/project-template/0008-external-inputs-are-pinned-and-kept-current.md)), and raise the
  OpenSSF Scorecard score where the repository controls it
  ([ADR-0020](docs/decisions/project-template/0020-github-projects-are-scored-by-openssf-scorecard.md)).

## Not Planned

[//]: # "Maintainers: list what the project rules out, and why."

- **.NET Framework.** The library is built on span APIs that .NET Framework lacks, as
  [README.md](README.md#requirements) says.
