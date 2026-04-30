param(
    [string]$JsonPath = "DocSF\ALLEm.json",
    [string]$AppSettingsPath = "src\EWS.API\appsettings.json",
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$jsonFullPath = (Resolve-Path -LiteralPath (Join-Path $root $JsonPath)).Path
$settingsFullPath = (Resolve-Path -LiteralPath (Join-Path $root $AppSettingsPath)).Path

function Get-SingleShortCodeMap {
    param(
        [object[]]$Rows,
        [string]$CodeField,
        [string]$ShortCodeField
    )

    $map = @{}
    $conflicts = @()

    $Rows |
        Where-Object { $_.$CodeField -and $_.$ShortCodeField -and $_.$ShortCodeField -ne '-' } |
        Group-Object $CodeField |
        ForEach-Object {
            $shortCodes = @($_.Group | Select-Object -ExpandProperty $ShortCodeField -Unique)
            if ($shortCodes.Count -eq 1) {
                $map[$_.Name] = $shortCodes[0]
            }
            else {
                $conflicts += [pscustomobject]@{
                    Code = $_.Name
                    ShortCodes = ($shortCodes -join ', ')
                    Count = $_.Count
                }
            }
        }

    [pscustomobject]@{
        Map = $map
        Conflicts = $conflicts
    }
}

Write-Host "Reading $jsonFullPath"
$payload = Get-Content -Raw -Encoding UTF8 -LiteralPath $jsonFullPath | ConvertFrom-Json
$employees = @($payload.empList.emp)

$deptResult = Get-SingleShortCodeMap -Rows $employees -CodeField 'empDeptCode' -ShortCodeField 'empDeptShortName'
$sectResult = Get-SingleShortCodeMap -Rows $employees -CodeField 'empSectCode' -ShortCodeField 'empSectShortName'

$settings = Get-Content -Raw -Encoding UTF8 -LiteralPath $settingsFullPath | ConvertFrom-Json
$connectionString = $settings.ConnectionStrings.DefaultConnection

Add-Type -AssemblyName System.Data
$connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
$connection.Open()

try {
    $deptUpdated = 0
    foreach ($code in $deptResult.Map.Keys) {
        if ($WhatIf) {
            Write-Host "[WhatIf] Department $code -> $($deptResult.Map[$code])"
            continue
        }

        $command = $connection.CreateCommand()
        $command.CommandText = "UPDATE dbo.Departments SET DeptShortCode = @ShortCode WHERE DeptCode = @Code"
        $null = $command.Parameters.Add("@ShortCode", [System.Data.SqlDbType]::NVarChar, 20)
        $null = $command.Parameters.Add("@Code", [System.Data.SqlDbType]::NVarChar, 20)
        $command.Parameters["@ShortCode"].Value = $deptResult.Map[$code]
        $command.Parameters["@Code"].Value = $code
        $deptUpdated += $command.ExecuteNonQuery()
    }

    $sectionUpdated = 0
    foreach ($code in $sectResult.Map.Keys) {
        if ($WhatIf) {
            Write-Host "[WhatIf] Section $code -> $($sectResult.Map[$code])"
            continue
        }

        $command = $connection.CreateCommand()
        $command.CommandText = "UPDATE dbo.Sections SET SectShortCode = @ShortCode WHERE SectCode = @Code"
        $null = $command.Parameters.Add("@ShortCode", [System.Data.SqlDbType]::NVarChar, 20)
        $null = $command.Parameters.Add("@Code", [System.Data.SqlDbType]::NVarChar, 20)
        $command.Parameters["@ShortCode"].Value = $sectResult.Map[$code]
        $command.Parameters["@Code"].Value = $code
        $sectionUpdated += $command.ExecuteNonQuery()
    }
}
finally {
    $connection.Close()
}

Write-Host "Employees read: $($employees.Count)"
Write-Host "Department mappings: $($deptResult.Map.Count)"
Write-Host "Section mappings: $($sectResult.Map.Count)"
Write-Host "Department conflicts skipped: $($deptResult.Conflicts.Count)"
Write-Host "Section conflicts skipped: $($sectResult.Conflicts.Count)"

if (-not $WhatIf) {
    Write-Host "Departments updated: $deptUpdated"
    Write-Host "Sections updated: $sectionUpdated"
}

if ($deptResult.Conflicts.Count -gt 0) {
    Write-Host ''
    Write-Warning 'Department short-code conflicts were skipped:'
    $deptResult.Conflicts | Format-Table -AutoSize
}

if ($sectResult.Conflicts.Count -gt 0) {
    Write-Host ''
    Write-Warning 'Section short-code conflicts were skipped:'
    $sectResult.Conflicts | Format-Table -AutoSize
}
