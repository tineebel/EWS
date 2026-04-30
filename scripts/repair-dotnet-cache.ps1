param(
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path

Write-Host 'Stopping .NET build servers...'
dotnet build-server shutdown | Out-Host

$cacheFiles = Get-ChildItem -LiteralPath $root -Recurse -Force -Filter '*.AssemblyReference.cache' |
    Where-Object { $_.FullName -like (Join-Path $root '*') }

if ($cacheFiles.Count -eq 0) {
    Write-Host 'No AssemblyReference cache files found.'
    exit 0
}

Write-Host "Found $($cacheFiles.Count) AssemblyReference cache file(s)."

foreach ($file in $cacheFiles) {
    $relativePath = $file.FullName.Substring($root.Length + 1)
    if ($WhatIf) {
        Write-Host "[WhatIf] Remove $relativePath"
        continue
    }

    try {
        Remove-Item -LiteralPath $file.FullName -Force
        Write-Host "Removed $relativePath"
    }
    catch {
        Write-Warning "Could not remove ${relativePath}: $($_.Exception.Message)"
    }
}

Write-Host 'Done. Run start-dev.bat again.'
