# Draws the Clarion mark from geometry and writes the app icon, the logo and the banner.
# The mark is a bold C in steel blue (#5C8BCF) with one cyan dot (#3FD0E0) inside it, flat, on the dark tile (#0F141B).
# Nothing is traced from a picture, so every size is drawn crisp. Small sizes use a heavier C so it still reads at 16 pixels.
#
#   ./scripts/make-brand-assets.ps1                    writes into the repository
#   ./scripts/make-brand-assets.ps1 -OutDir some\dir   writes the same files under another folder, for a look first
param([string]$OutDir)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
if (-not $OutDir) { $OutDir = $root }
$assets = Join-Path $OutDir 'src\Clarion.App\Assets'
$images = Join-Path $OutDir 'docs\images'
New-Item -ItemType Directory -Force $assets, $images | Out-Null

$blue = [System.Drawing.Color]::FromArgb(255, 0x5C, 0x8B, 0xCF)
$cyan = [System.Drawing.Color]::FromArgb(255, 0x3F, 0xD0, 0xE0)
$tile = [System.Drawing.Color]::FromArgb(255, 0x0F, 0x14, 0x1B)
$silver = [System.Drawing.Color]::FromArgb(255, 0xA7, 0xB3, 0xC4)
$white = [System.Drawing.Color]::FromArgb(255, 0xE6, 0xEB, 0xF2)

# The mark in fractions of the tile, by how big it will be seen.
function Get-Geometry([int]$size) {
    if ($size -le 24) { return @{ Cx = 0.475; Rout = 0.385; Stroke = 0.185; Dot = 0.135; Gap = 40.0 } }
    if ($size -le 48) { return @{ Cx = 0.480; Rout = 0.350; Stroke = 0.150; Dot = 0.122; Gap = 41.0 } }
    return @{ Cx = 0.485; Rout = 0.322; Stroke = 0.126; Dot = 0.112; Gap = 42.0 }
}

function Add-RoundedRect($path, [single]$x, [single]$y, [single]$w, [single]$h, [single]$r) {
    $d = $r * 2
    $path.AddArc($x, $y, $d, $d, 180, 90); $path.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $path.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90); $path.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $path.CloseFigure()
}

# The C is a ring with the opening on the right, cut along the radius at both ends.
function Draw-Mark($g, [single]$cx, [single]$cy, [single]$rOut, [single]$stroke, [single]$dotR, [double]$gap) {
    $rIn = $rOut - $stroke
    $sweep = 360 - 2 * $gap
    $ring = New-Object System.Drawing.Drawing2D.GraphicsPath
    $ring.AddArc($cx - $rOut, $cy - $rOut, 2 * $rOut, 2 * $rOut, [single]$gap, [single]$sweep)
    $ring.AddArc($cx - $rIn, $cy - $rIn, 2 * $rIn, 2 * $rIn, [single](360 - $gap), [single](-$sweep))
    $ring.CloseFigure()
    $b = New-Object System.Drawing.SolidBrush $blue
    $g.FillPath($b, $ring); $b.Dispose(); $ring.Dispose()
    $d = New-Object System.Drawing.SolidBrush $cyan
    $g.FillEllipse($d, $cx - $dotR, $cy - $dotR, 2 * $dotR, 2 * $dotR); $d.Dispose()
}

# One tile at the wanted size. It is drawn at eight times the size and brought down, which is what makes the edges clean.
function New-Tile([int]$size, [bool]$rounded = $true) {
    $s = 8
    $big = New-Object System.Drawing.Bitmap ($size * $s), ($size * $s), ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($big)
    $g.SmoothingMode = 'AntiAlias'; $g.PixelOffsetMode = 'HighQuality'; $g.Clear([System.Drawing.Color]::Transparent)
    $T = [single]($size * $s)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    if ($rounded) { Add-RoundedRect $path 0 0 $T $T ($T * 0.2237) } else { $path.AddRectangle((New-Object System.Drawing.RectangleF 0, 0, $T, $T)) }
    $bg = New-Object System.Drawing.SolidBrush $tile
    $g.FillPath($bg, $path); $bg.Dispose(); $path.Dispose()
    $geo = Get-Geometry $size
    Draw-Mark $g ($T * $geo.Cx) ($T / 2) ($T * $geo.Rout) ($T * $geo.Stroke) ($T * $geo.Dot) $geo.Gap
    $g.Dispose()
    $out = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $og = [System.Drawing.Graphics]::FromImage($out)
    $og.InterpolationMode = 'HighQualityBicubic'; $og.PixelOffsetMode = 'HighQuality'; $og.SmoothingMode = 'HighQuality'; $og.Clear([System.Drawing.Color]::Transparent)
    $attr = New-Object System.Drawing.Imaging.ImageAttributes
    $attr.SetWrapMode([System.Drawing.Drawing2D.WrapMode]::TileFlipXY)
    $og.DrawImage($big, (New-Object System.Drawing.Rectangle 0, 0, $size, $size), 0, 0, $big.Width, $big.Height, [System.Drawing.GraphicsUnit]::Pixel, $attr)
    $og.Dispose(); $big.Dispose(); $attr.Dispose()
    return $out
}

function Save-Png($bmp, [string]$path) { $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png) }

function Get-PngBytes($bmp) { $ms = New-Object IO.MemoryStream; $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png); return ,$ms.ToArray() }

# A classic 32 bit icon image: the bitmap header, the pixels from the bottom up, and an empty mask. Sizes under 256 use this form,
# which every Windows tool reads. The 256 size is a PNG, as Windows expects.
function Get-DibBytes($bmp) {
    $w = $bmp.Width; $h = $bmp.Height
    $ms = New-Object IO.MemoryStream; $bw = New-Object IO.BinaryWriter $ms
    $bw.Write([int]40); $bw.Write([int]$w); $bw.Write([int]($h * 2)); $bw.Write([int16]1); $bw.Write([int16]32); $bw.Write([int]0)
    $bw.Write([int]($w * $h * 4)); $bw.Write([int]0); $bw.Write([int]0); $bw.Write([int]0); $bw.Write([int]0)
    for ($y = $h - 1; $y -ge 0; $y--) { for ($x = 0; $x -lt $w; $x++) {
        $c = $bmp.GetPixel($x, $y); $bw.Write([byte]$c.B); $bw.Write([byte]$c.G); $bw.Write([byte]$c.R); $bw.Write([byte]$c.A) } }
    $maskRow = [int]([math]::Ceiling($w / 32.0) * 4)
    $bw.Write((New-Object byte[] ($maskRow * $h)))
    $bw.Flush(); return ,$ms.ToArray()
}

function Write-Ico([int[]]$sizes, [string]$path) {
    $entries = foreach ($s in $sizes) {
        $b = New-Tile $s
        $data = if ($s -ge 256) { Get-PngBytes $b } else { Get-DibBytes $b }
        $b.Dispose()
        [pscustomobject]@{ Size = $s; Data = [byte[]]$data }
    }
    $ms = New-Object IO.MemoryStream; $bw = New-Object IO.BinaryWriter $ms
    $bw.Write([int16]0); $bw.Write([int16]1); $bw.Write([int16]$entries.Count)
    $offset = 6 + 16 * $entries.Count
    foreach ($e in $entries) {
        $dim = if ($e.Size -ge 256) { 0 } else { $e.Size }
        $bw.Write([byte]$dim); $bw.Write([byte]$dim); $bw.Write([byte]0); $bw.Write([byte]0)
        $bw.Write([int16]1); $bw.Write([int16]32); $bw.Write([int]$e.Data.Length); $bw.Write([int]$offset)
        $offset += $e.Data.Length
    }
    foreach ($e in $entries) { $bw.Write([byte[]]$e.Data) }
    $bw.Flush(); [IO.File]::WriteAllBytes($path, $ms.ToArray())
}

# The icon, the in-app image and the logo
Write-Ico @(16, 20, 24, 32, 40, 48, 64, 128, 256) (Join-Path $assets 'Clarion.ico')
$app = New-Tile 512; Save-Png $app (Join-Path $assets 'Clarion.png'); Save-Png $app (Join-Path $images 'logo.png'); $app.Dispose()

# The banner: the mark straight on the dark ground, the name, the tagline, and the cyan bar. 2400 by 800 for sharp screens.
$bw = 2400; $bh = 800; $s = 2
$big = New-Object System.Drawing.Bitmap ($bw * $s), ($bh * $s), ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($big)
$g.SmoothingMode = 'AntiAlias'; $g.TextRenderingHint = 'AntiAliasGridFit'; $g.PixelOffsetMode = 'HighQuality'
$g.Clear($tile)
$geo = Get-Geometry 512
$markSize = 760 * $s
Draw-Mark $g (430 * $s) (400 * $s) ($markSize * $geo.Rout) ($markSize * $geo.Stroke) ($markSize * $geo.Dot) $geo.Gap
$nameFont = New-Object System.Drawing.Font 'Segoe UI Semibold', (250 * $s), ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$tagFont = New-Object System.Drawing.Font 'Segoe UI', (92 * $s), ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$fmt = New-Object System.Drawing.StringFormat; $fmt.FormatFlags = 'NoWrap'
$wb = New-Object System.Drawing.SolidBrush $white; $sb = New-Object System.Drawing.SolidBrush $silver; $cb = New-Object System.Drawing.SolidBrush $cyan
$g.DrawString('Clarion', $nameFont, $wb, (820 * $s), (165 * $s), $fmt)
$g.DrawString('See it. Set it. Done.', $tagFont, $sb, (836 * $s), (460 * $s), $fmt)
$g.FillRectangle($cb, (856 * $s), (610 * $s), (480 * $s), (16 * $s))
$g.Dispose()
$out = New-Object System.Drawing.Bitmap $bw, $bh, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$og = [System.Drawing.Graphics]::FromImage($out)
$og.InterpolationMode = 'HighQualityBicubic'; $og.PixelOffsetMode = 'HighQuality'
$attr = New-Object System.Drawing.Imaging.ImageAttributes; $attr.SetWrapMode([System.Drawing.Drawing2D.WrapMode]::TileFlipXY)
$og.DrawImage($big, (New-Object System.Drawing.Rectangle 0, 0, $bw, $bh), 0, 0, $big.Width, $big.Height, [System.Drawing.GraphicsUnit]::Pixel, $attr)
$og.Dispose(); Save-Png $out (Join-Path $images 'banner.png'); $out.Dispose(); $big.Dispose()

Write-Host "Wrote $assets\Clarion.ico, Clarion.png and $images\logo.png, banner.png"
