# Copyright (c) 2026 NCode Group
# SPDX-License-Identifier: Apache-2.0

<#
.SYNOPSIS
    Fail when a package doesn't have the version a release tag names.

.DESCRIPTION
    The build job of .github/workflows/main.yml runs this on a release, after it packs: version.json sets the packages'
    version, so a tag that names another version would publish packages with the wrong one. Throws when the tag isn't
    a version such as v1.2.3 or v1.2.3-beta.1, when no package matches -Path, or when a package's file name doesn't end
    in the tag's version. build/Test-PackageVersion.SelfTest.ps1 tests it.

.PARAMETER Tag
    The release tag, such as v1.2.3.

.PARAMETER Path
    The packages to check, a path that may hold wildcards, such as src/*/bin/Release/*.nupkg.

.EXAMPLE
    ./build/Test-PackageVersion.ps1 -Tag v1.2.3 -Path 'src/*/bin/Release/*.nupkg'
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Tag,

    [Parameter(Mandatory)]
    [string]$Path
)

$ErrorActionPreference = 'Stop'

if ($Tag -cnotmatch '^v([0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?)\z') {
    throw "The release tag '$Tag' isn't a version such as v1.2.3."
}
$version = $Matches[1]
$suffix = ".$version.nupkg"

$packages = @(Get-ChildItem -Path $Path -File -ErrorAction SilentlyContinue)
if (-not $packages) {
    throw 'The build produced no package.'
}

$wrong = @($packages | Where-Object { -not $_.Name.EndsWith($suffix, [StringComparison]::OrdinalIgnoreCase) })
if ($wrong) {
    $names = $wrong.Name -join ', '
    throw "These packages don't have version $version, the tag's: $names. Set version in version.json to $version."
}

"Every package has version $version."
