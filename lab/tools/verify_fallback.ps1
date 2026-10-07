# ---------------------------------------------------------------------------
# Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
# 作者：陈森（Chen Sen）  https://github.com/chendashi666
# 本文件由陈森创作：禁止盗卖，禁止商业用途。
# 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
# ---------------------------------------------------------------------------

#requires -Version 5.1
<#
    Laboratory baseline fallback verification.
    Copies the baseline sample to a scratch directory without a config.json and reports what
    Settings.Initialize() does. Upstream behaviour must be preserved: without the external
    configuration a release build carries no compiled-in settings and Initialize() returns false.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$BaselinePath
)

$ErrorActionPreference = 'Stop'
$source = (Resolve-Path -LiteralPath $BaselinePath).Path
$scratch = Join-Path ([System.IO.Path]::GetTempPath()) ('lab-fallback-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch -Force | Out-Null
$target = Join-Path $scratch ([System.IO.Path]::GetFileName($source))
Copy-Item -LiteralPath $source -Destination $target -Force

try {
    $assembly = [Reflection.Assembly]::LoadFile($target)
    $settingsType = $assembly.GetType('Quasar.Client.Config.Settings', $true)
    $result = $settingsType.GetMethod('Initialize').Invoke($null, $null)
    Write-Output ('config_present=' + (Test-Path -LiteralPath (Join-Path $scratch 'config.json')))
    Write-Output ('initialize_result=' + $result)
    Write-Output ('HOSTS=' + $settingsType.GetField('HOSTS').GetValue($null))
}
finally {
    Remove-Item -LiteralPath $scratch -Recurse -Force -ErrorAction SilentlyContinue
}
