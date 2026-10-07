# Quasar 检测评估实验台 · 实验室基线样本

> ### 作者：陈森（Chen Sen）· <https://github.com/chendashi666>
> **本项目所有实验室工程改造与新增代码均为陈森创作。禁止盗卖。禁止商业用途。**
> 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有，遵循 MIT License，其署名与许可证必须原样保留。
> 详见 **[NOTICE.md](NOTICE.md)** 与 **[LICENSE.LAB.md](LICENSE.LAB.md)**。

上游 [Quasar](https://github.com/quasar/Quasar) 1.4.1 的可复现构建工程，用于企业终端检测栈
（Microsoft Defender / Sysmon / EDR）对远程管理工具的**覆盖度测量**与**盲区定位**。

- **功能完整**：远程连接、文件管理、远程桌面、键盘记录、密码恢复等功能模块原样保留，未删减、未降级
- **最小改动**：仅做参数外置、元数据标准化、构建自动化、资源规范化
- **无外部依赖**：不引入任何第三方加壳平台或检测规避服务
- **可复现**：一份构建 + 一个 `config.json` = 一个实验条件，切换条件不需要重新编译

仅用于自建隔离靶场中的检测能力评估。

---

## 1. 环境要求

| 组件 | 要求 |
| --- | --- |
| 操作系统 | Windows 10 / 11 x64（构建机） |
| Visual Studio | 2022 或更新，勾选「.NET 桌面开发」工作负载；也可只用 Build Tools |
| .NET Framework | 4.8 开发包（必须提供 net452 目标包所需的 v4.5.2 引用程序集） |
| Windows SDK | 可选，提供 `signtool.exe`；没有就加 `-SkipSign` |
| PowerShell | 5.1（Windows 自带） |
| 网络 | 首次 NuGet 还原需要联网，之后可完全离线 |

环境自检：

~~~powershell
[Environment]::Version                            # 应为 4.x
Test-Path "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2"
Get-ChildItem "C:\Program Files (x86)\Windows Kits\10\bin" -Recurse -Filter signtool.exe -ErrorAction SilentlyContinue
~~~

## 2. 首次构建（一条命令）

~~~powershell
git clone <仓库地址>
cd Quasar-LabBaseline
powershell -ExecutionPolicy Bypass -File .\build_release.ps1 -Hosts "<控制端内网IP>:4782;" -Tag "COND-A" -Subnet "<内网网段>"
~~~

流水线共 9 段，全部通过才算成功：构建 → 元数据断言 → 依赖合并校验 → 实验台证书 → 基线样本 →
控制端分发 → 本地签名 → 清单 → 运行验证与连接烟测。

独立于网络的控制端/被控端参数一律通过命令行传入，脚本正文不含任何硬编码地址。

## 3. 一键出包（图形控制台）

双击 `lab\tools\open_packager.cmd`，或：

~~~powershell
.\dist\ControlConsole\Quasar.exe --lab-packager
~~~

填入控制端内网 IP，点「一键出包」，直接得到**单文件**被控端：

- 运行参数（含实验台证书绑定）内嵌进 exe，投递时只需这一个文件
- 默认同目录再写一份 `config.json` 作为覆盖层，用于以后切换实验条件
- 日志区回显产物路径、大小、SHA256 与内嵌参数

## 3.1 独立出包工具（单文件 exe）

不想安装构建环境时，直接用预构建的独立工具：

~~~
prebuilt\LabToolkit\QuasarLabToolkit.exe   单文件 3.7 MB，含图形出包控制台 + 完整 CLI
prebuilt\LabToolkit\client.bin             客户端模板，必须与 exe 同目录
~~~

双击 exe 打开图形出包控制台；也支持与主控制端完全相同的命令行：

~~~powershell
.\QuasarLabToolkit.exe --help-lab
.\QuasarLabToolkit.exe --build-client --out .\LabBaseline\LabBaseline.exe --hosts "<控制端内网IP>:4782;" --tag "COND-A" --subnet "<内网网段>"
.\QuasarLabToolkit.exe --write-config --out .\LabBaseline --hosts "<控制端内网IP>:4782;"
.\QuasarLabToolkit.exe --make-cert
~~~

**首次运行会自动生成实验台 CA**：同目录没有 `quasar.p12` 时，工具直接创建一份，无需手工准备证书。

> 一套 CA 必须两端共用：工具生成的 `quasar.p12` 要复制到控制端目录（`dist\ControlConsole\quasar.p12`），
> 否则工具产出的样本无法通过控制端的服务器证书校验。**这个文件是私钥，不要外发、不要提交仓库。**

完整版控制端也带同一个出包界面：

~~~powershell
.\dist\ControlConsole\Quasar.exe --lab-packager
~~~

## 4. 被控端运行

被控端只需要 `LabBaseline.exe` 一个文件（纯单文件模式）。可选地在同目录放 `config.json` 覆盖参数。

~~~powershell
.\LabBaseline.exe            # 无人值守运行，无窗口无托盘
.\LabBaseline.exe --config   # 打开参数窗体，改完保存即写 config.json
~~~

进程名 `LabBaseline.exe`。默认 `INSTALL=false`、`STARTUP=false`，因此不自我复制、不写启动项，
任务管理器结束进程即彻底停止。

> 最容易踩的坑：纯单文件模式下不要删掉 exe 里的内嵌配置；带 `config.json` 时它必须与 exe **同目录**，
> 否则参数不生效。

## 5. 一键切换实验条件

~~~powershell
.\lab\tools\repoint_condition.ps1 -QuasarExe .\dist\ControlConsole\Quasar.exe `
    -BaselineDir .\dist\LabBaseline -Hosts "<控制端内网IP>:4782;" -Tag "COND-B" -Subnet "<内网网段>"
~~~

只重写 `config.json`，被控端 exe 的 SHA256 不变 —— 这是「同一份构建对照不同条件」的关键约束。

## 6. 参数优先级

| 层 | 来源 | 说明 |
| --- | --- | --- |
| 1 | exe 内嵌资源 | 出包时写入，单文件即可运行 |
| 2 | 同目录 `config.json` | 其中**显式出现**的字段覆盖内嵌值，未出现的继承内嵌值 |
| 3 | 编译期加密参数 | 只有前两层都不存在时才启用，与上游行为完全一致 |

三层都带实验台证书绑定，运行时仍执行与上游一致的 SHA256 服务器证书校验，未做任何降级。

## 7. 目录结构

~~~
build_release.ps1              一键可复现构建流水线
Directory.Build.props          统一元数据单一来源
lab/
  LabBaseline.ico/.png         实验室标准图标
  README.md                    实验室工具链说明
  tools/                       出包/校验/信任/图标工具脚本
Quasar.Client/                 被控端（参数外置 + 配置窗体）
Quasar.Common/                 共享协议与实验室配置模型
Quasar.Server/                 控制端（含一键出包控制台与 CLI 构建入口）
Quasar.LabToolkit/             独立出包工具：复用同一构建引擎，合并为单文件 exe
Quasar.Common.Tests/           单元测试
VERIFICATION_REPORT.md         两台 Win10 靶机隔离内网验证方案与验收矩阵
AGENTS.md                      AI 协作约定（可选，不参与构建）
~~~

## 8. 构建期 CLI（无 GUI）

~~~powershell
Quasar.exe --make-cert   [--out <p12>] [--name <CN>] [--key 4096]
Quasar.exe --build-client --out <exe> --hosts "<ip:port;>" [--tag --mutex --subnet --icon ...]
Quasar.exe --write-config --out <dir> --hosts "<ip:port;>" [--tag --mutex --subnet]
Quasar.exe --help-lab
~~~

## 9. 相对上游的改造清单

| 类别 | 改动 |
| --- | --- |
| 参数外置 | 新增 `LabConfig` 模型、WinForms 配置窗体、三层配置优先级；客户端 `Settings.Initialize()` 增加外置配置注入点 |
| 元数据标准化 | `Directory.Build.props` + 四个 `AssemblyInfo.cs` 统一 Title/Product/Company/Version/Copyright/Description，重新生成 AssemblyGuid |
| 字符串资源 | 新增 `LabResources.resx`，项目名/命名空间/默认安装路径/日志标记集中管理，模板标识改运行时拼接 |
| 依赖整合 | `ILRepack.targets` 显式合并清单 + 产物校验，被控端为单文件 |
| 资源规范化 | 统一多尺寸实验室图标、版本资源与文件描述 |
| 签名 | 本地自签名证书 + `signtool` 签名，配套靶机信任导入脚本 |
| 构建自动化 | `build_release.ps1` 九段流水线与构建期 CLI |
| 独立工具 | 新增 `Quasar.LabToolkit` 项目：复用同一个 `ClientBuilder`，ILRepack 合并为单文件 exe，缺证书时自动生成实验台 CA |
| 未改动 | 上线/心跳/消息处理器/所有功能模块与上游一致 |

## 10. 常见问题

**构建报 `MSB4126 解决方案配置无效`**
解决方案平台名是 `AnyCPU`（无空格），不是 `Any CPU`。

**找不到 `signtool.exe`**
安装 Windows SDK，或给 `build_release.ps1` 加 `-SkipSign`。

**被控端不上线**
按顺序查：① 被控端目标端口是否等于控制端实际监听端口（Quasar 默认 4782，界面里点 Listen 才监听）；
② 被控端能否 ping 通目标地址；③ 控制端入站防火墙是否放行；
④ 控制端的 `quasar.p12` 是否与出包时同一份（重新生成证书会让旧样本全部失联）；
⑤ 被控端 `netstat` 状态：一直 `SYN_SENT` 是网络不通，通了一下就断是 TLS 校验失败。

**`Get-AuthenticodeSignature` 显示 `UnknownError`**
自签名根未导入。在靶机执行 `lab\tools\trust_lab_certificate.ps1` 后即为 `Valid`。

## 11. 上游与许可

本项目是上游 Quasar 的分支，遵循其 **MIT License**（见 `LICENSE`，版权归 MaxXor 所有）。
第三方组件许可见 `Licenses/`。
本分支新增的实验室工程代码同样以 MIT 许可发布。
