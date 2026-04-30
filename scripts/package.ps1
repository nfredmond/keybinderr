param(
  [string]$Configuration = "Release",
  [string]$Runtime = "win-x64",
  [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $repoRoot "artifacts\publish\Keybinderr"
$installerDir = Join-Path $repoRoot "artifacts\installer"

dotnet publish (Join-Path $repoRoot "src\Keybinderr.App\Keybinderr.App.csproj") `
  --configuration $Configuration `
  --runtime $Runtime `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:PublishDir=$publishDir

if ($SkipInstaller) {
  Write-Host "Published app to $publishDir"
  exit 0
}

$iscc = Get-Command "iscc.exe" -ErrorAction SilentlyContinue
if (-not $iscc) {
  throw "Inno Setup Compiler (iscc.exe) was not found. Install Inno Setup or rerun with -SkipInstaller."
}

New-Item -ItemType Directory -Force -Path $installerDir | Out-Null
& $iscc.Source (Join-Path $repoRoot "installer\keybinderr.iss") "/DPublishDir=$publishDir" "/DOutputDir=$installerDir"
Write-Host "Installer written to $installerDir"

