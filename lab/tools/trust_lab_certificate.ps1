# ---------------------------------------------------------------------------
# Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
# 作者：陈森（Chen Sen）  https://github.com/chendashi666
# 本文件由陈森创作：禁止盗卖，禁止商业用途。
# 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
# ---------------------------------------------------------------------------

#requires -Version 5.1
<#
    Laboratory baseline trust provisioning for a target machine.
    Imports the signer certificate and the laboratory server certificate authority into the local
    machine trust stores so the signed baseline sample is a trusted artefact on every testbed VM.
    Run once, elevated, on each control and target machine.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$SignerCertificatePath,
    [string]$ServerCertificatePath
)

$ErrorActionPreference = 'Stop'

$principal = New-Object System.Security.Principal.WindowsPrincipal([System.Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run this script from an elevated PowerShell session.'
}

Import-Certificate -FilePath $SignerCertificatePath -CertStoreLocation 'Cert:\LocalMachine\Root' | Out-Null
Import-Certificate -FilePath $SignerCertificatePath -CertStoreLocation 'Cert:\LocalMachine\TrustedPublisher' | Out-Null
Write-Output ('signer trusted: ' + $SignerCertificatePath)

if ($ServerCertificatePath) {
    Import-Certificate -FilePath $ServerCertificatePath -CertStoreLocation 'Cert:\LocalMachine\Root' | Out-Null
    Write-Output ('server ca trusted: ' + $ServerCertificatePath)
}
