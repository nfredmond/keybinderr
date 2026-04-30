# Repository Guidelines

## Project Structure & Module Organization

Keep the layout predictable:

- `src/Keybinderr.Core/` contains profile models, matching logic, validation, and JSON persistence.
- `src/Keybinderr.App/` contains the Windows tray app, WPF settings window, startup integration, and keyboard hook.
- `tests/Keybinderr.Core.Tests/` contains xUnit tests for platform-neutral behavior.
- `installer/` and `scripts/` contain packaging assets for the Windows installer.

Prefer small modules with clear ownership. If a feature spans multiple files, group it under a named subdirectory.

## Build, Test, and Development Commands

Run these from the repository root on Windows with the .NET 10 SDK installed:

- `dotnet restore .\Keybinderr.sln`: restore NuGet packages.
- `dotnet build .\Keybinderr.sln`: compile the app and tests.
- `dotnet test .\Keybinderr.sln`: run xUnit tests.
- `dotnet run --project .\src\Keybinderr.App\Keybinderr.App.csproj`: launch the tray app.
- `.\scripts\package.ps1`: publish a self-contained app and build the Inno Setup installer.

## Coding Style & Naming Conventions

Follow `.editorconfig`. Use 4-space indentation for C# and file-scoped namespaces. Keep platform-neutral logic in `Keybinderr.Core`; keep Win32, WPF, registry, and tray code in `Keybinderr.App`.

Recommended naming patterns:

- Files and directories: match the primary type name for C# files, for example `ProfileMatcher.cs`.
- Classes and records: `PascalCase`.
- Functions, variables, and methods: `camelCase`.
- Tests: name files after the unit under test, for example `ProfileMatcherTests.cs`.

## Testing Guidelines

Add tests with every behavior change in core profile, matching, validation, or persistence logic. Cover keyboard layout edge cases, process/window matching, disabled profiles, duplicate mappings, and invalid input.

Document any manual verification steps in the pull request when automation is not practical.

## Commit & Pull Request Guidelines

This checkout has no Git history available, so no existing commit convention can be inferred. Use short, imperative commit messages, preferably Conventional Commits:

- `feat: add keybinding parser`
- `fix: handle duplicate shortcuts`
- `test: cover invalid key sequences`

Pull requests should include a concise summary, testing performed, linked issues when applicable, and screenshots or screen recordings for UI changes. Keep PRs focused; separate refactors from behavior changes when possible.

## Agent-Specific Instructions

Do not assume missing tooling exists. Inspect the repository before editing and preserve user changes. On non-Windows hosts, note that the WPF app and installer cannot be fully validated locally.
