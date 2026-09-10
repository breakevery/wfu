# Changelog

本文件记录 WFU 项目的所有重要变更。
格式参考 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，版本号遵循语义化版本。

## [Unreleased]

### Added
- **WFU.Core：实现 M1 核心模块（2026-09-10）**
  - `Services/IFileService.cs` / `Services/FileService.cs`：异步文件读写，写入时自动创建目录，读取不存在文件抛 `FileNotFoundException`。
  - `Services/ISettingsStore.cs` / `Services/SettingsStore.cs`：基于 `System.Text.Json` 的 `settings.json` 持久化，内部使用 `Dictionary<string, object>`。
  - `PluginLoader.cs`：`Assembly.LoadFrom` 扫描目录加载 `IPlugin` 实现，容错不中断，`UnloadAll()` 调用 `Dispose()`。
  - `WFU.Core.csproj`：新增对 `WFU.PluginSDK` 的 `ProjectReference`。
  - 新增 `docs/Core.md`：详细记载核心模块契约、行为与设计取舍。
  - 验证：`dotnet build` 0 警告、0 错误。
- **WFU.PluginSDK：正式落地插件契约（2026-09-10）**
  - `IPlugin`：插件基础接口，定义生命周期方法 `Initialize()` / `Execute()` / `Dispose()`。
  - `PluginAttribute`：插件元数据特性，标注名称（Name）/ 版本（Version）/ 描述（Description）。
  - `ILanguagePack`：语言包契约，提供 `GetString(string key)`，支撑「零硬编码文本」。
  - `IThemeProvider`：主题契约，提供背景色 / 前景色 / 字体名称 / 字号。
  - 所有公开类型均带完整 XML 文档注释。
- **文档**
  - 新增 `docs/PluginSDK.md`：详细记载插件契约、生命周期、加载机制与示例。
  - 新增本 `CHANGELOG.md`。

### Notes
- 命名空间统一为 `WFU.PluginSDK`，目标框架 `net8.0`。
- 本阶段仅定义契约，尚未在 `WFU.Core` 中实现插件加载器（见 Roadmap M2）。
