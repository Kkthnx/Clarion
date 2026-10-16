# Adds one Simple Icons logo (the brand's outline, in the brand's colour) to the app logos, from the same release the others came from.
#
#   ./scripts/add-simple-icon.ps1 -Id app.vagrant -Slug vagrant
#   ./scripts/add-simple-icon.ps1 -Check      compares the width over height this script works out with the one saved for every logo
#
# The outline, title, colour and the brand's own source address are read from the Simple Icons release named in app-icons.json, so they are the set's
# words and not remembered ones. The width over height ("aspect") is worked out from the outline's bounding box, which is how wide it sits in its
# square. If the app has a picture file in Assets/AppLogos, that file is removed, since an app has an outline or a picture and never both.
param(
    [string]$Id,
    [string]$Slug,
    [switch]$Check
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$iconFile = Join-Path $root 'src\Clarion.Core\Catalog\Icons\app-icons.json'
$logoList = Join-Path $root 'src\Clarion.Core\Catalog\Icons\app-logo-files.json'
$logoDir = Join-Path $root 'src\Clarion.App\Assets\AppLogos'
$file = [IO.File]::ReadAllText($iconFile) | ConvertFrom-Json
$tag = ($file.set -replace '^Simple Icons ', '')

# The width over height of an outline, from the points of its path. Control points are included, so a curve that bulges is counted as wider than it
# is, which is close enough for telling a square mark from a wide word. The path is read one character at a time, because SVG lets numbers run
# together ("1.5.5", "-2-3") and lets the two flags of an arc sit side by side ("a1 1 0 011 1").
function Get-Aspect([string]$d) {
    $script:pos = 0; $script:d = $d
    function SkipSeparators { while ($script:pos -lt $script:d.Length -and ($script:d[$script:pos] -eq ' ' -or $script:d[$script:pos] -eq ',' -or $script:d[$script:pos] -eq "`t" -or $script:d[$script:pos] -eq "`n" -or $script:d[$script:pos] -eq "`r")) { $script:pos++ } }
    function ReadNumber {
        SkipSeparators
        $s = $script:pos
        if ($script:pos -lt $script:d.Length -and ($script:d[$script:pos] -eq '-' -or $script:d[$script:pos] -eq '+')) { $script:pos++ }
        while ($script:pos -lt $script:d.Length -and [char]::IsDigit($script:d[$script:pos])) { $script:pos++ }
        if ($script:pos -lt $script:d.Length -and $script:d[$script:pos] -eq '.') { $script:pos++; while ($script:pos -lt $script:d.Length -and [char]::IsDigit($script:d[$script:pos])) { $script:pos++ } }
        if ($script:pos -lt $script:d.Length -and ($script:d[$script:pos] -eq 'e' -or $script:d[$script:pos] -eq 'E')) {
            $script:pos++
            if ($script:pos -lt $script:d.Length -and ($script:d[$script:pos] -eq '-' -or $script:d[$script:pos] -eq '+')) { $script:pos++ }
            while ($script:pos -lt $script:d.Length -and [char]::IsDigit($script:d[$script:pos])) { $script:pos++ }
        }
        return [double]::Parse($script:d.Substring($s, $script:pos - $s), [Globalization.CultureInfo]::InvariantCulture)
    }
    function ReadFlag { SkipSeparators; $v = [int]$script:d[$script:pos] - 48; $script:pos++; return $v }
    function MoreNumbers { SkipSeparators; return $script:pos -lt $script:d.Length -and ($script:d[$script:pos] -match '[-+.0-9]') }

    $xs = [System.Collections.Generic.List[double]]::new(); $ys = [System.Collections.Generic.List[double]]::new()
    $cx = 0.0; $cy = 0.0; $sx = 0.0; $sy = 0.0; $cmd = ' '
    while ($true) {
        SkipSeparators
        if ($script:pos -ge $script:d.Length) { break }
        if ($script:d[$script:pos] -match '[A-Za-z]') { $cmd = $script:d[$script:pos]; $script:pos++ }
        $rel = [char]::IsLower($cmd); $up = ([string]$cmd).ToUpperInvariant()
        if ($up -eq 'Z') { $cx = $sx; $cy = $sy; continue }
        $first = $true
        while ($first -or (MoreNumbers)) {
            $first = $false
            switch ($up) {
                'M' { $nx = ReadNumber; $ny = ReadNumber; if ($rel) { $nx += $cx; $ny += $cy }; $cx = $nx; $cy = $ny; $sx = $cx; $sy = $cy; $xs.Add($cx); $ys.Add($cy); $up = 'L'; $rel = $rel }
                { $_ -in 'L', 'T' } { $nx = ReadNumber; $ny = ReadNumber; if ($rel) { $nx += $cx; $ny += $cy }; $cx = $nx; $cy = $ny; $xs.Add($cx); $ys.Add($cy) }
                'H' { $nx = ReadNumber; if ($rel) { $nx += $cx }; $cx = $nx; $xs.Add($cx); $ys.Add($cy) }
                'V' { $ny = ReadNumber; if ($rel) { $ny += $cy }; $cy = $ny; $xs.Add($cx); $ys.Add($cy) }
                'C' { for ($k = 0; $k -lt 3; $k++) { $px = ReadNumber; $py = ReadNumber; if ($rel) { $px += $cx; $py += $cy }; $xs.Add($px); $ys.Add($py); if ($k -eq 2) { $cx = $px; $cy = $py } } }
                { $_ -in 'S', 'Q' } { for ($k = 0; $k -lt 2; $k++) { $px = ReadNumber; $py = ReadNumber; if ($rel) { $px += $cx; $py += $cy }; $xs.Add($px); $ys.Add($py); if ($k -eq 1) { $cx = $px; $cy = $py } } }
                'A' { $null = ReadNumber; $null = ReadNumber; $null = ReadNumber; $null = ReadFlag; $null = ReadFlag; $nx = ReadNumber; $ny = ReadNumber; if ($rel) { $nx += $cx; $ny += $cy }; $cx = $nx; $cy = $ny; $xs.Add($cx); $ys.Add($cy) }
                default { throw "Unknown path command $cmd at $($script:pos)" }
            }
        }
    }
    $w = ($xs | Measure-Object -Maximum -Minimum); $h = ($ys | Measure-Object -Maximum -Minimum)
    $width = $w.Maximum - $w.Minimum; $height = $h.Maximum - $h.Minimum
    if ($height -le 0) { return 1.0 }
    return [Math]::Round($width / $height, 2)
}
if ($Check) {
    $worst = 0.0; $n = 0
    foreach ($p in $file.icons.PSObject.Properties) {
        $mine = Get-Aspect $p.Value.path; $saved = [double]$p.Value.aspect
        $diff = [Math]::Abs($mine - $saved); if ($diff -gt $worst) { $worst = $diff; $at = "$($p.Name): mine $mine, saved $saved" }
        $n++
    }
    "Compared $n logos. Largest difference $worst ($at)."
    return
}

if (-not $Id -or -not $Slug) { throw 'Give -Id and -Slug, or -Check.' }
if ($file.icons.PSObject.Properties.Name -contains $Id) { throw "$Id already has an outline." }

$svg = (Invoke-WebRequest -Uri "https://raw.githubusercontent.com/simple-icons/simple-icons/$tag/icons/$Slug.svg" -UseBasicParsing -TimeoutSec 30).Content
if ($svg -notmatch '<title>([^<]+)</title>' ) { throw "No title in $Slug.svg" }
$title = [System.Net.WebUtility]::HtmlDecode($Matches[1])
if ($svg -notmatch '<path d="([^"]+)"') { throw "No single path in $Slug.svg" }
$path = $Matches[1]
$data = (Invoke-WebRequest -Uri "https://raw.githubusercontent.com/simple-icons/simple-icons/$tag/data/simple-icons.json" -UseBasicParsing -TimeoutSec 60).Content | ConvertFrom-Json
$entry = @($data | Where-Object { $_.title -eq $title }) | Select-Object -First 1
if (-not $entry) { throw "No data entry titled $title" }

$text = [IO.File]::ReadAllText($iconFile)
$new = [ordered]@{ title = $title; hex = $entry.hex; source = $entry.source; aspect = (Get-Aspect $path); path = $path }
$icons = [ordered]@{}; foreach ($p in $file.icons.PSObject.Properties) { $icons[$p.Name] = $p.Value }
$icons[$Id] = $new
$sorted = [ordered]@{}; foreach ($k in ($icons.Keys | Sort-Object)) { $sorted[$k] = $icons[$k] }
$out = [ordered]@{ set = $file.set; setUrl = $file.setUrl; icons = $sorted }
$json = ($out | ConvertTo-Json -Depth 6) -replace "`r?`n", "`r`n"
[IO.File]::WriteAllText($iconFile, $json)

# An app has an outline or a picture, never both.
$png = Join-Path $logoDir "$Id.png"
if (Test-Path $png) { Remove-Item $png }
if (Test-Path $logoList) {
    $list = [IO.File]::ReadAllText($logoList) | ConvertFrom-Json
    if ($list.PSObject.Properties.Name -contains $Id) {
        $keep = [ordered]@{}; foreach ($p in $list.PSObject.Properties) { if ($p.Name -ne $Id) { $keep[$p.Name] = $p.Value } }
        [IO.File]::WriteAllText($logoList, (($keep | ConvertTo-Json -Depth 4) -replace "`r?`n", "`r`n"))
    }
}
"Added the $title outline for $Id (colour $($entry.hex), aspect $($new.aspect))."
