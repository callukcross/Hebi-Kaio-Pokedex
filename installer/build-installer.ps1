[CmdletBinding()]
param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$artifactRoot = Join-Path $repoRoot "artifacts"
$publishPath = Join-Path $artifactRoot "windows-publish"
$installerPath = Join-Path $artifactRoot "windows-installer"
$payloadPath = Join-Path $PSScriptRoot "payload.zip"

if (Test-Path -LiteralPath $publishPath) { Remove-Item -LiteralPath $publishPath -Recurse -Force }
if (Test-Path -LiteralPath $installerPath) { Remove-Item -LiteralPath $installerPath -Recurse -Force }
if (Test-Path -LiteralPath $payloadPath) { Remove-Item -LiteralPath $payloadPath -Force }
New-Item -ItemType Directory -Path $publishPath, $installerPath -Force | Out-Null

dotnet publish (Join-Path $repoRoot "GUI\Main Menu\GUI.csproj") -c $Configuration -r $Runtime --self-contained true -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o $publishPath
dotnet publish (Join-Path $PSScriptRoot "Uninstaller\HebiKaio.Uninstaller.csproj") -c $Configuration -r $Runtime --self-contained true -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o $publishPath

Compress-Archive -Path (Join-Path $publishPath "*") -DestinationPath $payloadPath -CompressionLevel Optimal
dotnet publish (Join-Path $PSScriptRoot "Installer\HebiKaio.Installer.csproj") -c $Configuration -r $Runtime --self-contained true -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o $installerPath

Remove-Item -LiteralPath $payloadPath -Force
Write-Host "Installer: $(Join-Path $installerPath 'Install-HebiKaio-Pokedex.exe')"
Write-Host "Application: $(Join-Path $publishPath 'HebiKaioPokedex.exe')"
Write-Host "Uninstaller: $(Join-Path $publishPath 'Uninstall-HebiKaio-Pokedex.exe')"
