# Finds a real logo for each app in the catalog that has no Simple Icons outline, and saves it into the app, so nothing is fetched while it runs.
#
#   ./scripts/fetch-app-logos.ps1              adds the logos that are missing
#   ./scripts/fetch-app-logos.ps1 -Id app.rufus -Force    looks again for one app
#
# Apps that have a Simple Icons outline are left alone (scripts/add-simple-icon.ps1 adds those). For each of the others, in this order:
#   1. The logo named for it in scripts/app-logo-sources.json, from one of two curated collections that publish PNG files, picked by hand
#      because a name that only looks alike (Flux and f.lux) would be the wrong program. The commit is written into the address.
#   2. The picture of the project's GitHub organization, when the app's links name one (not a person's account, and not the microsoft organization,
#      whose picture is the Microsoft logo and not the app's).
#   3. The large icon the app's own web page names: an apple-touch-icon, or the biggest PNG named by a rel="icon" link.
#   4. The biggest frame of the page's favicon.ico.
# Never used: GitHub's own icon (the page of a repository names it), and the icons of sites that serve a whole company's products (Microsoft, Amazon Web
# Services and similar), since they show the wrong logo. An app listed in scripts/app-logo-rejected.json is skipped, with the reason written there.
# A picture is kept only when it is at least 48 pixels on each side and close to square, and it is never made bigger, so a blurry 16 pixel favicon is not
# stretched. An app with no good picture keeps the letter tile. It is written down where each one came from, in app-logo-files.json, which the
# About page credits and a test checks. Run it by hand, look at what it saved, then commit. It is not part of the build.
param(
    [string]$Id,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$logoDir = Join-Path $root 'src\Clarion.App\Assets\AppLogos'
$listFile = Join-Path $root 'src\Clarion.Core\Catalog\Icons\app-logo-files.json'
$vectorFile = Join-Path $root 'src\Clarion.Core\Catalog\Icons\app-icons.json'
New-Item -ItemType Directory -Force $logoDir | Out-Null

$agent = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) ClarionLogoFetch'
$token = $env:GITHUB_TOKEN
if (-not $token -and (Get-Command gh -ErrorAction SilentlyContinue)) { $token = (& gh auth token) 2>$null }

$apps = [IO.File]::ReadAllText((Join-Path $root 'src\Clarion.Core\Catalog\Data\apps.json')) | ConvertFrom-Json
$vectors = ([IO.File]::ReadAllText($vectorFile) | ConvertFrom-Json).icons.PSObject.Properties.Name
$rejected = @{}
$rejectFile = Join-Path $PSScriptRoot 'app-logo-rejected.json'
if (Test-Path $rejectFile) { foreach ($p in ([IO.File]::ReadAllText($rejectFile) | ConvertFrom-Json).PSObject.Properties) { if ($p.Name -notlike '_*') { $rejected[$p.Name] = $p.Value } } }
$sourcesFile = Join-Path $PSScriptRoot 'app-logo-sources.json'
$named = @{}
if (Test-Path $sourcesFile) { foreach ($p in ([IO.File]::ReadAllText($sourcesFile) | ConvertFrom-Json).PSObject.Properties) { if ($p.Name -notlike '_*') { $named[$p.Name] = $p.Value } } }
$collections = @{
    'dashboard-icons' = @{ Repo = 'homarr-labs/dashboard-icons'; Folder = 'png' }
    'selfhst'         = @{ Repo = 'selfhst/icons'; Folder = 'png' }
}
$commits = @{}
function Get-Commit([string]$repo) {
    if (-not $script:commits.ContainsKey($repo)) {
        $headers = @{ 'User-Agent' = $agent }
        if ($token) { $headers['Authorization'] = "Bearer $token" }
        $script:commits[$repo] = (Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/commits/HEAD" -Headers $headers -TimeoutSec 25).sha
    }
    return $script:commits[$repo]
}
# Pages and files that show the wrong logo: GitHub's own, and company wide ones.
function Test-GenericIcon([string]$Url) {
    if ($Url -match '^https://github\.com/[^/]+\.png') { return $false }
    return $Url -match '^https://(www\.)?github\.com/' -or $Url -match '^https://(learn|www|visualstudio|docs)\.microsoft\.com/' -or $Url -match '^https://(www\.)?microsoft\.com/' -or $Url -match '^https://visualstudio\.microsoft\.com/' -or $Url -match 'docs\.aws\.amazon\.com'
}
$known = [ordered]@{}
if (Test-Path $listFile) { foreach ($p in ([IO.File]::ReadAllText($listFile) | ConvertFrom-Json).PSObject.Properties) { $known[$p.Name] = $p.Value } }

function Get-Bytes([string]$Url) {
    $r = Invoke-WebRequest -Uri $Url -UserAgent $agent -UseBasicParsing -TimeoutSec 25 -MaximumRedirection 5
    return , $r.RawContentStream.ToArray()
}

# Opens image bytes as a bitmap. Returns $null when they are not a picture System.Drawing can read.
function Open-Image([byte[]]$Bytes) {
    try { return [System.Drawing.Image]::FromStream([IO.MemoryStream]::new($Bytes)) } catch { return $null }
}

# The biggest picture inside an ICO file, as a bitmap, or $null.
function Open-IcoBest([byte[]]$Bytes) {
    if ($Bytes.Length -lt 22 -or $Bytes[0] -ne 0 -or $Bytes[1] -ne 0 -or $Bytes[2] -ne 1) { return $null }
    $count = [BitConverter]::ToUInt16($Bytes, 4)
    $best = $null
    for ($i = 0; $i -lt $count; $i++) {
        $o = 6 + 16 * $i
        $w = if ($Bytes[$o] -eq 0) { 256 } else { [int]$Bytes[$o] }
        $size = [BitConverter]::ToUInt32($Bytes, $o + 8); $offset = [BitConverter]::ToUInt32($Bytes, $o + 12)
        if ($offset + $size -gt $Bytes.Length) { continue }
        if ($null -eq $best -or $w -gt $best.W) { $best = @{ W = $w; Offset = $offset; Size = $size } }
    }
    if ($null -eq $best) { return $null }
    $frame = $Bytes[$best.Offset..($best.Offset + $best.Size - 1)]
    if ($frame.Length -gt 8 -and $frame[1] -eq 0x50 -and $frame[2] -eq 0x4E -and $frame[3] -eq 0x47) { return Open-Image ([byte[]]$frame) }
    try { return ([System.Drawing.Icon]::new([IO.MemoryStream]::new($Bytes), $best.W, $best.W)).ToBitmap() } catch { return $null }
}

# True when the picture is big enough and close to square to be a logo.
# True when the picture has something in it: at least 3 percent of the sampled pixels differ clearly from the first one, so a blank square or a
# fully see through one is not taken for a logo.
function Test-HasContent($Image) {
    $bmp = [System.Drawing.Bitmap]::new($Image)
    try {
        $ref = $bmp.GetPixel(0, 0); $different = 0; $total = 0
        $stepX = [Math]::Max(1, [int]($bmp.Width / 32)); $stepY = [Math]::Max(1, [int]($bmp.Height / 32))
        for ($y = 0; $y -lt $bmp.Height; $y += $stepY) {
            for ($x = 0; $x -lt $bmp.Width; $x += $stepX) {
                $p = $bmp.GetPixel($x, $y); $total++
                if ([Math]::Abs($p.A - $ref.A) -gt 40 -or ($p.A -gt 40 -and ([Math]::Abs($p.R - $ref.R) + [Math]::Abs($p.G - $ref.G) + [Math]::Abs($p.B - $ref.B)) -gt 60)) { $different++ }
            }
        }
        return $total -gt 0 -and ($different / $total) -ge 0.03
    } finally { $bmp.Dispose() }
}

# True when the picture is big enough, close to square and not blank, to be a logo.
function Test-Logo($Image) { return $null -ne $Image -and $Image.Width -ge 48 -and $Image.Height -ge 48 -and ($Image.Width / $Image.Height) -ge 0.8 -and ($Image.Width / $Image.Height) -le 1.25 -and (Test-HasContent $Image) }

# How much of the picture is see through, and how bright the solid part is, 0 for black to 1 for white.
function Measure-Logo([System.Drawing.Bitmap]$Bmp) {
    $clear = 0; $solid = 0; $lum = 0.0
    for ($y = 0; $y -lt $Bmp.Height; $y += 2) {
        for ($x = 0; $x -lt $Bmp.Width; $x += 2) {
            $p = $Bmp.GetPixel($x, $y)
            if ($p.A -lt 32) { $clear++ }
            elseif ($p.A -ge 128) { $solid++; $lum += (0.2126 * $p.R + 0.7152 * $p.G + 0.0722 * $p.B) / 255 }
        }
    }
    $total = [Math]::Max(1, $clear + $solid)
    return @{ Clear = $clear / $total; Luminance = $(if ($solid -gt 0) { $lum / $solid } else { 0.5 }) }
}

# Saves the picture at 128 pixels at most, never bigger than it was. A picture with a see through background goes on a plate, white for dark
# artwork and dark for pale artwork, so it reads the same on a light card and a dark one. Returns "white", "dark" or "none".
function Save-Logo($Image, [string]$Path) {
    $side = [Math]::Min(128, [Math]::Max($Image.Width, $Image.Height))
    $bmp = [System.Drawing.Bitmap]::new($side, $side, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)
    $scale = $side / [Math]::Max($Image.Width, $Image.Height)
    $w = [int][Math]::Round($Image.Width * $scale); $h = [int][Math]::Round($Image.Height * $scale)
    $g.DrawImage($Image, [int](($side - $w) / 2), [int](($side - $h) / 2), $w, $h)
    $g.Dispose()

    $plate = 'none'
    $m = Measure-Logo $bmp
    if ($m.Clear -ge 0.12) {
        $plate = if ($m.Luminance -ge 0.6) { 'dark' } else { 'white' }
        $color = if ($plate -eq 'dark') { [System.Drawing.Color]::FromArgb(255, 31, 38, 51) } else { [System.Drawing.Color]::FromArgb(255, 255, 255, 255) }
        $out = [System.Drawing.Bitmap]::new($side, $side, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g2 = [System.Drawing.Graphics]::FromImage($out)
        $g2.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $g2.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g2.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $g2.Clear([System.Drawing.Color]::Transparent)
        $r = [int]($side * 0.22); $d = $r * 2
        $shape = [System.Drawing.Drawing2D.GraphicsPath]::new()
        $shape.AddArc(0, 0, $d, $d, 180, 90); $shape.AddArc($side - $d - 1, 0, $d, $d, 270, 90)
        $shape.AddArc($side - $d - 1, $side - $d - 1, $d, $d, 0, 90); $shape.AddArc(0, $side - $d - 1, $d, $d, 90, 90); $shape.CloseFigure()
        $g2.FillPath([System.Drawing.SolidBrush]::new($color), $shape)
        $inset = [int]($side * 0.14); $inner = $side - 2 * $inset
        $g2.DrawImage($bmp, $inset, $inset, $inner, $inner)
        $g2.Dispose(); $shape.Dispose(); $bmp.Dispose(); $bmp = $out
    }
    $bmp.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    return $plate
}
function Get-Pages($app) {
    # The app's own pages: every link in its sources that is not the package folder in winget-pkgs or a licence text.
    $urls = @($app.sources | Where-Object { $_ -match '^https://' -and $_ -notmatch 'winget-pkgs' -and $_ -notmatch '(?i)licen[sc]e|/legal|/terms|/eula|/privacy|\.txt$' })
    return $urls
}

function Find-GitHubOrg($app) {
    foreach ($u in $app.sources) {
        if ($u -match '^https://github\.com/([A-Za-z0-9\-_.]+)/' -and $Matches[1] -ne 'microsoft') {
            $owner = $Matches[1]
            if ($owner -eq 'microsoft') { continue }
            $headers = @{ 'User-Agent' = $agent; 'Accept' = 'application/vnd.github+json' }
            if ($token) { $headers['Authorization'] = "Bearer $token" }
            try { $info = Invoke-RestMethod -Uri "https://api.github.com/users/$owner" -Headers $headers -TimeoutSec 25 } catch { continue }
            if ($info.type -eq 'Organization') { return @{ Owner = $owner; Url = "https://github.com/$owner.png?size=128" } }
        }
    }
    return $null
}

function Find-PageIcons([string]$PageUrl) {
    $html = (Invoke-WebRequest -Uri $PageUrl -UserAgent $agent -UseBasicParsing -TimeoutSec 25 -MaximumRedirection 5)
    $base = [Uri]$html.BaseResponse.ResponseUri.AbsoluteUri
    $found = @()
    foreach ($m in [regex]::Matches($html.Content, '<link\b[^>]*>', 'IgnoreCase')) {
        $tag = $m.Value
        if ($tag -notmatch 'rel\s*=\s*["'']([^"'']*)["'']') { continue }
        $rel = $Matches[1].ToLowerInvariant()
        if ($rel -notmatch 'icon') { continue }
        if ($tag -notmatch 'href\s*=\s*["'']([^"'']+)["'']') { continue }
        $href = $Matches[1]
        if ($href -match '^data:') { continue }
        $size = 0
        if ($tag -match 'sizes\s*=\s*["'']([0-9]+)x[0-9]+') { $size = [int]$Matches[1] }
        $isSvg = $href -match '\.svg(\?|$)' -or $tag -match 'image/svg'
        if ($isSvg) { continue }
        $weight = if ($rel -match 'apple-touch-icon') { 1000 + $size } else { $size }
        try { $found += @{ Url = ([Uri]::new($base, $href)).AbsoluteUri; Weight = $weight } } catch { }
    }
    $found += @{ Url = ([Uri]::new($base, '/favicon.ico')).AbsoluteUri; Weight = 1 }
    return @($found | Sort-Object { - $_.Weight })
}

$changed = 0; $got = 0; $none = [System.Collections.Generic.List[string]]::new()
foreach ($app in $apps) {
    if ($Id -and $app.id -ne $Id) { continue }
    if ($vectors -contains $app.id) { continue }
    if ($rejected.ContainsKey($app.id)) { continue }
    $file = Join-Path $logoDir "$($app.id).png"
    if ((Test-Path $file) -and -not $Force) { continue }

    $saved = $false; $source = ''
    $org = $null
    try { $org = Find-GitHubOrg $app } catch { }
    $candidates = [System.Collections.Generic.List[hashtable]]::new()
    if ($named.ContainsKey($app.id)) {
        $n = $named[$app.id]; $col = $collections[[string]$n.collection]
        if ($col) { $candidates.Add(@{ Url = "https://raw.githubusercontent.com/$($col.Repo)/$(Get-Commit $col.Repo)/$($col.Folder)/$($n.name).png"; Why = "named in app-logo-sources.json"; Ico = $false }) }
    }
    if ($org) { $candidates.Add(@{ Url = $org.Url; Why = "the picture of the GitHub organization $($org.Owner)"; Ico = $false }) }
    foreach ($page in (Get-Pages $app)) {
        try { foreach ($c in (Find-PageIcons $page)) { $candidates.Add(@{ Url = $c.Url; Why = "the icon named by $page"; Ico = $c.Url -match '\.ico(\?|$)' }) } } catch { }
    }
    foreach ($c in $candidates) {
        if (Test-GenericIcon $c.Url) { continue }
        try {
            $bytes = Get-Bytes $c.Url
            $img = if ($c.Ico) { Open-IcoBest $bytes } else { Open-Image $bytes }
            if ($null -eq $img -and -not $c.Ico) { $img = Open-IcoBest $bytes }
            if (Test-Logo $img) {
                $plate = Save-Logo $img $file
                $source = $c.Url
                $known[$app.id] = [ordered]@{ source = $c.Url; width = [Math]::Min(128, [Math]::Max($img.Width, $img.Height)); original = "$($img.Width)x$($img.Height)"; plate = $plate; sha256 = (Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant() }
                $img.Dispose(); $saved = $true; break
            }
            if ($img) { $img.Dispose() }
        } catch { }
    }
    if ($saved) { $got++; Write-Host "ok    $($app.id)  $source" } else { $none.Add($app.id); Write-Host "none  $($app.id)" }
    $changed++
}

# Sorted, so the file does not change between runs unless a logo does.
$sorted = [ordered]@{}
foreach ($k in ($known.Keys | Sort-Object)) { $sorted[$k] = $known[$k] }
$json = ($sorted | ConvertTo-Json -Depth 4) -replace "`r?`n", "`r`n"
[IO.File]::WriteAllText($listFile, $json)
Write-Host ""
Write-Host "Looked at $changed apps. Saved $got. Still without a logo: $($none.Count)."
if ($none.Count -gt 0) { Write-Host ($none -join ', ') }
