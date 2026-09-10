# Changelog

本文件记录 WFU 项目的所有重要变更。
格式参考 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，版本号遵循语义化版本。

## [Unreleased]

### Added
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
