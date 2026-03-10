# AGENTS.md

## Mandatory validation

Before finishing any coding task, always run:

- dotnet restore MachSoft.sln
- dotnet build MachSoft.sln --no-restore

If tests are available and relevant, also run:

- dotnet test MachSoft.sln --no-build --no-restore

## Rules

- Do not stop after editing code without validating compilation.
- Fix compile errors introduced by your own changes before finishing.
- Prefer minimal, high-confidence changes.
- Avoid broad refactors unless explicitly requested.
