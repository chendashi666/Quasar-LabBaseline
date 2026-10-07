# Quasar 检测评估实验台 · 基线样本验证报告

> ### 作者：陈森（Chen Sen）· <https://github.com/chendashi666>
> **本项目所有实验室工程改造与新增代码均为陈森创作。禁止盗卖。禁止商业用途。**
> 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有，遵循 MIT License，其署名与许可证必须原样保留。
> 详见 **[NOTICE.md](NOTICE.md)** 与 **[LICENSE.LAB.md](LICENSE.LAB.md)**。

- 报告类型：实验台验证方案 + 本轮已完成验证记录
- 样本标识：**实验室基线样本**（QUASAR-LAB-BASELINE）
- 适用环境：隔离内网自建靶场，两台 Windows 10 虚拟机，无外网
- 用途：终端检测栈（Defender / Sysmon / EDR）对 Quasar 的覆盖情况量化与盲区定位

---

## 1. 实验台拓扑

```
        隔离内网交换机（无外网、无 DHCP 或静态分配）
        ┌──────────────────────────────┐
        │                              │
  [VM-A 控制端]                  [VM-B 被控端]
  Win10 x64                      Win10 x64
  10.10.10.10                    10.10.10.20
  dist\ControlConsole\Quasar.exe  dist\LabBaseline\LabBaseline.exe
                                   dist\LabBaseline\config.json
        └──────────────────────────────┘
                 TCP 4782 (TLS 1.2)
```

- 控制端监听：TCP 4782（`Settings.ListenPort`，写入 `dist\ControlConsole\settings.xml` 可改）
- 被控端主动外连：由 `config.json` 的 `hosts` 决定
- 时间同步：实验前统一两台机时间（影响日志对齐），靶场无外网则手工校时

## 2. 前置准备

1. 两台 VM 均使用同一基线镜像，实验前打快照 `lab-baseline-clean`。
2. 关闭 Windows 自动更新，避免实验期间后台干扰。
3. 信任预置（每台机一次，管理员 PowerShell）：
   ```powershell
   .\lab\tools\trust_lab_certificate.ps1 -SignerCertificatePath .\dist\LabBaselineSigner.cer -ServerCertificatePath .\dist\LabServerCA.cer
   ```
   导入后 `Get-AuthenticodeSignature .\dist\LabBaseline\LabBaseline.exe` 的 `Status` 应为 `Valid`。
4. 检测侧就位：Sysmon 已部署并记录事件，Defender 实时防护按实验条件设定，EDR 代理按条件设定。
5. 防火墙：VM-A 放行 TCP 4782 入站；VM-B 按条件决定是否放行出站（**防/放行本身就是实验变量，需记录**）。

## 3. 产物校验

在被控端执行，确认样本未被篡改且条件正确：

```powershell
Get-FileHash .\LabBaseline.exe -Algorithm SHA256        # 与 MANIFEST.txt 比对
Get-AuthenticodeSignature .\LabBaseline.exe             # Status=Valid, Signer=Detection Evaluation Laboratory Baseline Signer
(Get-Item .\LabBaseline.exe).VersionInfo | Format-List  # ProductName/CompanyName/FileVersion/LegalCopyright
Get-Content .\config.json                                # 五项参数 + 证书绑定
```

判定标准：
- SHA256 与 `dist\LabBaseline\MANIFEST.txt` 一致
- 签名有效且签名主体统一（避免签名状态成为未受控变量）
- 元数据四项统一：`Quasar Detection Baseline` / `Detection Evaluation Laboratory` / `1.4.1.0` / 实验室版权行

## 4. 被控端部署与运行

### 4.0 一键出包（推荐）

在控制端打开出包控制台（双击 `lab\tools\open_packager.cmd`，或 `dist\ControlConsole\Quasar.exe --lab-packager`），
填入控制端内网 IP，点「一键出包」。得到：

- `LabBaseline.exe`：**单文件即可投递**，运行参数已内嵌
- `config.json`：覆盖层，用于后续切换实验条件而不重新出包

出包后只把 exe 拷到被控端即可运行；若同目录放了 `config.json`，其显式字段优先。

### 4.1 命令行等价流程

```powershell
# 1) 按实验条件填写参数（三种等价方式任选）
#    a. 直接编辑 config.json 的 hosts/port/mutex/tag/subnet
#    b. 在样本同目录运行参数窗体
.\LabBaseline.exe --config
#    c. 在控制端用脚本重写（无需重新构建）
.\lab\tools\repoint_condition.ps1 -QuasarExe .\dist\ControlConsole\Quasar.exe -BaselineDir .\dist\LabBaseline -Hosts "10.10.10.10:4782;" -Tag "COND-A"

# 2) 启动样本（无人值守模式，无托盘图标、无窗口）
Start-Process .\LabBaseline.exe
```

## 5. 控制端启动

```powershell
cd .\dist\ControlConsole
.\Quasar.exe
# 首次运行会提示证书；使用目录内已有的 quasar.p12（实验台 CA），不要重新生成，
# 否则已构建的基线样本将无法通过服务器证书校验。
```

## 6. 功能验收清单

对每一台条件样本逐项执行并记录证据。所有功能模块保持与上游一致，未经削减。

| 编号 | 功能 | 操作路径 | 通过判据 | 证据字段 |
| --- | --- | --- | --- | --- |
| F1 | 远程连接 | 控制端打开客户端连接列表 | 被控端在线，标识显示 `config.json` 中的 TAG；心跳保持 > 5 min 不重连 | 连接时间、TAG、重连次数 |
| F2 | 文件管理 | 右键客户端 → File Manager | 能列目录、上传、下载、改名、删除；上传后 SHA256 与源一致 | 操作列表、文件名、哈希 |
| F3 | 远程桌面 | 右键 → Remote Desktop | 画面实时刷新，鼠标键盘控制生效，分辨率/多显示器切换可用 | 帧率、输入延迟、截图 |
| F4 | 键盘记录 | 右键 → Keylogger | 能启停记录，日志文件产生于 `%APPDATA%\Logs`（`logDirectoryName`），内容可回读 | 日志路径、条数、时间跨度 |
| F5 | 密码恢复 | 右键 → Password Recovery | 恢复浏览器/邮箱等凭据列表并落盘，可导出 | 条目数、恢复来源、导出文件 |

补充可选验收（保持功能完整性核对用）：Remote Shell、Registry Editor、Task Manager、Startup Manager、TCP Connections、Reverse Proxy、System Information、Shutdown/Restart。

## 7. 检测侧数据采集与对照

每个条件样本采集以下数据，填入检出矩阵：

| 数据源 | 采集方式 | 关注点 |
| --- | --- | --- |
| Microsoft Defender | 事件日志 `Microsoft-Windows-Windows Defender/Operational` (1116/1117/1006/1015) | 是否检出、检出名称、处置动作 |
| Sysmon | `Microsoft-Windows-Sysmon/Operational` (1/3/11/12/13/22) | 进程创建链、网络连接、文件落地、注册表、DNS |
| EDR | 按产品导出告警与遥测 | 告警级别、规则 ID、阻断与否 |
| 网络侧 | 交换机镜像 / 主机抓包 | 4782 的 TLS 会话建立时间与频次 |

检出矩阵（示例骨架，实验时逐格填充）：

| 条件（TAG） | 样本 SHA256 | Defender | Sysmon | EDR | 盲区 |
| --- | --- | --- | --- | --- | --- |
| COND-A |  |  |  |  |  |
| COND-B |  |  |  |  |  |

## 8. 可复现性验证

1. **构建可复现**：连续两次 `build_release.ps1`（同一参数、`-SkipBuild` 关闭）产出的 `LabBaseline.exe` 应具有相同元数据、相同依赖结构；文件哈希允许因签名时间戳与编译器差异变化，因此**以 MANIFEST 记录为准**，跨次对比使用清单。
2. **条件切换不重构**：使用 `repoint_condition.ps1` 切换 `hosts/tag/subnet` 后，样本 SHA256 不变（只有 `config.json` 变化）——这是「同一份构建对照不同条件」的关键约束。
3. **兜底不变**：删除 `config.json` 后运行验证脚本，必须回落到上游行为（`initialize_result=False`，即 Release 下无编译期参数时不启动），证明未放宽任何逻辑。

## 9. 回滚

- 实验结束：恢复 VM 快照 `lab-baseline-clean`。
- 信任回滚：
  ```powershell
  Get-ChildItem Cert:\LocalMachine\Root, Cert:\LocalMachine\TrustedPublisher |
    Where-Object { $_.Subject -like '*Detection Evaluation Laboratory*' } | Remove-Item
  ```
- 样本侧代码回滚：`git checkout -- .`（本仓库改动均在版本控制内）。

---

## 10. 本轮已完成的自动化验证（本机，2026 实验台构建会话）

| 校验项 | 命令 | 实测结果 | 判定 |
| --- | --- | --- | --- |
| 解决方案构建 | `build_release.ps1` STEP 1 | 零错误；Client.exe 经 ILRepack 合并 5 个程序集 | 通过 |
| 元数据标准化 | STEP 2 断言 | ProductName=Quasar Detection Baseline；Company=Detection Evaluation Laboratory；FileVersion=1.4.1.0；版权行统一 | 通过 |
| 依赖整合 | ILRepack merge | Client.exe 3,346,944 字节，已内联 protobuf-net / MouseKeyHook / Quasar.Common / BouncyCastle | 通过 |
| 外部配置生效 | `verify_baseline.ps1` | `initialize_result=True`；HOSTS=10.20.30.40:4782；MUTEX/TAG/SUBNET 全部来自 config.json | 通过 |
| 证书校验未降级 | 同上 | SERVERCERT_THUMBPRINT=9C0831…665CB 与实验台 CA 一致，SHA256 签名校验通过 | 通过 |
| 兜底路径不变 | `verify_fallback.ps1` | 无 config.json 时 `config_present=False`、`initialize_result=False` | 通过 |
| 连接烟测（外部覆盖） | `smoke_connect.ps1` | `connection_observed=True`，连到 config.json 指定的 127.0.0.1:47821 | 通过 |
| 单文件投递 | 删除 config.json 后 `smoke_connect.ps1` | `connection_observed=True`，连到 exe 内嵌的 127.0.0.1:47822 | 通过 |
| 出包控制台 | `Quasar.exe --lab-packager` | 窗口标题 `Quasar 实验室一键出包控制台`，进程正常驻留 | 通过 |
| 参数窗体 | `LabBaseline.exe --config` | 窗口标题 `Quasar 实验室基线样本 - 参数配置`，进程正常驻留 | 通过 |
| 图标规范化 | `extract_icon.ps1` + `compare_icon.py` | 与标准素材像素级一致（mean_abs_diff=0.0） | 通过 |
| 本地自签名 | `signtool sign /fd SHA256 /sha1` | Quasar.exe 与 LabBaseline.exe 均签名成功；靶机导入证书后 Status=Valid | 通过 |

> 说明：`Get-AuthenticodeSignature` 在**未导入证书的构建机**上显示 `UnknownError` 属预期（自签名根不受信），导入 `LabBaselineSigner.cer` 后即为 `Valid`。

## 11. 交付清单

- `build_release.ps1` —— 一键可复现构建（构建→元数据校验→依赖合并→证书→基线样本→签名→清单→验证→烟测）
- `lab/` —— 图标素材、工具链、说明文档
- `dist/LabBaseline/` —— 被控端基线样本 + config.json + MANIFEST.txt
- `dist/ControlConsole/` —— 控制端及其依赖、实验台 CA、client.bin
- `dist/LabBaselineSigner.cer`、`dist/LabServerCA.cer` —— 靶机信任预置用
- 本报告 —— 两台 Win10 靶机隔离内网验证流程与判定标准
