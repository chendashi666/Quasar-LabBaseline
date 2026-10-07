# ---------------------------------------------------------------------------
# Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
# 作者：陈森（Chen Sen）  https://github.com/chendashi666
# 本文件由陈森创作：禁止盗卖，禁止商业用途。
# 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
# ---------------------------------------------------------------------------

#requires -Version 5.1
<#
.SYNOPSIS
    Quasar Detection Evaluation Laboratory - reproducible baseline build.

.DESCRIPTION
    One click pipeline which turns the Quasar source tree into a standardized, parameter
    externalized laboratory baseline sample:

        build -> metadata assertion -> dependency merge -> resource normalization
              -> certificate -> baseline client -> signing -> manifest -> verification

    Every produced artefact is labelled "laboratory baseline sample" and is only meant for the
    isolated detection evaluation testbed.

.PARAMETER Hosts
    Control host entries written into config.json, e.g. "10.10.10.10:4782;". Fill in with the
    internal testbed addresses.

.PARAMETER RotateServerCertificate
    Regenerates the laboratory certificate authority (quasar.p12) and invalidates previously
    built baseline samples.

.EXAMPLE
    .\build_release.ps1 -Hosts "10.10.10.10:4782;" -Tag "COND-A" -Subnet "10.10.10.0/24"
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [string]$Platform = 'AnyCPU',

    [string]$Hosts = '10.10.10.10:4782;',
    [int]$Port = 4782,
    [string]$Tag = 'COND-A',
    [string]$Subnet = '10.10.10.0/24',
    [string]$Mutex = '',

    [string]$OutputRoot = 'dist',
    [string]$BaselineName = 'LabBaseline.exe',

    [string]$SigningSubject = 'CN=Detection Evaluation Laboratory Baseline Signer',
    [switch]$RotateServerCertificate,
    [switch]$SkipBuild,
    [switch]$SkipSign,
    [switch]$SkipSmokeTest
)

$ErrorActionPreference = 'Stop'

$RepoRoot = $PSScriptRoot
$BinDir = Join-Path $RepoRoot ("bin\" + $Configuration + "\net452")
$BaselineDir = Join-Path $RepoRoot (Join-Path $OutputRoot 'LabBaseline')
$ConsoleDir = Join-Path $RepoRoot (Join-Path $OutputRoot 'ControlConsole')
$ToolkitDir = Join-Path $RepoRoot (Join-Path $OutputRoot 'LabToolkit')
$LabIcon = Join-Path $RepoRoot 'lab\LabBaseline.ico'
$LabTools = Join-Path $RepoRoot 'lab\tools'
$PowerShell51 = Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'

function Write-Step {
    param([string]$Message)
    Write-Host ''
    Write-Host ('==== ' + $Message) -ForegroundColor Cyan
}

function Find-MSBuild {
    $vswhere = Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFilesX86)) 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path -LiteralPath $vswhere) {
        $found = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' 2>$null
        if ($found) { return ($found | Select-Object -First 1) }
    }

    $roots = @(
        [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFiles),
        [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFilesX86)
    )
    foreach ($root in $roots) {
        $candidate = Get-ChildItem -Path $root -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like 'Microsoft Visual Studio*' } |
            ForEach-Object { Get-ChildItem -Path $_.FullName -Recurse -Filter MSBuild.exe -ErrorAction SilentlyContinue } |
            Where-Object { $_.FullName -match '\\MSBuild\\Current\\Bin\\MSBuild\.exe$' } |
            Select-Object -First 1
        if ($candidate) { return $candidate.FullName }
    }

    throw 'MSBuild.exe was not found. Install the Visual Studio Build Tools with the .NET desktop workload.'
}

function Invoke-QuasarCli {
    param(
        [Parameter(Mandatory = $true)][string]$ExePath,
        [Parameter(Mandatory = $true)][string[]]$Arguments
    )

    $quoted = foreach ($argument in $Arguments) {
        if ($argument -match '\s') { '"' + $argument + '"' } else { $argument }
    }
    $argumentLine = ($quoted -join ' ')

    $stdoutFile = [System.IO.Path]::GetTempFileName()
    $stderrFile = [System.IO.Path]::GetTempFileName()
    try {
        $process = Start-Process -FilePath $ExePath -ArgumentList $argumentLine -WorkingDirectory $RepoRoot -Wait -NoNewWindow -PassThru -RedirectStandardOutput $stdoutFile -RedirectStandardError $stderrFile
        $stdout = Get-Content -LiteralPath $stdoutFile -Raw -ErrorAction SilentlyContinue
        $stderr = Get-Content -LiteralPath $stderrFile -Raw -ErrorAction SilentlyContinue
        if ($stdout) { Write-Host $stdout.TrimEnd() }
        if ($process.ExitCode -ne 0) {
            throw ('Quasar CLI failed with exit code ' + $process.ExitCode + '. ' + $stderr)
        }
        return $stdout
    }
    finally {
        Remove-Item -LiteralPath $stdoutFile, $stderrFile -Force -ErrorAction SilentlyContinue
    }
}

function Find-SignTool {
    $kitsRoot = Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFilesX86)) 'Windows Kits\10\bin'
    if (-not (Test-Path -LiteralPath $kitsRoot)) { return $null }

    $candidates = Get-ChildItem -Path $kitsRoot -Recurse -Filter signtool.exe -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\x64\\signtool\.exe$' } |
        Sort-Object -Property FullName -Descending
    if ($candidates) { return ($candidates | Select-Object -First 1).FullName }
    return $null
}

function Get-SigningCertificate {
    param([string]$Subject)

    $existing = Get-ChildItem -Path Cert:\CurrentUser\My -ErrorAction SilentlyContinue |
        Where-Object { $_.Subject -eq $Subject -and $_.HasPrivateKey -and $_.NotAfter -gt (Get-Date) } |
        Sort-Object -Property NotAfter -Descending |
        Select-Object -First 1
    if ($existing) { return $existing }

    Write-Host ('Creating laboratory code signing certificate: ' + $Subject)
    $parameters = @{
        Type = 'CodeSigningCert'
        Subject = $Subject
        CertStoreLocation = 'Cert:\CurrentUser\My'
        KeyExportPolicy = 'Exportable'
        KeyAlgorithm = 'RSA'
        KeyLength = 3072
        HashAlgorithm = 'SHA256'
        NotAfter = (Get-Date).AddYears(5)
    }
    return (New-SelfSignedCertificate @parameters)
}

function Get-FileSha256 {
    param([string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
}

function Assert-Metadata {
    param([string]$Path, [string]$ExpectedDescriptionPrefix)

    $info = (Get-Item -LiteralPath $Path).VersionInfo
    $problems = @()
    if ($info.ProductName -ne 'Quasar Detection Baseline') { $problems += ('ProductName=' + $info.ProductName) }
    if ($info.CompanyName -notlike '*Chen Sen*') { $problems += ('CompanyName=' + $info.CompanyName) }
    if ($info.FileVersion -ne '1.4.1.0') { $problems += ('FileVersion=' + $info.FileVersion) }
    if ($info.LegalCopyright -notlike '*Chen Sen*') { $problems += ('LegalCopyright=' + $info.LegalCopyright) }
    if ($info.LegalCopyright -notlike '*MaxXor*') { $problems += ('LegalCopyright(missing upstream attribution)=' + $info.LegalCopyright) }
    if (-not $info.FileDescription.StartsWith($ExpectedDescriptionPrefix)) { $problems += ('FileDescription=' + $info.FileDescription) }

    if ($problems.Count -gt 0) {
        throw ('Standardized metadata mismatch in ' + $Path + ': ' + ($problems -join '; '))
    }
}

Write-Step ('STEP 1/9  Build (' + $Configuration + '|' + $Platform + ')')
if (-not $SkipBuild) {
    $msbuild = Find-MSBuild
    Write-Host ('MSBuild: ' + $msbuild)

    & $msbuild (Join-Path $RepoRoot 'Quasar.sln') /t:Restore /p:Configuration=$Configuration /p:Platform=$Platform /v:m /nologo
    if ($LASTEXITCODE -ne 0) { throw 'NuGet restore failed.' }

    & $msbuild (Join-Path $RepoRoot 'Quasar.sln') /p:Configuration=$Configuration /p:Platform=$Platform /v:m /nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
}
else {
    Write-Host 'Build skipped on request.'
}

if (-not (Test-Path -LiteralPath (Join-Path $BinDir 'Quasar.exe'))) { throw ('Quasar.exe is missing in ' + $BinDir) }
if (-not (Test-Path -LiteralPath (Join-Path $BinDir 'client.bin'))) { throw ('client.bin is missing in ' + $BinDir) }

Write-Step 'STEP 2/9  Metadata assertion and dependency merge check'
Assert-Metadata -Path (Join-Path $BinDir 'Quasar.exe') -ExpectedDescriptionPrefix 'Quasar Lab Baseline Console'
Assert-Metadata -Path (Join-Path $BinDir 'Client.exe') -ExpectedDescriptionPrefix 'Quasar Lab Baseline Client'
Assert-Metadata -Path (Join-Path $BinDir 'QuasarLabToolkit.exe') -ExpectedDescriptionPrefix 'Quasar Lab Baseline Toolkit'
Write-Host 'Standardized metadata OK for Quasar.exe, Client.exe and QuasarLabToolkit.exe.'

$mergedSize = (Get-Item -LiteralPath (Join-Path $BinDir 'Client.exe')).Length
if ($mergedSize -lt 1500000) { throw 'The merged client looks too small; ILRepack did not run.' }
Write-Host ('ILRepack merged client size: ' + $mergedSize + ' bytes (dependencies consolidated).')

if (-not (Test-Path -LiteralPath $LabIcon)) { throw ('Laboratory icon missing: ' + $LabIcon) }

Write-Step 'STEP 3/9  Laboratory certificate authority'
$quasarExe = Join-Path $BinDir 'Quasar.exe'
$serverCertificatePath = Join-Path $BinDir 'quasar.p12'
if ($RotateServerCertificate -and (Test-Path -LiteralPath $serverCertificatePath)) {
    Remove-Item -LiteralPath $serverCertificatePath -Force
}
if (-not (Test-Path -LiteralPath $serverCertificatePath)) {
    Invoke-QuasarCli -ExePath $quasarExe -Arguments @('--make-cert', '--out', $serverCertificatePath, '--name', 'Detection Evaluation Lab CA', '--key', '4096') | Out-Null
}
$serverCertificate = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($serverCertificatePath, '', [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)
Write-Host ('Server certificate thumbprint: ' + $serverCertificate.Thumbprint)

$outputRootPath = Join-Path $RepoRoot $OutputRoot
if (-not (Test-Path -LiteralPath $outputRootPath)) { New-Item -ItemType Directory -Path $outputRootPath -Force | Out-Null }
$serverCaExport = Join-Path $outputRootPath 'LabServerCA.cer'
[System.IO.File]::WriteAllBytes($serverCaExport, $serverCertificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert))
Write-Host ('Server certificate authority exported: ' + $serverCaExport)

Write-Step 'STEP 4/9  Baseline client build (dependency merge + resource normalization)'
if (-not $Mutex) { $Mutex = [Guid]::NewGuid().ToString() }
if (-not (Test-Path -LiteralPath $BaselineDir)) { New-Item -ItemType Directory -Path $BaselineDir -Force | Out-Null }

$baselinePath = Join-Path $BaselineDir $BaselineName
Invoke-QuasarCli -ExePath $quasarExe -Arguments @(
    '--build-client',
    '--out', $baselinePath,
    '--hosts', $Hosts,
    '--tag', $Tag,
    '--subnet', $Subnet,
    '--mutex', $Mutex,
    '--icon', $LabIcon,
    '--log-dir', 'Logs'
) | Out-Null

if (-not (Test-Path -LiteralPath (Join-Path $BaselineDir 'config.json'))) {
    throw 'config.json was not produced next to the baseline client.'
}

Write-Step 'STEP 5/9  Control console and standalone toolkit staging'
if (Test-Path -LiteralPath $ConsoleDir) { Remove-Item -LiteralPath $ConsoleDir -Recurse -Force }
New-Item -ItemType Directory -Path $ConsoleDir -Force | Out-Null

# Explicit distribution layout: the console payload must be identical on every experiment machine.
$consoleFiles = @(
    'Quasar.exe',
    'Quasar.exe.config',
    'Quasar.Common.dll',
    'protobuf-net.dll',
    'BouncyCastle.Crypto.dll',
    'Gma.System.MouseKeyHook.dll',
    'Open.Nat.dll',
    'Mono.Cecil.dll',
    'Mono.Cecil.Mdb.dll',
    'Mono.Cecil.Pdb.dll',
    'Mono.Cecil.Rocks.dll',
    'Vestris.ResourceLib.dll',
    'client.bin'
)
foreach ($name in $consoleFiles) {
    $source = Join-Path $BinDir $name
    if (-not (Test-Path -LiteralPath $source)) { throw ('Control console payload is missing: ' + $name) }
    Copy-Item -LiteralPath $source -Destination $ConsoleDir -Force
}
Copy-Item -LiteralPath $serverCertificatePath -Destination (Join-Path $ConsoleDir 'quasar.p12') -Force

# Localized resources of the console, when the build produced any.
$satelliteDir = Join-Path $BinDir 'zh-Hans'
if (Test-Path -LiteralPath $satelliteDir) {
    Get-ChildItem -LiteralPath $satelliteDir -Filter 'Quasar*.resources.dll' -ErrorAction SilentlyContinue | ForEach-Object {
        $destination = Join-Path $ConsoleDir 'zh-Hans'
        if (-not (Test-Path -LiteralPath $destination)) { New-Item -ItemType Directory -Path $destination -Force | Out-Null }
        Copy-Item -LiteralPath $_.FullName -Destination $destination -Force
    }
}
Write-Host ('Control console staged in ' + $ConsoleDir + ' (' + (Get-ChildItem -LiteralPath $ConsoleDir -File).Count + ' files)')

# Standalone toolkit: one single executable plus the client template, enough to produce baseline
# samples on a machine which never runs the full build pipeline.
$toolkitSource = Join-Path $BinDir 'QuasarLabToolkit.exe'
if (-not (Test-Path -LiteralPath $toolkitSource)) { throw ('QuasarLabToolkit.exe is missing in ' + $BinDir) }
if (Test-Path -LiteralPath $ToolkitDir) { Remove-Item -LiteralPath $ToolkitDir -Recurse -Force }
New-Item -ItemType Directory -Path $ToolkitDir -Force | Out-Null
Copy-Item -LiteralPath $toolkitSource -Destination $ToolkitDir -Force
Copy-Item -LiteralPath (Join-Path $BinDir 'client.bin') -Destination $ToolkitDir -Force
Write-Host ('Standalone toolkit staged in ' + $ToolkitDir + ' (QuasarLabToolkit.exe + client.bin)')

Write-Step 'STEP 6/9  Local self signing (signtool)'
$signatureSubject = $null
$signatureThumbprint = $null
if (-not $SkipSign) {
    $signTool = Find-SignTool
    if (-not $signTool) { throw 'signtool.exe was not found. Install the Windows SDK signing tools or pass -SkipSign.' }
    Write-Host ('signtool: ' + $signTool)

    $signingCertificate = Get-SigningCertificate -Subject $SigningSubject
    $signatureSubject = $signingCertificate.Subject
    $signatureThumbprint = $signingCertificate.Thumbprint
    Write-Host ('Signing certificate thumbprint: ' + $signatureThumbprint)

    $certificateExport = Join-Path $RepoRoot (Join-Path $OutputRoot 'LabBaselineSigner.cer')
    Export-Certificate -Cert $signingCertificate -FilePath $certificateExport -Force | Out-Null
    Write-Host ('Signer certificate exported: ' + $certificateExport)

    foreach ($target in @((Join-Path $ConsoleDir 'Quasar.exe'), $baselinePath, (Join-Path $ToolkitDir 'QuasarLabToolkit.exe'))) {
        & $signTool sign /fd SHA256 /sha1 $signatureThumbprint /v $target
        if ($LASTEXITCODE -ne 0) { throw ('Signing failed for ' + $target) }
        $signature = Get-AuthenticodeSignature -LiteralPath $target
        Write-Host ('Signature for ' + (Split-Path $target -Leaf) + ': signer=' + $signature.SignerCertificate.Subject + ' status=' + $signature.Status)
        Write-Host '  (import dist\LabBaselineSigner.cer and dist\LabServerCA.cer on the targets to make the root trusted)'
    }
}
else {
    Write-Host 'Signing skipped on request.'
}

Write-Step 'STEP 7/9  Manifest'
$gitCommit = 'unavailable'
try { $gitCommit = (& git -C $RepoRoot rev-parse HEAD).Trim() } catch { $gitCommit = 'unavailable' }

$manifestPath = Join-Path $BaselineDir 'MANIFEST.txt'
$manifest = New-Object System.Collections.Generic.List[string]
$manifest.Add('Quasar Detection Evaluation Laboratory - baseline sample manifest')
$manifest.Add('label              : laboratory baseline sample')
$manifest.Add('author             : Chen Sen - https://github.com/chendashi666')
$manifest.Add('license            : laboratory engineering - no resale, no commercial use')
$manifest.Add('upstream           : Quasar (c) MaxXor and contributors, MIT License')
$manifest.Add('purpose            : endpoint detection coverage measurement (isolated testbed only)')
$manifest.Add('built_utc          : ' + (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ'))
$manifest.Add('git_commit         : ' + $gitCommit)
$manifest.Add('configuration      : ' + $Configuration + '|' + $Platform)
$manifest.Add('hosts              : ' + $Hosts)
$manifest.Add('port               : ' + $Port)
$manifest.Add('tag                : ' + $Tag)
$manifest.Add('subnet             : ' + $Subnet)
$manifest.Add('mutex              : ' + $Mutex)
$manifest.Add('server_cert_sha1   : ' + $serverCertificate.Thumbprint)
if ($signatureSubject) { $manifest.Add('signature_subject  : ' + $signatureSubject) } else { $manifest.Add('signature_subject  : unsigned') }
if ($signatureThumbprint) { $manifest.Add('signature_thumb    : ' + $signatureThumbprint) } else { $manifest.Add('signature_thumb    : unsigned') }
$manifest.Add('')
$noticePath = Join-Path $RepoRoot 'NOTICE.md'
if (Test-Path -LiteralPath $noticePath) {
    $manifest.Add('')
    foreach ($noticeLine in (Get-Content -LiteralPath $noticePath -Encoding UTF8 -TotalCount 14)) {
        $manifest.Add($noticeLine)
    }
}
$manifest.Add('files:')
foreach ($file in (Get-ChildItem -LiteralPath $BaselineDir -File | Sort-Object Name)) {
    if ($file.Name -eq 'MANIFEST.txt') { continue }
    $manifest.Add('  ' + $file.Name.PadRight(24) + ' sha256=' + (Get-FileSha256 -Path $file.FullName) + ' bytes=' + $file.Length)
}
$manifest | Set-Content -LiteralPath $manifestPath -Encoding UTF8
Write-Host ('Manifest written: ' + $manifestPath)

Write-Step 'STEP 8/9  Runtime verification (external configuration and compiled-in fallback)'
$verifyScript = Join-Path $LabTools 'verify_baseline.ps1'
$fallbackScript = Join-Path $LabTools 'verify_fallback.ps1'
$smokeScript = Join-Path $LabTools 'smoke_connect.ps1'

$unrenamedClient = Join-Path $BinDir 'Client.exe'
$testConfig = Join-Path $BinDir 'config.json'
Invoke-QuasarCli -ExePath $quasarExe -Arguments @(
    '--write-config', '--out', $BinDir,
    '--hosts', '10.20.30.40:4782;', '--tag', 'VERIFY', '--subnet', '10.20.30.0/24', '--mutex', 'verify-mutex'
) | Out-Null

$verifyOutput = & $PowerShell51 -NoProfile -ExecutionPolicy Bypass -File $verifyScript -BaselinePath $unrenamedClient 2>&1
$verifyOutput | Write-Host
if (-not ($verifyOutput -match 'initialize_result=True')) { throw 'External configuration was not applied by Settings.Initialize().' }
if (-not ($verifyOutput -match 'HOSTS=10\.20\.30\.40:4782;')) { throw 'HOSTS from config.json was not applied.' }

$verifyFallback = & $PowerShell51 -NoProfile -ExecutionPolicy Bypass -File $fallbackScript -BaselinePath $unrenamedClient 2>&1
$verifyFallback | Write-Host
if (-not ($verifyFallback -match 'initialize_result=False')) { throw 'Compiled-in fallback path no longer behaves as upstream.' }

Write-Step 'STEP 9/9  Connection smoke test (external override and single file delivery)'
if (-not $SkipSmokeTest) {
    # Scenario A: an external config.json overrides the embedded parameters.
    $smokeDir = Join-Path ([System.IO.Path]::GetTempPath()) ('lab-smoke-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $smokeDir -Force | Out-Null
    Copy-Item -LiteralPath $baselinePath -Destination $smokeDir -Force

    $smokePort = 47821
    Invoke-QuasarCli -ExePath $quasarExe -Arguments @(
        '--write-config', '--out', $smokeDir,
        '--hosts', ('127.0.0.1:' + $smokePort + ';'), '--tag', 'SMOKE', '--mutex', ('smoke-' + [Guid]::NewGuid().ToString('N'))
    ) | Out-Null

    Write-Host 'A) external config.json override'
    $smokeOutput = & $PowerShell51 -NoProfile -ExecutionPolicy Bypass -File $smokeScript -BaselinePath (Join-Path $smokeDir $BaselineName) -Port $smokePort 2>&1
    $smokeOutput | Write-Host
    Remove-Item -LiteralPath $smokeDir -Recurse -Force -ErrorAction SilentlyContinue

    if (-not ($smokeOutput -match 'connection_observed=True')) { throw 'The baseline sample did not connect to the host and port taken from config.json.' }

    # Scenario B: single file delivery, the configuration is embedded in the executable.
    $singleDir = Join-Path ([System.IO.Path]::GetTempPath()) ('lab-single-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $singleDir -Force | Out-Null
    $singleExe = Join-Path $singleDir $BaselineName
    $singlePort = 47822
    Invoke-QuasarCli -ExePath $quasarExe -Arguments @(
        '--build-client', '--out', $singleExe,
        '--hosts', ('127.0.0.1:' + $singlePort + ';'), '--tag', 'SINGLE', '--subnet', '127.0.0.0/8',
        '--mutex', ('single-' + [Guid]::NewGuid().ToString('N'))
    ) | Out-Null
    Remove-Item -LiteralPath (Join-Path $singleDir 'config.json') -Force -ErrorAction SilentlyContinue
    Write-Host 'B) single file delivery (no config.json next to the executable)'

    $singleOutput = & $PowerShell51 -NoProfile -ExecutionPolicy Bypass -File $smokeScript -BaselinePath $singleExe -Port $singlePort 2>&1
    $singleOutput | Write-Host
    Remove-Item -LiteralPath $singleDir -Recurse -Force -ErrorAction SilentlyContinue

    if (-not ($singleOutput -match 'connection_observed=True')) { throw 'The single file baseline did not connect using its embedded configuration.' }
}
else {
    Write-Host 'Smoke test skipped on request.'
}

Remove-Item -LiteralPath $testConfig -Force -ErrorAction SilentlyContinue

Write-Step 'RESULT'
Write-Host ('Baseline sample : ' + $baselinePath)
Write-Host ('Baseline config : ' + (Join-Path $BaselineDir 'config.json'))
Write-Host ('Control console : ' + (Join-Path $ConsoleDir 'Quasar.exe'))
Write-Host ('Packaging UI    : ' + (Join-Path $ConsoleDir 'Quasar.exe') + ' --lab-packager   (or lab\tools\open_packager.cmd)')
Write-Host ('Standalone tool : ' + (Join-Path $ToolkitDir 'QuasarLabToolkit.exe'))
Write-Host ('Manifest        : ' + $manifestPath)
Write-Host 'Build finished. Repoint an experiment condition by editing config.json only, no rebuild required.'
