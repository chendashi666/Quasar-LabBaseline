# ---------------------------------------------------------------------------
# Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
# 作者：陈森（Chen Sen）  https://github.com/chendashi666
# 本文件由陈森创作：禁止盗卖，禁止商业用途。
# 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
# ---------------------------------------------------------------------------

#requires -Version 5.1
<#
.SYNOPSIS
    Builds the distributable source package of the Quasar detection evaluation laboratory baseline.

.DESCRIPTION
    Copies the repository into a clean staging directory while pruning everything which must not
    be published: build outputs, NuGet caches, laboratory certificates and private keys, and
    machine specific user files. The result is verified by a clean room build performed by the
    caller.

.PARAMETER Version
    Version string used in the package and archive name.

.PARAMETER OutputRoot
    Directory which receives the staging folder and the archive (ignored by git).

.PARAMETER IncludePrebuilt
    Also ships the already built control side binaries: the full control console
    (dist\ControlConsole) and the standalone one click toolkit (dist\LabToolkit).
    Laboratory certificates and private keys are deliberately removed: both tools create their own
    authority on first run, and baseline samples are then produced locally.

.PARAMETER IncludePrebuiltConsole
    Backwards compatible alias of -IncludePrebuilt.

.EXAMPLE
    .\lab\tools\make_release_package.ps1 -Version 1.4.1.0
#>
[CmdletBinding()]
param(
    [string]$Version = '1.4.1.0',
    [string]$OutputRoot = 'release',
    [switch]$IncludePrebuilt,
    [switch]$IncludePrebuiltConsole
)

$ErrorActionPreference = 'Stop'

$ToolRoot = $PSScriptRoot
$RepoRoot = (Resolve-Path -LiteralPath (Join-Path $ToolRoot '..\..')).Path
$PackageName = 'Quasar-LabBaseline-' + $Version
$StageRoot = Join-Path $RepoRoot $OutputRoot
$StageDir = Join-Path $StageRoot $PackageName
$ZipPath = Join-Path $StageRoot ($PackageName + '.zip')

# Directories which never belong into a source release.
$excludeDirectories = @('.git', '.vs', 'bin', 'obj', 'dist', 'packages', 'release', 'artifacts', '__pycache__', 'node_modules', '.idea')
# Secrets, private keys and machine specific user files.
$excludeFiles = @('*.pfx', '*.p12', '*.snk', '*.cer', '*.user', '*.suo', '*.nupkg')

function Copy-SourceTree {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination
    )

    foreach ($item in (Get-ChildItem -LiteralPath $Source -Force)) {
        if ($item.PSIsContainer) {
            if ($excludeDirectories -contains $item.Name) { continue }
            $child = Join-Path $Destination $item.Name
            New-Item -ItemType Directory -Path $child -Force | Out-Null
            Copy-SourceTree -Source $item.FullName -Destination $child
            continue
        }

        $skip = $false
        foreach ($pattern in $excludeFiles) {
            if ($item.Name -like $pattern) { $skip = $true; break }
        }
        if ($skip) { continue }

        Copy-Item -LiteralPath $item.FullName -Destination $Destination -Force
    }
}

Write-Host ''
Write-Host ('==== Packaging ' + $PackageName) -ForegroundColor Cyan
Write-Host ('Repository : ' + $RepoRoot)

if (Test-Path -LiteralPath $StageDir) { Remove-Item -LiteralPath $StageDir -Recurse -Force }
New-Item -ItemType Directory -Path $StageDir -Force | Out-Null

Copy-SourceTree -Source $RepoRoot -Destination $StageDir
Write-Host 'Source tree copied. Build outputs, certificate stores and user files excluded.'

if ($IncludePrebuilt -or $IncludePrebuiltConsole) {
    $prebuiltTargets = @(
        @{ Source = 'dist\ControlConsole'; Target = 'prebuilt\ControlConsole' },
        @{ Source = 'dist\LabToolkit';     Target = 'prebuilt\LabToolkit' }
    )

    foreach ($entry in $prebuiltTargets) {
        $source = Join-Path $RepoRoot $entry.Source
        if (-not (Test-Path -LiteralPath $source)) {
            throw ($entry.Source + ' not found. Run build_release.ps1 first.')
        }

        $target = Join-Path $StageDir $entry.Target
        New-Item -ItemType Directory -Path $target -Force | Out-Null

        Get-ChildItem -LiteralPath $source -File | Where-Object {
            $_.Extension -ne '.p12' -and $_.Name -ne 'settings.xml'
        } | ForEach-Object {
            Copy-Item -LiteralPath $_.FullName -Destination $target -Force
        }

        Write-Host ('Prebuilt copied: ' + $entry.Source + ' -> ' + $target)
    }

    Write-Host 'Laboratory certificates and private keys removed from the prebuilt payloads.'
}

$gitCommit = 'unavailable'
try { $gitCommit = (& git -C $RepoRoot rev-parse HEAD).Trim() } catch { $gitCommit = 'unavailable' }
$gitBranch = 'unavailable'
try { $gitBranch = (& git -C $RepoRoot rev-parse --abbrev-ref HEAD).Trim() } catch { $gitBranch = 'unavailable' }
$gitDirty = 'unknown'
try {
    $status = (& git -C $RepoRoot status --porcelain) -join ''
    $gitDirty = if ([string]::IsNullOrWhiteSpace($status)) { 'clean' } else { 'modified-working-tree' }
} catch { $gitDirty = 'unknown' }

$info = New-Object System.Collections.Generic.List[string]
$info.Add('Quasar Detection Evaluation Laboratory - baseline source package')
$info.Add('package        : ' + $PackageName)
$info.Add('version        : ' + $Version)
$info.Add('built_utc      : ' + (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ'))
$info.Add('git_commit     : ' + $gitCommit)
$info.Add('git_branch     : ' + $gitBranch)
$info.Add('git_tree       : ' + $gitDirty)
$info.Add('upstream       : https://github.com/quasar/Quasar (MIT License, Copyright (c) 2023 MaxXor)')
$info.Add('')
$info.Add('excluded from this package:')
$info.Add('  - build outputs: ' + ($excludeDirectories -join ', '))
$info.Add('  - secrets / user files: ' + ($excludeFiles -join ', '))
$info.Add('  - all laboratory certificates (*.p12) and code signing keys')
$info.Add('')
$info.Add('first build:')
$info.Add('  powershell -ExecutionPolicy Bypass -File .\build_release.ps1 -Hosts "<CONTROL_IP>:4782;" -Tag "COND-A" -Subnet "<SUBNET>"')
$info.Add('')
$info.Add('')
if ($IncludePrebuilt -or $IncludePrebuiltConsole) {
    $info.Add('prebuilt payloads (no certificates, tools create their own authority on first run):')
    $info.Add('  prebuilt/ControlConsole/  full control console, use --lab-packager for the packaging UI')
    $info.Add('  prebuilt/LabToolkit/      standalone single file toolkit + client.bin, no build required')
}
$info.Add('author         : Chen Sen - https://github.com/chendashi666')
$info.Add('license        : laboratory engineering - no resale, no commercial use')
$info.Add('documentation  : README.LAB.md, lab/README.md, VERIFICATION_REPORT.md')
$info.Add('')
$noticePath = Join-Path $RepoRoot 'NOTICE.md'
if (Test-Path -LiteralPath $noticePath) {
    foreach ($noticeLine in (Get-Content -LiteralPath $noticePath -Encoding UTF8)) {
        $info.Add($noticeLine)
    }
}
$info | Set-Content -LiteralPath (Join-Path $StageDir 'PACKAGE-INFO.txt') -Encoding UTF8

$files = Get-ChildItem -LiteralPath $StageDir -Recurse -File | Sort-Object FullName
$manifest = New-Object System.Collections.Generic.List[string]
foreach ($file in $files) {
    $relative = $file.FullName.Substring($StageDir.Length + 1).Replace('\', '/')
    $manifest.Add(((Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + $relative))
}
$manifest | Set-Content -LiteralPath (Join-Path $StageDir 'MANIFEST.sha256') -Encoding ASCII

if (Test-Path -LiteralPath $ZipPath) { Remove-Item -LiteralPath $ZipPath -Force }
Compress-Archive -Path $StageDir -DestinationPath $ZipPath -CompressionLevel Optimal

$zipHash = (Get-FileHash -LiteralPath $ZipPath -Algorithm SHA256).Hash.ToLowerInvariant()
$zipSize = (Get-Item -LiteralPath $ZipPath).Length
$stageSize = (Get-ChildItem -LiteralPath $StageDir -Recurse -File | Measure-Object -Property Length -Sum).Sum

Write-Host ''
Write-Host ('staging  : ' + $StageDir)
Write-Host ('archive  : ' + $ZipPath)
Write-Host ('files    : ' + $files.Count)
Write-Host ('unpacked : ' + $stageSize + ' bytes')
Write-Host ('archive  : ' + $zipSize + ' bytes')
Write-Host ('sha256   : ' + $zipHash)
Write-Host 'Package ready.'
