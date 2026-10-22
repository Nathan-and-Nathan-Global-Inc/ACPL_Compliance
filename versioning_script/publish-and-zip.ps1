<#
Publish and Zip for sfl_startup
- Publishes the ASP.NET Core project
- Reads version from .csproj (Version or VersionPrefix/Suffix)
- Creates a versioned ZIP in your specified OneDrive folder

Path used:
E:\OneDrive - Nathan and Nathan Global Inc\_share_with_customers\ascent_releases_stagging

Author: Balakrishnan S (tailored)
#>

param(
    [string]$Configuration = "Release",
    [string]$Framework     = "net8.0",
    # Relative to this script. Update if you rename the project file.
    [string]$ProjectPath   = "..\prvalid.csproj",
    # Optional override for the ZIP's app name; default is the csproj name
    [string]$AppName       = "",
    # Target output folder for the ZIP (your OneDrive folder). You can override at runtime if needed.
    [string]$ReleaseFolder = "E:\OneDrive - Nathan and Nathan Global Inc\_share_with_customers\ascent_releases_stagging"
)

$ErrorActionPreference = "Stop"


Write-Host "=== PUBLISH AND ZIP: acpl_mom ===" -ForegroundColor Cyan

# -------------------------------
# Resolve absolute paths
# -------------------------------
$ScriptDir       = Split-Path -Parent $MyInvocation.MyCommand.Path
$ResolvedCsproj  = Resolve-Path (Join-Path $ScriptDir $ProjectPath)
$ProjectFolder   = Split-Path -Parent $ResolvedCsproj
$TempPublishDir  = Join-Path $ProjectFolder "publish-temp"

if ([string]::IsNullOrWhiteSpace($AppName)) {
    $AppName = [IO.Path]::GetFileNameWithoutExtension($ResolvedCsproj)
}

Write-Host "Project   : $ResolvedCsproj" -ForegroundColor DarkGray
Write-Host "App Name  : $AppName" -ForegroundColor DarkGray
Write-Host "Framework : $Framework" -ForegroundColor DarkGray
Write-Host "Config    : $Configuration" -ForegroundColor DarkGray
Write-Host "ReleaseTo : $ReleaseFolder" -ForegroundColor DarkGray
Write-Host ""

# -------------------------------
# Step 1: Read Version from .csproj
# -------------------------------
function Get-ProjectVersion {
    param([string]$csprojPath)
    [xml]$xml = Get-Content -LiteralPath $csprojPath

    $versionNode = $xml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    if ($versionNode) { return $versionNode }

    $prefix = $xml.Project.PropertyGroup.VersionPrefix | Where-Object { $_ } | Select-Object -First 1
    $suffix = $xml.Project.PropertyGroup.VersionSuffix | Where-Object { $_ } | Select-Object -First 1
    if ($prefix) { return "$prefix$suffix" }

    return $null
}

$version = Get-ProjectVersion -csprojPath $ResolvedCsproj
if (-not $version) {
    throw "Version not found in $ResolvedCsproj. Define <Version> or <VersionPrefix>/<VersionSuffix> in the .csproj."
}
Write-Host "Detected Version: $version" -ForegroundColor Green

# -------------------------------
# Step 2: dotnet publish
# -------------------------------
if (Test-Path -LiteralPath $TempPublishDir) {
    Write-Host "Cleaning previous publish folder..." -ForegroundColor DarkGray
    Remove-Item -LiteralPath $TempPublishDir -Recurse -Force
}

Write-Host "Running: dotnet publish ..." -ForegroundColor DarkGray
$publishArgs = @(
    "`"$ResolvedCsproj`"",
    "-c", $Configuration,
    "-f", $Framework,
    "-o", "`"$TempPublishDir`"",
    "-p:ContinuousIntegrationBuild=true"
)

& dotnet publish @publishArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}
Write-Host "Publish completed." -ForegroundColor Green

# -------------------------------
# Step 3: Build ZIP name (timestamp, optional git commit)
# -------------------------------
$timestamp = (Get-Date).ToString("yyyyMMdd-HHmm")
$zipName   = "$AppName-$version-$timestamp.zip"

# Optional git short commit
try {
    $gitCommit = (git -C "`"$ProjectFolder`"" rev-parse --short HEAD 2>$null)
    if ($LASTEXITCODE -eq 0 -and $gitCommit) {
        $zipName = "$AppName-$version-$gitCommit-$timestamp.zip"
        Write-Host "Git commit detected: $gitCommit" -ForegroundColor DarkGray
    }
} catch { }

# -------------------------------
# Step 4: Ensure release folder exists (your OneDrive path)
# -------------------------------
if (-not (Test-Path -LiteralPath $ReleaseFolder)) {
    Write-Host "Creating release folder: $ReleaseFolder" -ForegroundColor DarkGray
    New-Item -ItemType Directory -Force -Path $ReleaseFolder | Out-Null
}

$zipPath = Join-Path $ReleaseFolder $zipName
Write-Host "ZIP target : $zipPath" -ForegroundColor DarkGray

# -------------------------------
# Step 5: Create ZIP
# -------------------------------
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Write-Host "Creating ZIP..." -ForegroundColor DarkGray
Compress-Archive -Path (Join-Path $TempPublishDir "*") -DestinationPath $zipPath -Force

Write-Host ""
Write-Host "ZIP created:" -ForegroundColor Yellow
Write-Host " $zipPath" -ForegroundColor Yellow

Write-Host ""
Write-Host "=== DONE ===" -ForegroundColor Cyan