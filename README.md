# WFU — WebView IDE for Unity

> A lightweight, plugin-based IDE for building mini-apps that run inside Unity applications via WebView.

[![GitHub](https://img.shields.io/badge/GitHub-breakevery%2Fwfu-181717?logo=github)](https://github.com/breakevery/wfu)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](https://opensource.org/licenses/Apache-2.0)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows-blue)]()
[![Status](https://img.shields.io/badge/status-development-yellow)]()

---

## 📖 Table of Contents

- [Introduction](#-introduction)
- [Philosophy](#-philosophy)
- [Features](#-features)
- [Architecture](#-architecture)
- [Roadmap](#-roadmap-2026--2027)
- [Quick Start](#-quick-start)
- [Project Structure](#-project-structure)
- [Plugin Development](#-plugin-development)
- [Contributing](#-contributing)
- [License & Commercial Authorization](#-license--commercial-authorization)

---

## 📌 Introduction

**English** | WFU (WebView For Unity) is a minimalist IDE that lets developers create HTML/CSS/JavaScript mini-apps and run them inside Unity applications through WebView.

It is **not** a full-featured IDE like VS Code. Instead, it provides a **tiny native core** (~200 lines) with file editing, local preview, debugging, and settings — everything else is a plugin.

**中文** | WFU（WebView For Unity）是一个极简 IDE，让开发者能够创建 HTML/CSS/JavaScript 小程序，并通过 WebView 运行在 Unity 应用程序内部。

它**不是**一个像 VS Code 那样的全功能 IDE。相反，它提供了一个**极小的原生核心**（约 200 行），只包含文件编辑、本地预览、调试和设置功能——其他一切皆为插件。

---

## 🧠 Philosophy

> **"Core is law, plugins are choice."**

| Principle | Meaning |
|-----------|---------|
| **Microkernel** | Native core does only 4 things: read/write, edit, debug, settings. |
| **Plugin Everything** | Language packs, themes, toolbars, formatters — all are plugins. |
| **Interface First** | Plugins must implement `IPlugin` or its derivatives to be loaded. |
| **Zero Hardcoded Text** | The core contains **no** display strings — all text comes from `ILanguagePack`. |

---

## ✨ Features

| Module | Description |
|--------|-------------|
| 🖥️ **Editor Host** | WPF + AvalonEdit with syntax highlighting, line numbers |
| 📂 **File Service** | Read/write `.html`, `.js`, `.css` files (no parsing) |
| 🐛 **Debug Pipe** | WebSocket log output via `localhost` |
| ⚙️ **Settings Store** | Persistent `settings.json` (font, theme, recent projects) |
| 🔌 **Plugin Loader** | Scan `Plugins/` folder and load `.dll` via `Assembly.Load` |
| 🔗 **C# ↔ JS Bridge** | Expose native methods to WebView2 JavaScript |
| 📦 **One-Click Export** | Package current project as `.zip` for Unity deployment |

---

## 🏗️ Architecture

```

┌─────────────────────────────────────────────────────────────────┐
│                        WFU Native Core                         │
│  ┌──────────────┐ ┌──────────────┐ ┌────────────────────────┐ │
│  │ FileService  │ │ EditorHost   │ │ DebugPipe (WebSocket)  │ │
│  └──────────────┘ └──────────────┘ └────────────────────────┘ │
│  ┌──────────────┐ ┌──────────────────────────────────────────┐ │
│  │ SettingsStore│ │ PluginLoader (Assembly.Load + Reflection)│ │
│  └──────────────┘ └──────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────────┐
│                    Plugin Layer (DLLs)                         │
│  ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐ │
│  │EnglishPack │ │BasicTheme  │ │FileTree    │ │StatusBar   │ │
│  └────────────┘ └────────────┘ └────────────┘ └────────────┘ │
│  ┌────────────┐ ┌────────────┐ ┌────────────────────────────┐ │
│  │HotkeyPlugin│ │Toolbar     │ │ (Your custom plugin here)  │ │
│  └────────────┘ └────────────┘ └────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────────┐
│                    Unity Host Application                      │
│              (Loads exported .zip via WebView)                  │
└─────────────────────────────────────────────────────────────────┘

```

---

## 🗺️ Roadmap (2026 – 2027)

### ✅ Completed

- [x] GitHub organization `breakevery` created
- [x] Repository initialized with Apache 2.0 license
- [x] Project architecture finalized (microkernel + plugins)

### 🔨 In Progress (Sep – Dec 2026)

| Milestone | Target | Deliverable |
|-----------|--------|-------------|
| **M1: Bare Core** | Sep 2026 | Editor can open/save files, English menu shows |
| **M2: Plugin Loader** | Oct 2026 | Scan `Plugins/` and load `.dll` with `IPlugin` |
| **M3: Debug Pipe** | Oct 2026 | WebSocket logs to debug panel |
| **M4: Official Plugins** | Nov 2026 | FileTree, StatusBar, Hotkey, Theme, Toolbar |
| **M5: Export** | Nov 2026 | One-click `.zip` packaging |
| **M6: Beta Release** | Dec 2026 | Single `.exe` published on Gitee |

### 📅 Future Plans (2027+)

- [ ] Linux support (WebKitGTK)
- [ ] Plugin Marketplace
- [ ] Chinese / Japanese language packs
- [ ] Code formatter plugin (Prettier)
- [ ] Git integration plugin
- [ ] Gitee mirror repository

---

## 🚀 Quick Start

### Prerequisites

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/)

### Build from Source

```bash
git clone https://github.com/breakevery/wfu.git
cd wfu
dotnet restore
dotnet build -c Release
dotnet run --project src/WFU.Core
```

Run Pre-built Binary

Download WFU.exe from Releases and run.

---

📁 Project Structure

```
WFU/
├── src/
│   ├── WFU.Core/                 # Native core (FileService, EditorHost, DebugPipe, SettingsStore)
│   ├── WFU.PluginSDK/            # Plugin interfaces (IPlugin, ILanguagePack, IThemeProvider)
│   └── WFU.Host/                 # WPF main window
├── plugins/                      # Official plugins
│   ├── EnglishPack/
│   ├── BasicTheme/
│   ├── FileTreePlugin/
│   ├── StatusBarPlugin/
│   ├── HotkeyPlugin/
│   └── ToolbarPlugin/
├── samples/                      # Example mini-apps
├── docs/                         # Documentation
├── tests/                        # Unit tests
├── README.md                     # This file
└── LICENSE                       # Apache License 2.0
```

---

🔌 Plugin Development

Implement the Base Interface

```csharp
using WFU.PluginSDK;

[Plugin("MyPlugin", "1.0.0", "Does something useful")]
public class MyPlugin : IPlugin
{
    public void Initialize() { /* Called on load */ }
    public void Execute()    { /* Called on trigger */ }
    public void Dispose()    { /* Called on unload */ }
}
```

Build & Deploy

```bash
dotnet build -c Release
# Copy .dll to WFU_HOME/Plugins/
```

Language Pack Plugin

```csharp
public class EnglishPack : ILanguagePack
{
    public string GetString(string key) => key switch
    {
        "menu_file" => "File",
        "menu_edit" => "Edit",
        "menu_view" => "View",
        "menu_help" => "Help",
        _ => key
    };
}
```

Theme Plugin

```csharp
public class DarkTheme : IThemeProvider
{
    public string BackgroundColor => "#1E1E1E";
    public string ForegroundColor => "#D4D4D4";
    public string FontFamily => "Consolas";
    public double FontSize => 14.0;
}
```

---

🤝 Contributing / 贡献指南

English | Contributions are welcome! Please follow these steps:

1. Fork the repository: https://github.com/breakevery/wfu
2. Create a feature branch (git checkout -b feature/amazing)
3. Commit your changes (git commit -m 'Add amazing feature')
4. Push to the branch (git push origin feature/amazing)
5. Open a Pull Request

We especially welcome:

· New language packs
· New themes
· Tool/utility plugins
· Bug fixes
· Documentation improvements

---

中文 | 欢迎贡献！请按以下步骤操作：

1. Fork 本仓库：https://github.com/breakevery/wfu
2. 创建功能分支（git checkout -b feature/amazing）
3. 提交更改（git commit -m 'Add amazing feature'）
4. 推送到分支（git push origin feature/amazing）
5. 提交 Pull Request

我们特别欢迎：

· 新语言包
· 新主题
· 工具/实用插件
· Bug 修复
· 文档改进

---

📧 Contact / 联系方式

GitHub: https://github.com/breakevery/wfu
Organization: Breakevery

Email (Primary / 主要): chenyindrager@outlook.com
Email (Backup / 备用): 1404807068@qq.com

---

📄 License & Commercial Authorization / 许可证与商业授权

This project uses a dual-licensing model.

本项目采用 双许可模式。

---

1. Open-Source License — Apache 2.0

Free for personal, educational, and open-source use.
Full terms: LICENSE

个人、教育和开源使用免费。
完整条款见 LICENSE 文件。

---

2. Commercial License / 商业授权

For closed-source integration, custom development, or enterprise support:

闭源集成、定制开发或企业支持，请联系：

📧 Email (Primary / 主要): chenyindrager@outlook.com
📧 Email (Backup / 备用): 1404807068@qq.com

Subject / 邮件主题: [Commercial License] Your Company / 您的公司

Please include in your email:

· Your company/organization name
· Intended use case and deployment scale
· Any specific customization requirements

邮件中请附上：

· 您的公司/组织名称
· 预期用途与部署规模
· 任何特定的定制需求

Benefit / 权益 Included / 包含
No open-source obligation / 无开源义务 ✅
Priority support / 优先技术支持 ✅
Custom development / 定制开发 ✅ (on request)
Indemnification / 法律担保 ✅

---

⚠️ Disclaimer: All contributors retain their respective copyrights. The dual-licensing model does not affect the open-source status of Apache 2.0.

⚠️ 免责声明： 所有贡献者保留各自著作权。双许可模式不影响 Apache 2.0 的开源属性。

---

🙏 Acknowledgments

· AvalonEdit — WPF code editor control
· WebView2 — Microsoft Edge WebView2
· SQLite — Lightweight embedded database

---

Built with ❤️ by the Breakevery team.
For inquiries: chenyindrager@outlook.com