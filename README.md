# WFU — WebView IDE for Unity

> A lightweight, plugin-based IDE for building mini-apps that run inside Unity applications via WebView.
>
> 一个轻量级、可插拔的 IDE，用于构建运行在 Unity 应用程序内部的 WebView 小程序。

[![GitHub](https://img.shields.io/badge/GitHub-breakevery%2Fwfu-181717?logo=github)](https://github.com/breakevery/wfu)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](https://opensource.org/licenses/Apache-2.0)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows-blue)]()
[![Status](https://img.shields.io/badge/status-v0.1.0--alpha-orange)]()

---

> 📦 **This is the source-code repository. / 这是源码仓库。**
>
> Pre-built binaries and releases are hosted on a separate repository:
> 预编译的二进制发行版托管在另一个仓库：
>
> 👉 **Binary Releases:** *Coming soon / 待公布*

---

## 📖 Table of Contents / 目录

- [Introduction / 简介](#-introduction--简介)
- [Philosophy / 设计哲学](#-philosophy--设计哲学)
- [Features / 功能特性](#-features--功能特性)
- [Architecture / 架构](#-architecture--架构)
- [Build from Source / 从源码构建](#-build-from-source--从源码构建)
- [Project Structure / 项目结构](#-project-structure--项目结构)
- [Plugin Development / 插件开发](#-plugin-development--插件开发)
- [Roadmap / 路线图](#-roadmap--路线图)
- [Contributing / 贡献指南](#-contributing--贡献指南)
- [License & Commercial / 许可证与商业授权](#-license--commercial--许可证与商业授权)

---

## 📌 Introduction / 简介

**English** | WFU (WebView For Unity) is a minimalist IDE that lets developers write HTML / CSS / JavaScript mini-apps and run them inside Unity applications through WebView. It provides a tiny native core with file editing, live preview, WebView2-based C#↔JS bridging, and a plugin system — everything else is extensible via plugins.

This repository contains the **full source code** of the IDE. Compiled binaries are distributed separately (see the banner above).

**中文** | WFU（WebView For Unity）是一个极简 IDE，让开发者编写 HTML / CSS / JavaScript 小程序，并通过 WebView 运行在 Unity 应用程序内部。它提供一个极小的原生核心，包含文件编辑、实时预览、基于 WebView2 的 C#↔JS 双向通信与插件系统——其他一切皆可通过插件扩展。

本仓库包含 IDE 的 **完整源代码**。编译后的二进制发行版在独立仓库中分发（见上方横幅）。

---

## 🧠 Philosophy / 设计哲学

> **"Core is law, plugins are choice."**  
> **"内核为王，插件自由。"**

| Principle / 原则 | Meaning / 含义 |
|------------------|----------------|
| **Microkernel / 微内核** | 原生核心只做 4 件事：读/写、编辑、调试、设置。 |
| **Plugin Everything / 全部插件化** | 语言包、主题、工具栏、格式化——全是插件。 |
| **Interface First / 接口优先** | 插件必须实现 `IPlugin` 或其派生接口才能被加载。 |
| **Zero Hardcoded Text / 零硬编码文本** | 内核不含任何展示文本，全部从 `ILanguagePack` 获取。 |
| **Graceful Degradation / 优雅降级** | 插件缺失时程序仍可运行，回退到硬编码与默认样式。 |

---

## ✨ Features / 功能特性

| Module / 模块 | Description / 说明 |
|---------------|---------------------|
| 🖥️ **Editor Host / 编辑器宿主** | WPF + AvalonEdit，支持语法高亮、行号、Consolas 14pt |
| 📂 **File Service / 文件服务** | 异步读/写 `.html` `.js` `.css` 及纯文本 |
| 🔗 **C# ↔ JS Bridge / 双向桥接** | 通过 `AddHostObjectToScript` 暴露 C# 对象给 JavaScript |
| 🌐 **WebView2 Preview / 实时预览** | 内嵌 Microsoft Edge WebView2，支持 F12 DevTools |
| ⚙️ **Settings Persistence / 设置持久化** | 窗口位置/大小自动保存，含虚拟屏边界校验 |
| 🔌 **Plugin System / 插件系统** | 扫描 `Plugins/` 目录，反射加载实现 `IPlugin` 的 DLL |
| 🌍 **Language Pack / 语言包** | `ILanguagePack` 接口，官方附带 EnglishPack |
| 🎨 **Theme Provider / 主题** | `IThemeProvider` 接口，官方附带 BasicTheme（深色） |
| 🛡️ **Global Exception Handler / 全局异常** | AppDomain + Dispatcher 双通道，程序不闪退 |

---

## 🏗️ Architecture / 架构

```

┌───────────────────────────────────────────────────────────────┐
│                    WFU Native Core / 原生内核                 │
│  ┌──────────────┐ ┌──────────────┐ ┌───────────────────────┐ │
│  │ FileService  │ │ EditorHost   │ │ DebugPipe (WebSocket) │ │
│  │ 文件服务      │ │ 编辑器宿主    │ │ 调试管道              │ │
│  └──────────────┘ └──────────────┘ └───────────────────────┘ │
│  ┌──────────────┐ ┌──────────────────────────────────────┐   │
│  │SettingsStore │ │ PluginLoader (Assembly.Load + 反射)   │   │
│  │设置存储       │ │ 插件加载器                            │   │
│  └──────────────┘ └──────────────────────────────────────┘   │
└───────────────────────────────────────────────────────────────┘
│
▼
┌───────────────────────────────────────────────────────────────┐
│                   Plugin Layer / 插件层                       │
│  ┌────────────┐ ┌────────────┐ ┌────────────────────────┐    │
│  │EnglishPack │ │BasicTheme  │ │  (Your plugin here)    │    │
│  │英语语言包   │ │深色主题     │ │  (你的插件)             │    │
│  └────────────┘ └────────────┘ └────────────────────────┘    │
└───────────────────────────────────────────────────────────────┘
│
▼
┌───────────────────────────────────────────────────────────────┐
│              Unity Host Application / Unity 宿主              │
│    (Loads exported .zip mini-apps via WebView)                │
│    (通过 WebView 加载导出的小程序 zip)                          │
└───────────────────────────────────────────────────────────────┘

```

---

## 🔧 Build from Source / 从源码构建

### Prerequisites / 环境要求

- **Windows 10 / 11 (64-bit)**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)（构建需要；运行时只需 Desktop Runtime）
- [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/)（Win10/11 通常已内置）

### Build & Run / 构建与运行

```bash
# Clone the repository / 克隆仓库
git clone https://github.com/breakevery/wfu.git
cd wfu

# Restore dependencies / 还原依赖
dotnet restore

# Build (Debug) / 构建（Debug）
dotnet build

# Run / 运行
dotnet run --project src/WFU.Host
```

Release Build / Release 构建

```bash
dotnet publish src/WFU.Host/WFU.Host.csproj \
  -c Release -r win-x64 \
  --self-contained false \
  -p:PublishSingleFile=true \
  -o publish
```

Output: publish/WFU.Host.exe + publish/Plugins/
产物：publish/WFU.Host.exe + publish/Plugins/

---

📁 Project Structure / 项目结构

```
WFU/
├── src/
│   ├── WFU.Core/                 # Native core / 原生核心
│   │   ├── Services/             # FileService, SettingsStore / 文件服务、设置存储
│   │   └── PluginLoader.cs       # Reflection-based plugin loader / 反射加载器
│   ├── WFU.PluginSDK/            # Plugin contracts / 插件契约
│   │   ├── IPlugin.cs
│   │   ├── ILanguagePack.cs
│   │   ├── IThemeProvider.cs
│   │   └── PluginAttribute.cs
│   ├── WFU.Bridge/               # C# ↔ JS bridge objects / 桥接对象
│   │   └── WfuBridgeObject.cs
│   └── WFU.Host/                 # WPF main application / WPF 主程序
│       ├── Views/
│       ├── ViewModels/
│       ├── Converters/
│       └── Resources/
├── plugins/                      # Official plugins / 官方插件
│   ├── EnglishPack/
│   └── BasicTheme/
├── docs/                         # Documentation / 文档
│   ├── Alpha.md
│   ├── Bridge.md
│   ├── Core.md
│   ├── PluginSDK.md
│   ├── Plugins.md
│   └── UI.md
├── README.md
├── CHANGELOG.md
└── LICENSE
```

---

🔌 Plugin Development / 插件开发

Minimal Plugin Template / 最小插件模板

1. Create a .csproj:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <OutputPath>..\..\src\WFU.Host\bin\$(Configuration)\net8.0-windows\Plugins\YourPlugin\</OutputPath>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\WFU.PluginSDK\WFU.PluginSDK.csproj">
      <Private>false</Private>
    </ProjectReference>
  </ItemGroup>
</Project>
```

2. Implement a plugin class:

```csharp
using WFU.PluginSDK;

[Plugin("MyPlugin", "1.0.0", "My first plugin")]
public class MyPlugin : IPlugin, ILanguagePack
{
    public void Initialize() { /* Called on load */ }
    public void Execute()    { /* Called on trigger */ }
    public void Dispose()    { /* Called on unload */ }

    public string GetString(string key) => key switch
    {
        "menu_file" => "File",
        "menu_edit" => "Edit",
        _ => key
    };
}
```

3. Build and deploy / 构建部署:

```bash
dotnet build plugins/YourPlugin/YourPlugin.csproj -c Release
# DLL will be copied to src/WFU.Host/bin/.../Plugins/YourPlugin/
```

Critical Notes / 关键注意事项

· ⚠️ Must implement IPlugin — even if the 3 lifecycle methods are empty.
· ⚠️ Do NOT copy WFU.PluginSDK.dll into the plugin folder — use <Private>false</Private>.
· ⚠️ Key names must match ILanguagePack.GetString() calls in the main app.
· ⚠️ Plugin failure is isolated — a broken plugin won't crash the app.

For a full guide, see docs/Plugins.md.

完整指南见 docs/Plugins.md。

---

🗺️ Roadmap / 路线图

✅ Completed / 已完成

☑ M1 — Core modules (FileService, SettingsStore, PluginLoader)
☑ M2 — WPF main window + AvalonEdit integration + MVVM
☑ M3 — WebView2 integration + C#↔JS bridge
☑ M4 — Plugin system (EnglishPack + BasicTheme) + graceful degradation
☑ v0.1.0-alpha — Source code stabilized, binary release pipeline established

🔨 Planned / 规划中

☐ M5 — Interface refactoring (ILanguagePack : IPlugin), 8 missing keys, status bar localization
☐ M6 — One-click export .zip for Unity deployment
☐ Code formatter plugin (Prettier-style)
☐ Git integration plugin
☐ Chinese / Japanese language packs
☐ Linux / macOS support (WebKitGTK)

---

🤝 Contributing / 贡献指南

English | Contributions are welcome! We especially welcome:

· 🌍 New language packs
· 🎨 New themes
· 🔧 Tool / utility plugins
· 🐛 Bug fixes
· 📖 Documentation improvements

How to contribute:

1. Fork the repository
2. Create a feature branch (git checkout -b feature/amazing)
3. Commit your changes (git commit -m 'Add amazing feature')
4. Push to branch (git push origin feature/amazing)
5. Open a Pull Request

中文 | 欢迎贡献！我们特别欢迎：

· 🌍 新语言包
· 🎨 新主题
· 🔧 工具/实用插件
· 🐛 Bug 修复
· 📖 文档改进

贡献步骤：

1. Fork 本仓库
2. 创建功能分支（git checkout -b feature/amazing）
3. 提交更改（git commit -m 'Add amazing feature'）
4. 推送到分支（git push origin feature/amazing）
5. 提交 Pull Request

---

📄 License & Commercial / 许可证与商业授权

This project uses a dual-licensing model.

本项目采用 双许可模式。

1. Open-Source License / 开源许可证

Licensed under the Apache License, Version 2.0 — see LICENSE for details.

采用 Apache 2.0 开源协议 —— 详见 LICENSE 文件。

Free for / 免费用于：

· ✅ Personal use / 个人使用
· ✅ Educational use / 教育用途
· ✅ Internal business use / 内部商业用途
· ✅ Open-source projects (even commercial) / 开源项目（含商业开源）

Requirements / 要求：

· Must retain copyright notice & disclaimer / 必须保留版权声明与免责声明
· Must state modifications / 必须声明对源码的修改

2. Commercial License / 商业授权

For closed-source integration, custom development, or enterprise support, please contact:

如需 闭源集成、定制开发 或 企业支持，请联系：

Contact / 联系方式 Value / 值
GitHub https://github.com/breakevery/wfu
Organization / 组织 Breakevery
Email (Primary / 主要) chenyindrager@outlook.com
Email (Backup / 备用) 1404807068@qq.com
Subject / 邮件主题 [Commercial License] Your Company

Benefit / 权益 Included / 包含
No open-source obligation / 无开源义务 ✅
Priority technical support / 优先技术支持 ✅
Custom development / 定制开发 ✅ (on request)
Legal indemnification / 法律担保 ✅

---

⚠️ Disclaimer: All contributors retain their respective copyrights. The dual-licensing model does not affect the open-source status of Apache 2.0; it merely provides an alternative commercial channel for users with specific needs.

⚠️ 免责声明： 所有贡献者保留各自著作权。双许可模式不影响 Apache 2.0 的开源属性，仅为有特定需求的用户提供额外的商业授权通道。

---

🙏 Acknowledgments / 致谢

· AvalonEdit — WPF code editor control
· Microsoft.Web.WebView2 — Edge WebView2 runtime
· CommunityToolkit.Mvvm — MVVM helpers

---

Built with ❤️ by the Breakevery team.
For inquiries: chenyindrager@outlook.com

```