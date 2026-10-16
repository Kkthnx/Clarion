# Writes the catalog entry for one app, from what winget's own record says, and optionally adds it to apps.json.
#
#   ./scripts/new-app-entry.ps1 -Id Ablaze.Floorp -Name Floorp -Topic Browsers -Summary "A Firefox based browser built in Japan." -Advice "Pick the browser you want."
#   ./scripts/new-app-entry.ps1 ... -Add
#
# The publisher, licence, version, installer type and the host the installer comes from are read with `winget show`, so they are the record's words and not
# remembered ones. The script refuses a package whose manifest says ElevationRequirement: elevationProhibited, because .Clarion always installs as
# administrator (Spotify is the example). It needs winget and a Windows that shows winget in English, since it reads the labels. The summary and the
# advice are yours to write. Run the tests afterwards, then try the app: apply it, look for it, revert it.
#
# Needs: winget, and the GitHub address raw.githubusercontent.com for the installer manifest.
param(
    [Parameter(Mandatory)][string]$Id,
    [Parameter(Mandatory)][string]$Name,
    [Parameter(Mandatory)][string]$Topic,
    [Parameter(Mandatory)][string]$Summary,
    [Parameter(Mandatory)][string]$Advice,
    [string]$Key,
    # What could go wrong, when it is more than an ordinary installer. Start it with the level, as the catalog does: "Medium. ..."
    [string]$Risk,
    [ValidateSet("Low", "Medium", "High")][string]$RiskLevel = "Low",
    # Extra plain sentences for the Facts list, such as who else depends on the package.
    [string[]]$ExtraFacts = @(),
    [switch]$NewTopic,
    [switch]$Add
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$appsFile = Join-Path $root 'src\Clarion.Core\Catalog\Data\apps.json'

if (-not $Key) { $Key = 'app.' + (($Name.ToLowerInvariant() -replace '[^a-z0-9]+', '-').Trim('-')) }
if ($Key -notmatch '^app\.[a-z0-9][a-z0-9\-]*$') { throw "The key $Key is not a plain lower case name." }

$shown = & winget show --id $Id -e --source winget --accept-source-agreements --disable-interactivity 2>&1 | Out-String
if ($shown -notmatch "(?m)^Found .*\[$([regex]::Escape($Id))\]") { throw "winget has no package with the exact ID $Id. It said: $($shown.Trim())" }

# Top level fields come before the first indented block, and the installer block is the one that names an installer type.
$top = @{}
$installer = @{}
$inInstaller = $false
foreach ($line in ($shown -split "`r?`n")) {
    if ($line -match '^Installer:\s*$') { $inInstaller = $true; continue }
    if ($line -match '^\S[^:]*:\s*$') { $inInstaller = $false; continue }
    if ($inInstaller -and $line -match '^\s+([A-Za-z0-9 ]+):\s*(.+)$') { if (-not $installer.ContainsKey($Matches[1])) { $installer[$Matches[1]] = $Matches[2].Trim() }; continue }
    if (-not $inInstaller -and $line -match '^([A-Za-z ]+):\s*(.+)$') { if (-not $top.ContainsKey($Matches[1])) { $top[$Matches[1]] = $Matches[2].Trim() } }
}
foreach ($need in 'Version', 'Publisher', 'License') { if (-not $top[$need]) { throw "winget's record for $Id has no $need." } }
foreach ($need in 'Installer Type', 'Installer Url') { if (-not $installer[$need]) { throw "winget's record for $Id has no $need." } }

$folder = 'manifests/' + $Id.Substring(0, 1).ToLowerInvariant() + '/' + ($Id -replace '\.', '/')
$manifestUrl = "https://raw.githubusercontent.com/microsoft/winget-pkgs/master/$folder/$($top['Version'])/$Id.installer.yaml"
try { $manifest = (Invoke-WebRequest -Uri $manifestUrl -UseBasicParsing -TimeoutSec 30).Content }
catch { throw "Could not read the installer manifest at $manifestUrl. Check the elevation requirement by hand. $($_.Exception.Message)" }
if ($manifest -match '(?m)^\s*ElevationRequirement:\s*elevationProhibited') { throw "$Id forbids an administrator install, so .Clarion cannot install it." }
$location = $null
if ($manifest -match '(?m)^\s*InstallLocationRequired:\s*true') {
    # The package cannot install without a folder, so the entry carries the one its own manifest names.
    if ($manifest -notmatch "(?m)^\s*DefaultInstallLocation:\s*(.+)$") { throw "$Id needs an install folder and its manifest names none. Add the entry by hand: $manifestUrl" }
    $location = $Matches[1].Trim().Trim("'").Trim('"')
    $location = $location -replace '(?i)%PROGRAMFILES\(X86\)%', '%ProgramFiles(x86)%' -replace '(?i)%PROGRAMFILES%', '%ProgramFiles%'
}

$hostName = ([Uri]$installer['Installer Url']).Host
$tree = "https://github.com/microsoft/winget-pkgs/tree/master/$folder"
$sources = @($tree)
foreach ($label in 'Homepage', 'License Url') {
    $u = $top[$label]
    if ($u -and $u -match '^https://' -and -not ($sources | Where-Object { $_.TrimEnd('/') -ieq $u.TrimEnd('/') })) { $sources += $u }
}

$entry = [ordered]@{
    id                = $Key
    lastVerifiedBuild = [int][Environment]::OSVersion.Version.Build
    category          = 'Install'
    topic             = $Topic
    name              = $Name
    summary           = $Summary
    what              = "Installs $Name with winget, pinned to the package $Id in winget's community source. Turning it off runs the program's own uninstaller through winget."
    benefit           = "You get $Name without hunting for the right download page, and winget checks the installer's checksum before it runs."
    risk              = $(if ($Risk) { $Risk } else { "Low. The publisher's installer runs with administrator rights. Read the licence terms on the review screen before you accept them." })
    evidence          = 'Proven'
    riskLevel         = $RiskLevel
    recommendation    = 'Optional'
    advice            = $Advice
    facts             = @(
        "Publisher: $($top['Publisher'])",
        "Licence: $($top['License']), as winget's package record states it",
        "The installer is downloaded from $hostName and checked against a checksum",
        "Installer type: $($installer['Installer Type']), package ID $Id"
    ) + $ExtraFacts + $(if ($location) { @("Installs into $($location -replace '%ProgramFiles\(x86\)%', 'Program Files (x86)' -replace '%ProgramFiles%', 'Program Files'), the folder the winget package names, because its installer cannot run without one") } else { @() })
    scope             = 'Machine'
    apply             = @($(if ($location) { [ordered]@{ type = 'winget.install'; id = $Id; location = $location } } else { [ordered]@{ type = 'winget.install'; id = $Id } }))
    undo              = @([ordered]@{ type = 'winget.uninstall'; id = $Id })
    sources           = $sources
}

if (-not $Add) { $entry | ConvertTo-Json -Depth 10; return }

$raw = [IO.File]::ReadAllText($appsFile)
$list = [System.Collections.Generic.List[object]]::new()
foreach ($e in ($raw | ConvertFrom-Json)) { $list.Add($e) }
if ($list | Where-Object { $_.id -eq $Key }) { throw "The catalog already has $Key." }
if ($list | Where-Object { $_.apply[0].id -ieq $Id }) { throw "The catalog already installs $Id." }

# Groups keep the order they first appear in, and each group is by name.
$topics = @($list | ForEach-Object { $_.topic } | Select-Object -Unique)
if ($topics -notcontains $Topic) {
    if (-not $NewTopic) { throw "The topic $Topic is new. Add -NewTopic to start it. The topics are: $($topics -join ', ')." }
    $topics += $Topic
}
$index = $list.Count
for ($i = 0; $i -lt $list.Count; $i++) {
    $t = $topics.IndexOf($list[$i].topic); $mine = $topics.IndexOf($Topic)
    if ($t -gt $mine -or ($t -eq $mine -and [string]::Compare($list[$i].name, $Name, [StringComparison]::OrdinalIgnoreCase) -gt 0)) { $index = $i; break }
}
$list.Insert($index, [pscustomobject]$entry)
$text = ($list | ConvertTo-Json -Depth 10) -replace "`r?`n", "`r`n"
[IO.File]::WriteAllText($appsFile, $text)
"Added $Key before position $index. $($list.Count) apps now."
