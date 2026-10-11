# Builds the app icon from the brand icon tile in docs/brand, so the file Windows reads and the picture shown in the app come from the
# same source. Sizes under 256 are written as classic 32 bit bitmaps, which every Windows tool reads, and 256 as a PNG, as Windows expects.
#
#   ./scripts/make-icon.ps1                       writes src/Clarion.App/Assets/Clarion.ico and Clarion.png, and docs/images/logo.png
#   ./scripts/make-icon.ps1 -OutDir some\dir      writes the same files under another folder, for a look first
param(
    [string]$Source = (Join-Path $PSScriptRoot '..\docs\brand\app-icon-1024.png'),
    [string]$OutDir
)
Add-Type -AssemblyName System.Drawing
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$assets = if ($OutDir) { Join-Path $OutDir 'Assets' } else { Join-Path $root 'src\Clarion.App\Assets' }
$images = if ($OutDir) { Join-Path $OutDir 'images' } else { Join-Path $root 'docs\images' }
New-Item -ItemType Directory -Force $assets, $images | Out-Null
$sizes = 16, 20, 24, 32, 40, 48, 64, 128, 256

$original = [System.Drawing.Image]::FromFile((Resolve-Path $Source))

function New-Scaled([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = 'HighQualityBicubic'; $g.SmoothingMode = 'HighQuality'; $g.PixelOffsetMode = 'HighQuality'; $g.CompositingMode = 'SourceCopy'
    $g.DrawImage($original, 0, 0, $size, $size); $g.Dispose()
    return $bmp
}

function Get-PngBytes($bmp) { $ms = New-Object IO.MemoryStream; $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png); return ,$ms.ToArray() }

# The bitmap header, the pixels from the bottom up, and an empty mask.
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
        $b = New-Scaled $s
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

Write-Ico $sizes (Join-Path $assets 'Clarion.ico')
$app = New-Scaled 512
$app.Save((Join-Path $assets 'Clarion.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$app.Save((Join-Path $images 'logo.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$app.Dispose(); $original.Dispose()
Write-Host "Wrote $assets\Clarion.ico, Clarion.png and $images\logo.png"
