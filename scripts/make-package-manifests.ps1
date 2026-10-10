# Writes the winget and Scoop manifests for one release, from the checksums that release published.
# Nothing is submitted anywhere. The files are for you to read, validate and send.
#
#   ./scripts/make-package-manifests.ps1 -License "MIT"
#   ./scripts/make-package-manifests.ps1 -Version 0.1.0-beta.6 -License "Proprietary" -OutDir packaging
#
# -License has no default on purpose. Both package managers show it to people, and the project has not chosen one yet.
param(
    [string]$Version,
    [Parameter(Mandatory = $true)][string]$License,
    [string]$LicenseUrl = "",
    [string]$OutDir = "packaging"
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
if (-not $Version) {
    $Version = ([xml](Get-Content (Join-Path $root 'Directory.Build.props'))).Project.PropertyGroup.Version
}
$repo = 'Kkthnx/Clarion'
$tag = "v$Version"
$setupName = "Clarion-$Version-setup.exe"
$zipName = "Clarion-$Version-win-x64.zip"

function Get-Sha256([string]$assetName) {
    # The release page is the source of truth, so the hash is read from the checksum file that was uploaded with the release.
    $local = Join-Path $root "artifacts\$assetName.sha256"
    if (Test-Path $local) { return ((Get-Content $local -Raw).Trim() -split '\s+')[0].ToLowerInvariant() }
    $tmp = Join-Path ([IO.Path]::GetTempPath()) ("clarion-sha-" + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory $tmp | Out-Null
    try {
        gh release download $tag --repo $repo --pattern "$assetName.sha256" --dir $tmp
        if ($LASTEXITCODE -ne 0) { throw "Could not read $assetName.sha256 from release $tag." }
        return ((Get-Content (Join-Path $tmp "$assetName.sha256") -Raw).Trim() -split '\s+')[0].ToLowerInvariant()
    }
    finally { [IO.Directory]::Delete($tmp, $true) }
}

$setupHash = Get-Sha256 $setupName
$zipHash = Get-Sha256 $zipName
foreach ($h in @($setupHash, $zipHash)) { if ($h -notmatch '^[0-9a-f]{64}$') { throw "A checksum is not 64 hex characters: $h" } }

$base = "https://github.com/$repo/releases/download/$tag"
$id = 'Kkthnx.Clarion'
$outRoot = if ([IO.Path]::IsPathRooted($OutDir)) { $OutDir } else { Join-Path $root $OutDir }
$dir = Join-Path $outRoot "winget\manifests\k\Kkthnx\Clarion\$Version"
New-Item -ItemType Directory -Force $dir | Out-Null
$utf8 = New-Object System.Text.UTF8Encoding($false)
$schema = '1.12.0'

[IO.File]::WriteAllText((Join-Path $dir "$id.yaml"), @"
# yaml-language-server: `$schema=https://aka.ms/winget-manifest.version.$schema.schema.json
PackageIdentifier: $id
PackageVersion: $Version
DefaultLocale: en-US
ManifestType: version
ManifestVersion: $schema
"@, $utf8)

$licenseUrlLine = if ($LicenseUrl) { "LicenseUrl: $LicenseUrl`n" } else { "" }
[IO.File]::WriteAllText((Join-Path $dir "$id.locale.en-US.yaml"), @"
# yaml-language-server: `$schema=https://aka.ms/winget-manifest.defaultLocale.$schema.schema.json
PackageIdentifier: $id
PackageVersion: $Version
PackageLocale: en-US
Publisher: Kkthnx
PublisherUrl: https://github.com/Kkthnx
PackageName: Clarion
PackageUrl: https://github.com/$repo
License: $License
${licenseUrlLine}ShortDescription: See what Windows is doing, change only what you choose, and undo any of it.
Description: A Windows 11 tool for privacy settings, app removal, cleanup and repair. Every change is explained first, recorded with the value it replaced, and can be reverted. It sends nothing anywhere.
Tags:
- debloat
- privacy
- tweaks
- windows-11
- cleanup
ReleaseNotesUrl: https://github.com/$repo/releases/tag/$tag
ManifestType: defaultLocale
ManifestVersion: $schema
"@, $utf8)

[IO.File]::WriteAllText((Join-Path $dir "$id.installer.yaml"), @"
# yaml-language-server: `$schema=https://aka.ms/winget-manifest.installer.$schema.schema.json
PackageIdentifier: $id
PackageVersion: $Version
InstallerType: inno
Scope: machine
ElevationRequirement: elevationRequired
MinimumOSVersion: 10.0.19041.0
UpgradeBehavior: install
Installers:
- Architecture: x64
  InstallerUrl: $base/$setupName
  InstallerSha256: $($setupHash.ToUpperInvariant())
ManifestType: installer
ManifestVersion: $schema
"@, $utf8)

# Scoop installs the portable zip. The version check reads the release list, because the "latest" address skips pre-releases.
$scoopDir = Join-Path $outRoot "scoop"
New-Item -ItemType Directory -Force $scoopDir | Out-Null
$scoop = [ordered]@{
    version      = $Version
    description  = 'See what Windows is doing, change only what you choose, and undo any of it.'
    homepage     = "https://github.com/$repo"
    license      = $License
    notes        = 'Clarion asks for administrator rights when it opens, because most of what it changes belongs to the whole PC.'
    architecture = [ordered]@{ '64bit' = [ordered]@{ url = "$base/$zipName"; hash = $zipHash } }
    shortcuts    = @(, @('Clarion.exe', 'Clarion'))
    checkver     = [ordered]@{ url = "https://api.github.com/repos/$repo/releases"; jsonpath = '$[0].tag_name'; regex = 'v([\w.\-]+)' }
    autoupdate   = [ordered]@{ architecture = [ordered]@{ '64bit' = [ordered]@{ url = "https://github.com/$repo/releases/download/v`$version/Clarion-`$version-win-x64.zip" } } }
}
[IO.File]::WriteAllText((Join-Path $scoopDir 'clarion.json'), ($scoop | ConvertTo-Json -Depth 6), $utf8)

Write-Host "winget: $dir"
Write-Host "scoop:  $scoopDir\clarion.json"
Write-Host "Check the winget files with:  winget validate --manifest `"$dir`""
