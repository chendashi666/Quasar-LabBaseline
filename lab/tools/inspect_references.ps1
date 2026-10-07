# ---------------------------------------------------------------------------
# Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
# 作者：陈森（Chen Sen）  https://github.com/chendashi666
# 本文件由陈森创作：禁止盗卖，禁止商业用途。
# 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
# ---------------------------------------------------------------------------

param([string]$ExePath)
$ExePath = (Resolve-Path -LiteralPath $ExePath).Path
$asm = [Reflection.Assembly]::ReflectionOnlyLoadFrom($ExePath)
Write-Output ('assembly=' + $asm.FullName)
Write-Output '--- referenced assemblies ---'
$asm.GetReferencedAssemblies() | ForEach-Object { Write-Output ('  ' + $_.Name + ' ' + $_.Version) }
Write-Output '--- target framework ---'
$asm.GetCustomAttributesData() | Where-Object { $_.AttributeType.Name -eq 'TargetFrameworkAttribute' } | ForEach-Object { $_.ConstructorArguments[0].Value }
