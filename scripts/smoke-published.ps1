# Starts the published program, the one that goes into the setup file and the zip, and checks that its command line answers.
# The tests start the built program, which is not the same files: this catches a publish that drops a file the program needs.
param([Parameter(Mandatory)][string]$PublishDir, [Parameter(Mandatory)][string]$Version)
$ErrorActionPreference = 'Stop'
$exe = Join-Path (Resolve-Path $PublishDir).Path 'Clarion.exe'
if (-not (Test-Path $exe)) { throw "Clarion.exe was not found in $PublishDir." }
$data = Join-Path ([IO.Path]::GetTempPath()) ('clarion-smoke-' + [guid]::NewGuid().ToString('N'))
$env:CLARION_DATA_DIR = $data
$env:CLARION_NO_ELEVATE = '1'
try {
    foreach ($check in @(
        @{ Args = '--version'; Expect = $Version },
        @{ Args = '--list-presets'; Expect = 'minimal' },
        @{ Args = '--list-settings'; Expect = 'privacy.' }
    )) {
        $out = Join-Path $data 'out.txt'; $err = Join-Path $data 'err.txt'
        New-Item -ItemType Directory -Force $data | Out-Null
        $p = Start-Process -FilePath $exe -ArgumentList $check.Args -RedirectStandardOutput $out -RedirectStandardError $err -Wait -PassThru -WindowStyle Hidden
        $text = (Get-Content $out -Raw) + (Get-Content $err -Raw)
        if ($p.ExitCode -ne 0) { throw "Clarion.exe $($check.Args) exited with $($p.ExitCode): $text" }
        if ($text -notmatch [regex]::Escape($check.Expect)) { throw "Clarion.exe $($check.Args) did not print '$($check.Expect)': $text" }
        Write-Host "ok  Clarion.exe $($check.Args)"
    }
}
finally {
    Remove-Item $data -Recurse -Force -ErrorAction SilentlyContinue
}
