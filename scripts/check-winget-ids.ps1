# Checks that every winget package ID in the app catalog still exists in winget's community source. Run before a release, and monthly by
# .github/workflows/check-sources.yml.
#
# The community source is the microsoft/winget-pkgs repository. A package with the ID Publisher.Name.Sub has its files under
# manifests/<first letter of the ID, lower case>/Publisher/Name/Sub/<version>/<ID>.yaml, so the ID is looked up there by its exact spelling.
# winget itself is not needed, which means this also runs on a runner that does not have it.
# An ID is MISSING when the folder or its version files are not there. Anything that is not an answer from GitHub (a timeout, a rate limit)
# is listed as UNSURE and does not fail the run.
# Exit code: 1 when at least one ID is missing, otherwise 0.
param([string]$ReportPath)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$apps = Get-Content (Join-Path $root 'src\Clarion.Core\Catalog\Data\apps.json') -Raw | ConvertFrom-Json
$ids = @($apps | ForEach-Object { $_.apply } | Where-Object { $_.type -eq 'winget.install' } | ForEach-Object { $_.id } | Sort-Object -Unique)
if ($ids.Count -eq 0) { throw 'No winget package IDs were found in apps.json.' }

$token = $env:GITHUB_TOKEN
if (-not $token -and (Get-Command gh -ErrorAction SilentlyContinue)) { $token = (& gh auth token) 2>$null }
$headers = @{ 'User-Agent' = 'ClarionIdCheck'; 'Accept' = 'application/vnd.github+json' }
if ($token) { $headers['Authorization'] = "Bearer $token" }

# One folder listing. Returns the entries, $null when GitHub says it is not there, and throws on any other failure.
function Get-Listing([string]$Path) {
    $url = "https://api.github.com/repos/microsoft/winget-pkgs/contents/$Path"
    try { return @(Invoke-RestMethod -Uri $url -Headers $headers -TimeoutSec 30) }
    catch {
        $response = $_.Exception.Response
        if ($null -ne $response -and [int]$response.StatusCode -eq 404) { return $null }
        throw
    }
}

$missing = [System.Collections.Generic.List[string]]::new()
$unsure = [System.Collections.Generic.List[string]]::new()
foreach ($id in $ids) {
    $folder = 'manifests/' + $id.Substring(0, 1).ToLowerInvariant() + '/' + ($id -replace '\.', '/')
    try {
        $versions = Get-Listing $folder
        if ($null -eq $versions) { $missing.Add("$id (no folder $folder)"); continue }
        # The folder of a package also holds the folders of packages below it, so a version folder is the one that has the ID's own file.
        $found = $false
        foreach ($dir in @($versions | Where-Object { $_.type -eq 'dir' } | Sort-Object name -Descending)) {
            $files = Get-Listing "$folder/$($dir.name)"
            if ($null -ne $files -and ($files | Where-Object { $_.name -ceq "$id.installer.yaml" })) { $found = $true; break }
        }
        if (-not $found) { $missing.Add("$id (folder exists, but no version holds $id.installer.yaml)") }
    }
    catch { $unsure.Add("$id ($($_.Exception.Message))") }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("## winget package IDs")
$lines.Add("")
$lines.Add("Checked $($ids.Count) IDs against microsoft/winget-pkgs. Missing: $($missing.Count). Unsure: $($unsure.Count).")
foreach ($m in $missing) { $lines.Add("- MISSING: $m") }
foreach ($u in $unsure) { $lines.Add("- UNSURE: $u") }
$lines | ForEach-Object { Write-Host $_ }
if ($ReportPath) { $lines | Set-Content -Path $ReportPath -Encoding utf8 }
if ($missing.Count -gt 0) { exit 1 }
