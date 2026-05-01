# Keybinderr

Keybinderr is a Windows 11 tray utility for per-game keyboard remapping. It is designed for players who want game-specific layouts, such as ESDF movement in games that only expose WASD defaults, without changing normal typing behavior outside the focused game window.

## Features

- Tray-first Windows app with a settings window.
- Built-in `Normal / QWERTY` and `ESDF RPG` profiles.
- Editable per-game profiles matched by `.exe` path and optional window title.
- User-mode keyboard hook that remaps only when the matching game window is foreground.
- Per-user startup toggle using the current user's Windows Run key.
- Inno Setup packaging for an easy `.exe` installer.

## Requirements

- Windows 11 for running the app.
- .NET 10 SDK for development and publishing.
- Inno Setup 6 for building the installer.

## Easy Install

See [INSTALL.md](INSTALL.md) for the easiest Windows install path. GitHub Actions builds a `KeybinderrSetup.exe` installer artifact on every push to `main`, and the app installs under the current user without requiring administrator rights.

## Commands

```powershell
dotnet restore .\Keybinderr.sln
dotnet build .\Keybinderr.sln
dotnet test .\Keybinderr.sln
dotnet run --project .\src\Keybinderr.App\Keybinderr.App.csproj
.\scripts\package.ps1
```

Use `.\scripts\package.ps1 -SkipInstaller` to publish the self-contained app without requiring Inno Setup.

## Compatibility Notes

Keybinderr intentionally uses a user-mode hook, not a driver. That keeps installation simple and avoids admin prompts, but some games that use raw input or anti-cheat protection may ignore remapped keys.

