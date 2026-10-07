# ---------------------------------------------------------------------------
# Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
# 作者：陈森（Chen Sen）  https://github.com/chendashi666
# 本文件由陈森创作：禁止盗卖，禁止商业用途。
# 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
# ---------------------------------------------------------------------------

param([string]$ExePath, [string]$OutPng)
Add-Type -AssemblyName System.Drawing
$ExePath = (Resolve-Path -LiteralPath $ExePath).Path
$ico = [System.Drawing.Icon]::ExtractAssociatedIcon($ExePath)
$bmp = $ico.ToBitmap()
$bmp.Save($OutPng, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Output ('icon_extracted=' + $OutPng + ' size=' + $bmp.Width + 'x' + $bmp.Height)
