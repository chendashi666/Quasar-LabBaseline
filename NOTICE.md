# 版权与使用声明 · NOTICE

## 作者

**本项目（Quasar 检测评估实验台 · Quasar-LabBaseline）的实验室工程改造与新增代码，全部由 陈森（Chen Sen）独立创作完成。**

- GitHub：<https://github.com/chendashi666>
- 仓库：<https://github.com/chendashi666/Quasar-LabBaseline>

作者创作范围包括但不限于：

- 运行参数外置体系（`LabConfig` 模型、三层配置优先级、WinForms 参数配置窗体）
- 实验室元数据标准化与字符串资源集中（`Directory.Build.props`、`LabResources.resx`）
- 依赖整合与单文件合并（`ILRepack.targets`）、资源规范化（实验室标准图标）
- 本地自签名与靶机信任预置流程
- 一键可复现构建流水线 `build_release.ps1`
- 一键出包控制台与独立出包工具 `QuasarLabToolkit`
  （`FrmLabPackager`、`LabBuildCli`、`LabConfigFactory`、`LabCertificate`）
- 验证方案与验收矩阵 `VERIFICATION_REPORT.md`

## 使用限制（针对陈森创作的实验室工程部分）

> ### 禁止盗卖。禁止任何形式的商业用途。

- 禁止将本项目（含全部或部分源码、构建产物、发布包）用于任何商业目的；
- 禁止以任何形式转售、倒卖、付费分发本项目或其衍生作品；
- 禁止删除、篡改、隐藏本声明以及源码、界面、构建产物中的作者署名；
- 允许在保留完整署名的前提下，用于个人学习、研究与自建检测评估靶场。

## 上游代码

本项目是 [Quasar](https://github.com/quasar/Quasar) 的衍生工程。
上游 Quasar 代码的版权归 **MaxXor 及 Quasar 贡献者** 所有，遵循 **MIT License**（见 `LICENSE`），
其许可条款不受本声明影响。上游部分的版权署名与许可证必须原样保留，不得移除。

第三方组件许可证见 `Licenses/`。

## 一句话

实验室工程 = **陈森（Chen Sen）** 创作，**禁止盗卖、禁止商业用途**；
上游 Quasar = MIT，署名归 MaxXor。
