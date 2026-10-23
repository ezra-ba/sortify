# Sortify Repository Guide

## Project

Sortify is a local desktop application that watches configured folders and proposes rule-based file names and target directories. A file operation is executed only after explicit user confirmation.

The application uses C# on .NET 10, Avalonia UI, and the MVVM pattern. AI endpoints, cloud services, and external APIs are not required for the core product.

## Solution Structure

- `src/Sortify.App`: Avalonia views, view models, application startup, and dependency composition.
- `src/Sortify.Core`: domain models, rule logic, and interfaces. This project must not depend on Avalonia or infrastructure implementations.
- `src/Sortify.Infrastructure`: folder monitoring, file operations, and local persistence implementations.
- `tests/Sortify.Core.Tests`: automated tests for rule evaluation and other domain behavior.
- `assets/branding`: canonical Sortify and SYD branding assets.
- `docs`: architecture and technical decision documents.

Dependencies flow toward `Sortify.Core`. Keep file-system access out of views and view models.

## Build and Test

Run these commands from the repository root:

```powershell
dotnet restore .\Sortify.sln
dotnet build .\Sortify.sln --configuration Release --no-restore
dotnet test .\Sortify.sln --configuration Release --no-build --no-restore
dotnet run --project .\src\Sortify.App
```

Before opening a pull request, the Release build and all available tests must pass without warnings.

## Code Conventions

- Follow the existing C# and Avalonia patterns in the repository.
- Keep nullable reference types and warnings-as-errors enabled.
- Use MVVM: views describe presentation, view models expose UI state and commands, and services perform application work.
- Keep domain logic deterministic and independently testable in `Sortify.Core`.
- Prefer immutable records for domain values and interfaces at infrastructure boundaries.
- Do not silently overwrite or delete user files.
- Every file operation must preserve the source file on failure.
- Add tests for rule matching, conflicts, invalid input, and file-operation safety as those features are implemented.
- Update documentation when architecture, configuration, or user-visible behavior changes.

## Issue Workflow

- Every change must have one primary GitHub issue.
- The issue must describe the goal and verifiable acceptance criteria.
- Keep one main concern per branch and pull request.
- Link the issue in the pull request with `Closes #<issue-number>` when the PR completes it.
- Do not mix unrelated cleanup or branding changes into a feature PR.

## Branch Names

Sortify uses an extended form of the [Conventional Branch](https://conventionalbranch.org/) specification.

Feature branches must use:

```text
features/<github-username>/<issue-number>/<kebab-case-description>
```

Example:

```text
features/ezra-ba/3/repository-konfiguration
```

Other supported branch types use the same ownership and issue segments:

```text
bugfix/<github-username>/<issue-number>/<kebab-case-description>
hotfix/<github-username>/<issue-number>/<kebab-case-description>
chore/<github-username>/<issue-number>/<kebab-case-description>
codex/<github-username>/<issue-number>/<kebab-case-description>
```

Release branches use `release/v<major>.<minor>.<patch>`. Trunk branches (`main`, `master`, and `develop`) do not use a prefix.

Branch segments must use lowercase letters, digits, and single hyphens. Do not use spaces, underscores, leading or trailing hyphens, or consecutive hyphens.

## Commits and Pull Requests

- Use concise Conventional Commit messages such as `feat:`, `fix:`, `docs:`, `test:`, `refactor:`, and `chore:`.
- Write commit messages in imperative form and keep each commit focused.
- Pull requests must explain the change, reference the issue, and list verification performed.
- Include screenshots for visible Avalonia UI changes.
- Request review from the responsible code owner.
- Do not push directly to the protected trunk branch.

## Ownership

- Ezra Bauchinger (`@ezra-ba`): repository configuration, project coordination, and final integration.
- Manuel Stromberger (`@smanuel89`): Avalonia UI concepts, settings, documentation, and quality assurance.
- Tunahan Barak (`@TuNi02`): rule concepts, regex evaluation, core logic, and branding.

Preserve existing user changes. If unrelated work is present, leave it untouched and stage only files belonging to the current issue.
