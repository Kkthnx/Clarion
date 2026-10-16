# Works out which apps may be installed for one account only, and writes src/Clarion.Core/Catalog/Apps/app-scopes.json.
#
#   ./scripts/check-winget-scopes.ps1
#
# Microsoft says scope is not consistent across installers: MSIX packages are reliable, MSI mostly, and for EXE installers "behavior around scope is
# not necessarily deterministic" (learn.microsoft.com/windows/package-manager/winget/troubleshooting). So winget accepting --scope user is not enough,
# because for an installer that names no scope it passes the flag along and the installer may ignore it. An app is offered for one account only when
# BOTH are true:
#   1. winget itself picks an installer for it with `winget show --scope user`, and
#   2. either its installer manifest in winget-pkgs declares `Scope: user` (the package's maintainers say it works), or winget itself carries out the
#      scope, which it does for portable and MSIX packages.
# Needs winget, the GitHub CLI signed in (or GITHUB_TOKEN), and a Windows that shows winget in English, since it reads the English labels.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$apps = [IO.File]::ReadAllText((Join-Path $root 'src\Clarion.Core\Catalog\Data\apps.json')) | ConvertFrom-Json
$outFile = Join-Path $root 'src\Clarion.Core\Catalog\Apps\app-scopes.json'
$token = $env:GITHUB_TOKEN
if (-not $token -and (Get-Command gh -ErrorAction SilentlyContinue)) { $token = (& gh auth token) 2>$null }
if (-not $token) { throw 'Sign in with the GitHub CLI (gh auth login) or set GITHUB_TOKEN, since about 500 requests are made.' }
$headers = @{ 'User-Agent' = 'ClarionScopeCheck'; 'Accept' = 'application/vnd.github+json'; 'Authorization' = "Bearer $token" }

function Get-List([string]$Path) {
    try { return @(Invoke-RestMethod -Uri "https://api.github.com/repos/microsoft/winget-pkgs/contents/$Path" -Headers $headers -TimeoutSec 30) } catch { return $null }
}

$result = [ordered]@{}
$checked = 0
foreach ($app in $apps) {
    $id = [string]$app.apply[0].id
    $checked++
    $shown = & winget show --id $id -e --source winget --scope user --accept-source-agreements --disable-interactivity 2>&1 | Out-String
    $selected = ($shown -match '(?m)^\s+Installer Url:') -and ($shown -notmatch 'No applicable installer found')
    if (-not $selected) { continue }
    $type = if ($shown -match '(?m)^\s+Installer Type:\s*(\S+)') { $Matches[1] } else { '' }

    # Does the installer manifest say Scope: user?
    $folder = 'manifests/' + $id.Substring(0, 1).ToLowerInvariant() + '/' + ($id -replace '\.', '/')
    $declared = $false
    $dirs = Get-List $folder
    foreach ($d in @($dirs | Where-Object { $_.type -eq 'dir' } | Sort-Object name -Descending)) {
        $files = Get-List "$folder/$($d.name)"
        $installer = $files | Where-Object { $_.name -ceq "$id.installer.yaml" }
        if (-not $installer) { continue }
        $text = (Invoke-WebRequest -Uri $installer.download_url -UseBasicParsing -TimeoutSec 30).Content
        $declared = $text -match '(?m)^\s*-?\s*Scope:\s*user\s*$'
        break
    }

    $basis = if ($declared) { 'declared' } elseif ($type -in 'portable', 'msix') { $type } else { $null }
    if ($null -eq $basis) { continue }
    $result[[string]$app.id] = [ordered]@{ basis = $basis; installerType = $type }
    Write-Host "$checked  $($app.id)  $basis  $type"
}

$sorted = [ordered]@{}
foreach ($k in ($result.Keys | Sort-Object)) { $sorted[$k] = $result[$k] }
$json = ($sorted | ConvertTo-Json -Depth 4) -replace "`r?`n", "`r`n"
if ($sorted.Count -eq 0) { $json = '{}' }
[IO.File]::WriteAllText($outFile, $json)
Write-Host "$($sorted.Count) of $($apps.Count) apps may be installed for one account only."
