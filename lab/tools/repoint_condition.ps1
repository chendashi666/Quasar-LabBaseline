# ---------------------------------------------------------------------------
# Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
# 作者：陈森（Chen Sen）  https://github.com/chendashi666
# 本文件由陈森创作：禁止盗卖，禁止商业用途。
# 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
# ---------------------------------------------------------------------------

#requires -Version 5.1
<#
    Laboratory baseline condition switch.
    Rewrites config.json of an already built baseline sample so the very same executable runs
    under a different experiment condition. No rebuild and no re-signing is required, which is
    what keeps the measurements comparable between conditions.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$QuasarExe,
    [Parameter(Mandatory = $true)][string]$BaselineDir,
    [Parameter(Mandatory = $true)][string]$Hosts,
    [int]$Port = 4782,
    [string]$Tag = 'COND-A',
    [string]$Subnet = '',
    [string]$Mutex = ''
)

$ErrorActionPreference = 'Stop'

$QuasarExe = (Resolve-Path -LiteralPath $QuasarExe).Path
$BaselineDir = (Resolve-Path -LiteralPath $BaselineDir).Path

if (-not $Mutex) { $Mutex = [Guid]::NewGuid().ToString() }

$arguments = @('--write-config', '--out', $BaselineDir, '--hosts', $Hosts, '--port', $Port, '--tag', $Tag, '--mutex', $Mutex)
if ($Subnet) { $arguments += @('--subnet', $Subnet) }

$stdoutFile = [System.IO.Path]::GetTempFileName()
$stderrFile = [System.IO.Path]::GetTempFileName()
try {
    $process = Start-Process -FilePath $QuasarExe -ArgumentList $arguments -Wait -NoNewWindow -PassThru -RedirectStandardOutput $stdoutFile -RedirectStandardError $stderrFile
    Get-Content -LiteralPath $stdoutFile -Raw | Write-Host
    if ($process.ExitCode -ne 0) {
        throw ('Repoint failed: ' + (Get-Content -LiteralPath $stderrFile -Raw))
    }
}
finally {
    Remove-Item -LiteralPath $stdoutFile, $stderrFile -Force -ErrorAction SilentlyContinue
}

Write-Output ('condition applied: hosts=' + $Hosts + ' tag=' + $Tag + ' subnet=' + $Subnet)
Write-Output ('config: ' + (Join-Path $BaselineDir 'config.json'))
