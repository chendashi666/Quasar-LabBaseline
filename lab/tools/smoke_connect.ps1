# ---------------------------------------------------------------------------
# Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
# 作者：陈森（Chen Sen）  https://github.com/chendashi666
# 本文件由陈森创作：禁止盗卖，禁止商业用途。
# 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
# ---------------------------------------------------------------------------

#requires -Version 5.1
<#
    Laboratory baseline connection smoke test.
    Starts a loopback TCP listener, launches the baseline sample and reports whether the sample
    actually connected to the host and port taken from its config.json. This proves end to end
    that the external configuration drives the network parameters of the built sample.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$BaselinePath,
    [Parameter(Mandatory = $true)][int]$Port,
    [int]$TimeoutSeconds = 15
)

$ErrorActionPreference = 'Stop'
$BaselinePath = (Resolve-Path -LiteralPath $BaselinePath).Path

$listener = New-Object System.Net.Sockets.TcpListener([System.Net.IPAddress]::Loopback, $Port)
$listener.Start()

$process = $null
$accepted = $false
$remote = '<none>'

try {
    $process = Start-Process -FilePath $BaselinePath -PassThru
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if ($listener.Pending()) {
            $client = $listener.AcceptTcpClient()
            $accepted = $true
            $remote = $client.Client.RemoteEndPoint.ToString()
            $client.Close()
            break
        }
        Start-Sleep -Milliseconds 200
    }
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
    $listener.Stop()
    $name = [System.IO.Path]::GetFileNameWithoutExtension($BaselinePath)
    Get-Process -Name $name -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
}

Write-Output ('listener_port=' + $Port)
Write-Output ('connection_observed=' + $accepted)
Write-Output ('remote_endpoint=' + $remote)

if (-not $accepted) { exit 1 }
exit 0
