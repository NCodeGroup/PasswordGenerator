# Copyright (c) 2026 NCode Group
# SPDX-License-Identifier: Apache-2.0

<#
.SYNOPSIS
    Test build/Test-PackageVersion.ps1 against package names made up for each case.

.DESCRIPTION
    Creates empty package files in a new folder under the system's temporary folder, runs build/Test-PackageVersion.ps1
    on each case and prints whether it passed or threw as the case expects. Throws when a case gives the wrong result,
    so the run exits non-zero; deletes the folder either way. The build job of .github/workflows/main.yml runs it on
    every event, so a mistake in the check shows before a release needs it.

.EXAMPLE
    ./build/Test-PackageVersion.SelfTest.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$check = Join-Path $PSScriptRoot 'Test-PackageVersion.ps1'
$id = 'NCode.PasswordGenerator'
$cases = @(
    @{ Name = 'match'; Tag = 'v1.2.3'; Packages = @("$id.1.2.3.nupkg", "$id.1.2.3.snupkg"); Passes = $true }
    @{ Name = 'pre-release tag'; Tag = 'v1.2.3-beta.1'; Packages = @("$id.1.2.3-beta.1.nupkg"); Passes = $true }
    @{ Name = 'mismatch'; Tag = 'v1.2.4'; Packages = @("$id.1.2.3.nupkg"); Passes = $false }
    @{
        Name = 'one of two mismatched'; Tag = 'v1.2.3'; Passes = $false
        Packages = @("$id.1.2.3.nupkg", "$id.Other.1.2.4.nupkg")
    }
    @{ Name = 'commit suffix on a release'; Tag = 'v1.2.3'; Packages = @("$id.1.2.3-g1a2b3c4d.nupkg"); Passes = $false }
    @{ Name = 'tag without v'; Tag = '1.2.3'; Packages = @("$id.1.2.3.nupkg"); Passes = $false }
    @{ Name = 'tag that is no version'; Tag = 'release-1'; Packages = @("$id.1.2.3.nupkg"); Passes = $false }
    @{ Name = 'no package'; Tag = 'v1.2.3'; Packages = @(); Passes = $false }
)

$root = Join-Path ([IO.Path]::GetTempPath()) "Test-PackageVersion-$([guid]::NewGuid().ToString('N'))"
$failures = 0
try {
    foreach ($case in $cases) {
        $folder = Join-Path $root ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $folder | Out-Null
        foreach ($package in $case.Packages) {
            New-Item -ItemType File -Path (Join-Path $folder $package) | Out-Null
        }

        $passed = $true
        try {
            & $check -Tag $case.Tag -Path (Join-Path $folder '*.nupkg') | Out-Null
        }
        catch {
            $passed = $false
        }

        if ($passed -eq $case.Passes) {
            "ok: $($case.Name)"
        }
        else {
            $failures++
            "FAILED: $($case.Name): expected the check to $(if ($case.Passes) { 'pass' } else { 'throw' })"
        }
    }
}
finally {
    Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
}

if ($failures) {
    throw "$failures of $($cases.Count) cases gave the wrong result."
}
