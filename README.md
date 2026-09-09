# wfu (Webview IDE for Unity)
A WebView-based IDE for building embedded mini-apps within Unity


# WebView-IDE for Unity Mini-Apps

> An integrated development environment (IDE) built with WPF + WebView2 for creating and previewing mini-apps that run inside Unity applications.

[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](https://opensource.org/licenses/Apache-2.0)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows-blue)]()

---

## 📖 Table of Contents / 目录

- [Introduction / 介绍](#-introduction--介绍)
- [Features / 特性](#-features--特性)
- [Architecture / 架构](#-architecture--架构)
- [Quick Start / 快速开始](#-quick-start--快速开始)
- [Project Structure / 项目结构](#-project-structure--项目结构)
- [Plugin Development / 插件开发](#-plugin-development--插件开发)
- [Contributing / 贡献指南](#-contributing--贡献指南)
- [License & Commercial Authorization / 许可证与商业授权](#-license--commercial-authorization--许可证与商业授权)

---

## 📌 Introduction / 介绍

**English** | This project aims to build a lightweight IDE that allows developers to create mini-apps running inside a Unity application via WebView. The IDE features real-time code editing, live preview, a plugin extension system, and shared database access — with the final output being loadable directly into a Unity host.

**中文** | 本项目旨在构建一个轻量级 IDE，让开发者能够创建运行在 Unity 应用程序内部的"小程序"（基于 WebView）。IDE 提供实时代码编辑、即时预览、插件扩展系统和数据库共享能力——最终产物可直接加载到 Unity 宿主程序中。

---

## ✨ Features / 特性

| English | 中文 |
|---------|------|
| 🖥️ **WPF + WebView2** — Modern UI with embedded Chromium engine | 🖥️ **WPF + WebView2** — 现代化界面，内嵌 Chromium 引擎 |
| ✏️ **Code Editor** — Syntax highlighting, line numbers, auto-completion (AvalonEdit) | ✏️ **代码编辑器** — 语法高亮、行号显示、自动补全（AvalonEdit） |
| 🔄 **Live Preview** — Real-time preview of mini-apps within the IDE | 🔄 **即时预览** — 在 IDE 内实时预览小程序效果 |
| 🗄️ **Shared Database** — SQLite support, accessible from both C# and JavaScript | 🗄️ **数据库共享** — SQLite 支持，C# 和 JavaScript 均可访问 |
| 🔌 **Plugin System** — Extensible via DLL plugins (IPlugin interface) | 🔌 **插件系统** — 通过 DLL 插件扩展（IPlugin 接口） |
| 📦 **One-Click Export** — Package mini-apps as .zip for Unity deployment | 📦 **一键导出** — 将小程序打包为 .zip 供 Unity 加载 |
| 🌐 **C# ↔ JS Bridge** — Bidirectional communication between .NET and WebView | 🌐 **C# ↔ JS 桥接** — .NET 与 WebView 双向通信 |

---

## 🏗️ Architecture / 架构

```

┌─────────────────────────────────────────────────────────────┐
│                    WebView-IDE (WPF)                        │
│  ┌─────────────┐  ┌─────────────┐  ┌───────────────────┐  │
│  │  AvalonEdit  │  │  File Tree  │  │  WebView2 Preview │  │
│  │  (Editor)    │  │  (Project)  │  │  (Live Preview)   │  │
│  └──────┬──────┘  └──────┬──────┘  └─────────┬─────────┘  │
│         │                │                     │            │
│         └────────────────┼─────────────────────┘            │
│                          │                                  │
│  ┌───────────────────────▼───────────────────────────────┐  │
│  │              ASP.NET Core Embedded HTTP Server         │  │
│  │                   (localhost:5000)                     │  │
│  └───────────────────────┬───────────────────────────────┘  │
│                          │                                  │
│  ┌───────────────────────▼───────────────────────────────┐  │
│  │              SQLite Database (Shared Access)           │  │
│  └───────────────────────┬───────────────────────────────┘  │
│                          │                                  │
│  ┌───────────────────────▼───────────────────────────────┐  │
│  │              Plugin Loader (Assembly.Load)             │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────┐
│              Unity Host Application                         │
│         (Loads exported .zip mini-apps via WebView)         │
└─────────────────────────────────────────────────────────────┘

```

---

## 🚀 Quick Start / 快速开始

### Prerequisites / 环境要求

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) (Windows 10/11 通常已内置)

### Build & Run / 构建与运行

```bash
# Clone the repository / 克隆仓库
git clone https://github.com/[your-username]/[repo-name].git
cd [repo-name]

# Restore dependencies / 恢复依赖
dotnet restore

# Build the project / 构建项目
dotnet build -c Release

# Run the IDE / 运行 IDE
dotnet run --project src/IDE.Editor
```

Create Your First Mini-App / 创建你的第一个小程序

1. Open the IDE and create a new project / 打开 IDE，新建项目
2. Write HTML/CSS/JavaScript code in the editor / 在编辑器中编写 HTML/CSS/JavaScript
3. Click Preview to see the result in WebView2 / 点击 预览 在 WebView2 中查看效果
4. Click Export to generate a .zip package / 点击 导出 生成 .zip 包
5. Load the .zip in your Unity app via WebView / 在 Unity 应用中通过 WebView 加载该 .zip

---

📁 Project Structure / 项目结构

```
WebView-IDE/
├── src/
│   ├── IDE.Core/                 # Core interfaces & shared models / 核心接口与共享模型
│   │   ├── Plugin/               # IPlugin interface / 插件接口定义
│   │   └── Models/               # Data models / 数据模型
│   ├── IDE.Editor/               # WPF UI / 主界面
│   │   ├── Views/                # MainWindow, dialogs / 主窗口、对话框
│   │   ├── ViewModels/           # MVVM ViewModels
│   │   └── Services/             # Editor services / 编辑器服务
│   ├── IDE.Bridge/               # C# ↔ JS communication / C# 与 JS 通信桥接
│   │   ├── HttpServer/           # ASP.NET Core embedded server
│   │   └── WebViewBridge/        # AddHostObjectToScript wrapper
│   └── IDE.PluginSDK/            # SDK for plugin developers / 插件开发 SDK
├── samples/                      # Example mini-apps / 示例小程序
├── docs/                         # Documentation / 文档
├── tests/                        # Unit tests / 单元测试
├── README.md                     # This file / 本文件
└── LICENSE                       # Apache License 2.0
```

---

🔌 Plugin Development / 插件开发

To create a plugin, implement the IPlugin interface:

```csharp
using IDE.Core.Plugin;

[Plugin("MyPlugin", "1.0.0", "Plugin description")]
public class MyPlugin : IPlugin
{
    public void Initialize() { /* Called when plugin is loaded */ }
    public void Execute()    { /* Called when plugin is triggered */ }
    public void Dispose()    { /* Called when plugin is unloaded */ }
}
```

Build your plugin as a .dll and drop it into the Plugins/ folder — the IDE will load it automatically via Assembly.Load reflection.

---

🤝 Contributing / 贡献指南

Contributions are welcome! Please read CONTRIBUTING.md for details on our code of conduct and the process for submitting pull requests.

欢迎贡献！请阅读 CONTRIBUTING.md 了解行为准则和提交 Pull Request 的流程。

---

📄 License & Commercial Authorization / 许可证与商业授权

This project is released under a dual-licensing model to balance open-source freedom and commercial sustainability.

本项目采用 双许可模式，以平衡开源自由与商业可持续性。

---

1. Open-Source License / 开源许可证

The project is licensed under the Apache License, Version 2.0 – see the LICENSE file for details.

本项目默认采用 Apache 2.0 开源协议 —— 详见 LICENSE 文件。

Under this license, you are free to:

· Use the code for personal, educational, or internal business purposes
· Modify and distribute the code
· Use it in open-source projects (even commercial open-source)

Requirements: You must retain the copyright notice, disclaimer, and state any modifications.

在本协议下，你可以：

· 将代码用于 个人、教育或内部商业用途
· 修改和分发代码
· 在开源项目（含商业开源）中使用

要求： 必须保留版权声明、免责声明，并声明对源码的修改。

---

2. Commercial License / 商业授权

If your use case does not fully comply with Apache 2.0 requirements, or if you need to integrate this project into closed-source / proprietary software without disclosing source code, or if you require custom development, dedicated support, or indemnification, please contact us for a Commercial License.

如果你的使用场景无法完全遵守 Apache 2.0 的全部条款，或者需要将此项目集成到 闭源/专有软件 中且无需公开源码，或者需要 定制开发、专属技术支持或法律担保，请联系我们获取 商业授权。

Commercial Licensing benefits include:

· ✅ No obligation to open-source your derivative code
· ✅ Priority technical support
· ✅ Custom feature development (upon request)
· ✅ Legal protection and indemnification clauses

商业授权权益包括：

· ✅ 无开源衍生代码的义务
· ✅ 优先技术支持
· ✅ 定制功能开发（按需）
· ✅ 法律保护与免责条款

---

📧 Contact for Commercial License / 商业授权联系方式

Email: your-email@example.com
Subject: [Commercial License] Your Company / Project Name

Please include in your email:

· Your company/organization name
· Intended use case and deployment scale
· Any specific customization requirements

邮箱： your-email@example.com
邮件主题： [商业授权] 您的公司/项目名称

邮件中请附上：

· 您的公司/组织名称
· 预期用途与部署规模
· 任何特定的定制需求

---

⚠️ Disclaimer: All contributors retain their respective copyrights. The dual-licensing model does not affect the open-source status of Apache 2.0; it merely provides an alternative commercial channel for users with specific needs.

⚠️ 免责声明： 所有贡献者保留各自著作权。双许可模式不影响 Apache 2.0 的开源属性，仅为有特定需求的用户提供额外的商业授权通道。

---

🙏 Acknowledgments / 致谢

· AvalonEdit — WPF code editor control
· WebView2 — Microsoft Edge WebView2
· SQLite — Lightweight embedded database

```

---

### 📝 后续操作清单

- [ ] 将 `[your-username]` 替换为你的 GitHub 用户名
- [ ] 将 `[repo-name]` 替换为你的仓库名
- [ ] 将 `your-email@example.com` 替换为你的真实联系邮箱
- [ ] 在仓库根目录创建 `LICENSE` 文件（粘贴 [Apache 2.0 全文](https://www.apache.org/licenses/LICENSE-2.0)）
- [ ] 可选择创建 `CONTRIBUTING.md`、`CODE_OF_CONDUCT.md`、`CHANGELOG.md`
- [ ] 将以上 README.md 完整内容复制到你的仓库中

祝你的开源项目顺利起步！如果还需要其他辅助文件（如 `CONTRIBUTING.md` 模板、`.gitignore` 模板等），随时告诉我。🎉
```
