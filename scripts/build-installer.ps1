# Publishes the app and builds the setup program and zip, with checksums, into artifacts.
param([string]$Version)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
if (-not $Version) {
    $Version = ([xml](Get-Content (Join-Path $root 'Directory.Build.props'))).Project.PropertyGroup.Version
}
$out = Join-Path $root 'artifacts'
$pub = Join-Path $out 'publish'
if (Test-Path $pub) { [IO.Directory]::Delete($pub, $true) }

dotnet publish (Join-Path $root 'src\Clarion.App') -c Release -p:Platform=x64 -r win-x64 --self-contained -o $pub
if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }

$iscc = @("${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe", "$env:ProgramFiles\Inno Setup 6\ISCC.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw 'The setup compiler was not found. Install Inno Setup 6.' }
& $iscc "/DAppVersion=$Version" "/DSourceDir=$pub" (Join-Path $root 'installer\Clarion.iss')
if ($LASTEXITCODE -ne 0) { throw 'Setup build failed.' }

$zip = Join-Path $out "Clarion-$Version-win-x64.zip"
if (Test-Path $zip) { [IO.File]::Delete($zip) }
Compress-Archive -Path (Join-Path $pub '*') -DestinationPath $zip -CompressionLevel Optimal

foreach ($f in @($zip, (Join-Path $out "Clarion-$Version-setup.exe"))) {
    $h = (Get-FileHash $f -Algorithm SHA256).Hash.ToLower()
    Set-Content "$f.sha256" "$h  $(Split-Path $f -Leaf)"
    '{0}  {1:N1} MB' -f (Split-Path $f -Leaf), ((Get-Item $f).Length / 1MB)
}
