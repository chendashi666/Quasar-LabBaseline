# ---------------------------------------------------------------------------
# Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
# 作者：陈森（Chen Sen）  https://github.com/chendashi666
# 本文件由陈森创作：禁止盗卖，禁止商业用途。
# 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
# ---------------------------------------------------------------------------

#requires -Version 5.1
<#
    Laboratory baseline runtime verification (fallback host: Windows PowerShell 5.1, .NET Framework).
    Loads the produced baseline sample and reports the effective run parameters after
    Settings.Initialize(), which is the exact code path the client uses when it starts.

    The process is intentionally single shot: run it again with config.json removed to verify the
    compiled-in fallback path.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$BaselinePath
)

$ErrorActionPreference = 'Stop'
$BaselinePath = (Resolve-Path -LiteralPath $BaselinePath).Path

$assembly = [Reflection.Assembly]::LoadFile($BaselinePath)
$settingsType = $assembly.GetType('Quasar.Client.Config.Settings', $true)

function Get-SettingValue([string]$name) {
    $field = $settingsType.GetField($name)
    if ($null -eq $field) { return '<missing>' }
    return $field.GetValue($null)
}

Write-Output ('baseline=' + $BaselinePath)
Write-Output ('config_present=' + (Test-Path -LiteralPath (Join-Path (Split-Path $BaselinePath -Parent) 'config.json')))

$initMethod = $settingsType.GetMethod('Initialize')
$applied = $initMethod.Invoke($null, $null)
Write-Output ('initialize_result=' + $applied)
Write-Output ('HOSTS=' + (Get-SettingValue 'HOSTS'))
Write-Output ('MUTEX=' + (Get-SettingValue 'MUTEX'))
Write-Output ('TAG=' + (Get-SettingValue 'TAG'))
Write-Output ('SUBNET=' + (Get-SettingValue 'SUBNET'))
Write-Output ('RECONNECTDELAY=' + (Get-SettingValue 'RECONNECTDELAY'))
Write-Output ('VERSION=' + (Get-SettingValue 'VERSION'))
Write-Output ('LOGDIRECTORYNAME=' + (Get-SettingValue 'LOGDIRECTORYNAME'))
Write-Output ('INSTALLPATH=' + (Get-SettingValue 'INSTALLPATH'))
Write-Output ('UNATTENDEDMODE=' + (Get-SettingValue 'UNATTENDEDMODE'))

$cert = Get-SettingValue 'SERVERCERTIFICATE'
if ($null -ne $cert -and $cert -ne '<missing>') {
    Write-Output ('SERVERCERT_SUBJECT=' + $cert.Subject)
    Write-Output ('SERVERCERT_THUMBPRINT=' + $cert.Thumbprint)
    Write-Output ('ENCRYPTIONKEY=' + (Get-SettingValue 'ENCRYPTIONKEY'))
} else {
    Write-Output 'SERVERCERT_SUBJECT=<none>'
}
