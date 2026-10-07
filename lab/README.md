# 实验室基线样本构建目录（lab/）

> ### 作者：陈森（Chen Sen）· <https://github.com/chendashi666>
> **本项目所有实验室工程改造与新增代码均为陈森创作。禁止盗卖。禁止商业用途。**
> 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有，遵循 MIT License，其署名与许可证必须原样保留。
> 详见 **[NOTICE.md](NOTICE.md)** 与 **[LICENSE.LAB.md](LICENSE.LAB.md)**。

本目录是本实验台的样本侧工程资产，配合仓库根目录的 `build_release.ps1` 使用。
所有产物均为**实验室基线样本**，仅用于隔离检测评估实验台的对照测量。

## 目录结构

| 路径 | 用途 |
| --- | --- |
| `lab/LabBaseline.ico` | 实验室标准图标（16/24/32/48/64/128/256 多尺寸，统一应用到控制端与被控端 exe） |
| `lab/LabBaseline.png` | 图标的 256×256 参考图 |
| `lab/tools/make_icon.py` | 重新生成标准图标（Pillow） |
| `lab/tools/verify_baseline.ps1` | 运行期验证：加载样本，调用 `Settings.Initialize()`，回显生效参数 |
| `lab/tools/verify_fallback.ps1` | 兜底验证：无 `config.json` 时必须回落上游行为 |
| `lab/tools/smoke_connect.ps1` | 连接烟测：本机监听回环端口，确认样本按 `config.json` 发起连接 |
| `lab/tools/extract_icon.ps1` | 提取 exe 内嵌图标用于资源核对 |
| `lab/tools/compare_icon.py` | 图标像素比对（与标准素材一致性） |
| `lab/tools/repoint_condition.ps1` | 切换实验条件：只重写 `config.json`，不重新构建、不重新签名 |
| `lab/tools/open_packager.cmd` | 双击启动「一键出包控制台」（等价 `Quasar.exe --lab-packager`） |
| `lab/tools/trust_lab_certificate.ps1` | 靶机信任预置：导入签名证书与实验台 CA（需管理员） |

## 一键出包（图形控制台）

双击 [open_packager.cmd](lab/tools/open_packager.cmd)，或命令行：

```powershell
.\dist\ControlConsole\Quasar.exe --lab-packager
```

界面里填 **控制端内网 IP**（可多行 / 多个用 `;` 分隔），可选填端口、TAG、SUBNET、MUTEX、输出目录，点一次「一键出包」：

- 直接产出可直接投递的被控端 exe（运行参数已内嵌，单文件即可运行）
- 默认同时在同目录写一份 `config.json` 作为覆盖层，用于后续切换实验条件
- 日志区回显产物路径、文件大小、SHA256、内嵌参数

> 出包依赖 `dist\ControlConsole\client.bin` 与 `quasar.p12`；缺失时先用 [build_release.ps1](build_release.ps1) 完成一次构建。

## 一键构建

```powershell
.\build_release.ps1 -Hosts "10.10.10.10:4782;" -Tag "COND-A" -Subnet "10.10.10.0/24"
```

参数：

| 参数 | 默认 | 说明 |
| --- | --- | --- |
| `-Hosts` | `10.10.10.10:4782;` | 控制端地址，多条用 `;` 分隔；内网参数自行填写 |
| `-Port` | `4782` | 仅写入 config.json 记录；端口以 `-Hosts` 中的为准 |
| `-Tag` | `COND-A` | 实验条件标签，用于区分对照组 |
| `-Subnet` | `10.10.10.0/24` | 内网网段元数据 |
| `-Mutex` | 自动 GUID | 单实例互斥名；同一条件下应保持固定 |
| `-OutputRoot` | `dist` | 产物根目录 |
| `-RotateServerCertificate` | 关 | 重新生成实验台 CA（会使旧样本失效） |
| `-SkipBuild` / `-SkipSign` / `-SkipSmokeTest` | 关 | 跳过对应阶段（用于快速重跑） |

## 产物

```
dist/
  LabBaseline/
    LabBaseline.exe     被控端基线样本（功能完整，依赖已 ILRepack 合并，已签名）
    config.json         运行参数（五项 + 证书绑定），唯一需要改动的文件
    MANIFEST.txt        构建参数、Git 提交、签名指纹、文件 SHA256
  ControlConsole/
    Quasar.exe          控制端（已签名）
    quasar.p12          实验台 CA（控制端加载）
    client.bin          控制端生成自定义客户端用的模板
    *.dll               控制台运行依赖
  LabBaselineSigner.cer 签名证书公钥（导入靶机受信存储）
  LabServerCA.cer       实验台 CA 公钥（导入靶机受信存储）
```

## 切换实验条件（不重新构建）

```powershell
.\lab\tools\repoint_condition.ps1 -QuasarExe .\dist\ControlConsole\Quasar.exe `
    -BaselineDir .\dist\LabBaseline -Hosts "10.10.20.10:4782;" -Tag "COND-B" -Subnet "10.10.20.0/24"
```

或直接在靶机上双击 `LabBaseline.exe --config` 打开参数窗体填写五项并保存 `config.json`。

## 运行参数读取顺序

样本 exe 内嵌一份完整实验参数（出包时写入），外置 `config.json` 是覆盖层：

1. **内嵌层**：打包控制台写入 exe 的 `Quasar.LabBaseline.config.json` 资源。
2. **覆盖层**：exe 同目录 `config.json` 中**显式出现**的字段覆盖内嵌值，未出现的字段继承内嵌值。
3. **兜底**：两层都不存在 → 完全走上游行为（Release 无编译期参数时不启动），行为不变。

因此：

- 只投递 exe 一个文件即可运行（无需带 dll、无需带 json）；
- 切换实验条件只改同目录 `config.json` 的 `hosts/tag/subnet`，样本 SHA256 不变；
- 两层都带实验台证书绑定，仍执行与上游一致的 SHA256 服务器证书校验，不降级。

## 参数说明（五项）

| 字段 | 说明 |
| --- | --- |
| `HOSTS` | 控制端地址，不带端口时自动补 `PORT` |
| `PORT` | 控制端监听端口 |
| `MUTEX` | 单实例互斥名，留空自动生成 GUID |
| `TAG` | 实验条件标签 |
| `SUBNET` | 内网网段元数据。上游 Quasar 1.4.1 已无消费点，仅作为实验记录元数据透传，不改变行为 |

## 重新生成图标

```powershell
& "<python>" .\lab\tools\make_icon.py
```
