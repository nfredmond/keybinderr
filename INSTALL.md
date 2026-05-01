# Install Keybinderr

Keybinderr is a Windows tray app. The easiest install path is the signed-by-Windows-user-mode installer artifact produced by GitHub Actions.

## Option 1 — Download the installer artifact

1. Open the repository's **Actions** tab on GitHub.
2. Select **Build Windows Installer**.
3. Open the latest successful run.
4. Download the `KeybinderrSetup` artifact.
5. Unzip it and run `KeybinderrSetup.exe`.

The installer places Keybinderr under your user profile and does not require administrator rights.

## Option 2 — Build locally from source

Requirements:

- Windows 11
- .NET 10 SDK
- Inno Setup 6, if you want the `.exe` installer

From PowerShell:

```powershell
git clone https://github.com/nfredmond/keybinderr.git
cd keybinderr
.\scripts\package.ps1
```

The installer will be written to:

```text
artifacts\installer\KeybinderrSetup.exe
```

To publish the self-contained app without Inno Setup:

```powershell
.\scripts\package.ps1 -SkipInstaller
```

The app files will be written to:

```text
artifacts\publish\Keybinderr
```

## First launch

After install, launch **Keybinderr** from the Start Menu. Use **Keybinderr Settings** to create or edit game profiles. Keep `Normal / QWERTY` available as the safety profile.

## Compatibility note

Keybinderr uses a user-mode keyboard hook, not a driver. That keeps install simple and avoids admin prompts, but games using raw input or strict anti-cheat may ignore remapped keys.
