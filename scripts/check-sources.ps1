# Checks that every source link in the catalog still resolves. Run before a release.
$root = Split-Path $PSScriptRoot -Parent
$urls = Select-String -Path "$root\src\Clarion.Core\Catalog\Data\*.json" -Pattern 'https://[^"]+' -AllMatches |
    ForEach-Object { $_.Matches.Value } | Sort-Object -Unique
$bad = 0
foreach ($u in $urls) {
    try {
        $r = Invoke-WebRequest -Uri $u -Method Head -MaximumRedirection 5 -TimeoutSec 20 -ErrorAction Stop
        Write-Host "$($r.StatusCode) $u"
    } catch {
        Write-Host "$($_.Exception.Response.StatusCode.value__) $u" -ForegroundColor Red
        $bad++
    }
}
if ($bad -gt 0) { exit 1 }
