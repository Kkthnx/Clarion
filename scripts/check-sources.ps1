# Checks that every source link in the catalog still resolves. Run before a release, and monthly by .github/workflows/check-sources.yml.
#
# A link counts as DEAD only when the site answers 404 or 410. Anything else that is not a success (a timeout, a 403 from a bot filter,
# a 429 or a 5xx) is listed as UNSURE and does not fail the run, because those usually pass on the next try.
# Exit code: 1 when at least one link is dead, otherwise 0.
param([string]$ReportPath)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$files = Get-ChildItem -Path (Join-Path $root 'src\Clarion.Core\Catalog') -Filter *.json -Recurse

# Link -> the catalog entries that cite it, so a dead link can be traced back.
$citedBy = @{}
foreach ($file in $files) {
    $currentId = ''
    foreach ($line in Get-Content $file.FullName) {
        if ($line -match '"id":\s*"([^"]+)"') { $currentId = $Matches[1] }
        foreach ($m in [regex]::Matches($line, 'https://[^"\s]+')) {
            if (-not $citedBy.ContainsKey($m.Value)) { $citedBy[$m.Value] = [System.Collections.Generic.List[string]]::new() }
            if ($currentId -and -not $citedBy[$m.Value].Contains($currentId)) { $citedBy[$m.Value].Add($currentId) }
        }
    }
}

$headers = @{ 'User-Agent' = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) ClarionSourceCheck' }

function Get-StatusCode([string]$Url, [string]$Method) {
    try {
        $r = Invoke-WebRequest -Uri $Url -Method $Method -Headers $headers -MaximumRedirection 5 -TimeoutSec 25 -UseBasicParsing -ErrorAction Stop
        return [int]$r.StatusCode
    }
    catch {
        $response = $_.Exception.Response
        if ($null -ne $response) { return [int]$response.StatusCode }
        return 0   # no answer at all: timeout, DNS failure, connection refused
    }
}

$dead = [System.Collections.Generic.List[string]]::new()
$unsure = [System.Collections.Generic.List[string]]::new()
$ok = 0

foreach ($url in ($citedBy.Keys | Sort-Object)) {
    $code = Get-StatusCode $url 'Head'
    # Some sites refuse HEAD, or only answer a real request. Ask again the ordinary way before judging.
    if ($code -lt 200 -or $code -ge 400) { $code = Get-StatusCode $url 'Get' }

    $who = ($citedBy[$url] -join ', ')
    if ($code -ge 200 -and $code -lt 400) { $ok++; Write-Host "$code  $url" }
    elseif ($code -eq 404 -or $code -eq 410) { $dead.Add("$code  $url  (cited by $who)"); Write-Host "DEAD $code  $url  ($who)" -ForegroundColor Red }
    else { $unsure.Add("$code  $url  (cited by $who)"); Write-Host "UNSURE $code  $url  ($who)" -ForegroundColor Yellow }
}

$summary = "Checked $($citedBy.Count) links: $ok fine, $($unsure.Count) unsure, $($dead.Count) dead."
Write-Host ''
Write-Host $summary

if ($ReportPath) {
    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('# Source link check')
    $lines.Add('')
    $lines.Add($summary)
    if ($dead.Count -gt 0) { $lines.Add(''); $lines.Add('## Dead (404 or 410). Fix or replace these.'); foreach ($d in $dead) { $lines.Add("- $d") } }
    if ($unsure.Count -gt 0) { $lines.Add(''); $lines.Add('## Unsure. Usually passes on the next run.'); foreach ($u in $unsure) { $lines.Add("- $u") } }
    Set-Content -Path $ReportPath -Value $lines -Encoding UTF8
}

if ($dead.Count -gt 0) { exit 1 }
