# Changelog

本文件记录 WFU 项目的所有重要变更。
格式参考 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，版本号遵循语义化版本。

## [Unreleased]

### Fixed
- **[M1-FIX] 补齐设置持久化接线（2026-09-10）**
  - 修复：`SettingsStore` 从未被应用层调用（导致 `settings.json` 不生成）。
  - 提取 `_settingsStore` 为字段；`OnWindowLoaded` 中 `Load` + `RestoreWindowBounds`（含虚拟屏边界校验）。
  - `OnWindowClosing` 保存窗口位置/大小（最大化时保存 `RestoreBounds`）。
  - 验证：`settings.json` 正确生成/恢复，越界回退默认，S6.1 第 2 项转 PASS。
- **[FIX] 修复 AvalonEdit 双向绑定缺陷 + Alpha 验收（2026-09-10）**
  - 修复：`Behaviors/EditorTextBinding.cs` 改为**单向**（视图模型→编辑器）桥接，反向由 `MainWindow` 的 `TextChanged` 转发。
  - 修复：M2 遗留缺陷——编辑后 `New` / `Open` 无法刷新编辑器（回写覆盖了绑定）。
  - 新增：工具栏 `Preview` 实现（把 `EditorContent` 渲染到 WebView2）。
  - 新增：状态栏 IsModified 视觉指示器（`● 已修改` / `已保存`）。
  - 文档：新增 `docs/Alpha.md`（验收结果、修复说明、已知限制）；验收 **8/8 PASS**。
  - 验证：`dotnet build` 0 警告 0 错误。

### Added
- **M5-2 补全 EnglishPack 8 个缺失 key（2026-09-10）**
  - 新增 Edit 子项 5 个 key（`menu_edit_undo/redo/cut/copy/paste`）。
  - 新增 View 子项 2 个 key（`menu_view_toggle_left/right`）。
  - 新增工具栏 1 个 key（`toolbar_bridge_test`）。
  - `MainWindow.xaml` 为这 8 个控件添加 `x:Name`；`ApplyLanguage()` 补全动态赋值。
  - 验证：编译 0 警告 0 错误；语言包生效（临时值验证）；降级不崩溃。
- **M5-1 接口继承重构（2026-09-10）**
  - `IPlugin` 生命周期方法（`Initialize`/`Execute`/`Dispose`）改为**默认空实现**。
  - `ILanguagePack` / `IThemeProvider` 改为**继承 `IPlugin`**，插件类无需再重复实现生命周期方法。
  - `IThemeProvider` 新增 `EditorBackground` / `EditorForeground`（**带默认实现**，默认复用 `BackgroundColor` / `ForegroundColor`）。
  - 简化 `EnglishPack` / `BasicTheme` 插件类声明（移除 3 个空方法）。
  - 文档：更新 `docs/PluginSDK.md`。
- **M4 插件系统：EnglishPack + BasicTheme 上线（2026-09-10）**
  - `plugins/EnglishPack`：`ILanguagePack` 实现，覆盖菜单/工具栏 23 条 key。
  - `plugins/BasicTheme`：`IThemeProvider` 实现，深色主题 `#1E1E1E`。
  - `WFU.Host` 启动时自动扫描 `Plugins/`（根 + 一层子目录）加载插件。
  - `ApplyLanguage()` / `ApplyTheme()` 消费插件数据（语言包方案 B：代码后置硬替换）。
  - 优雅降级：插件缺失/加载失败时回退到硬编码与默认样式，程序不崩溃。
  - 新增 `docs/Plugins.md`：插件开发完整文档（加载机制 / 写插件 / 降级 / 故障排查）。
  - 验证：M4 端到端 15 项全部 PASS（含降级与 Alpha/M3 回归）。
- **WFU.Bridge / WFU.Host：实现 M3 WebView2 集成与 C#↔JS 双向桥接（2026-09-10）**
  - 新增 `WFU.Bridge` 类库（net8.0）：`WfuBridgeObject`（`[ComVisible(true)]`，含 `ShowMessage` / `GetTimestamp` / `SaveFileAsync` / `Ping`）。
  - 新增 `WFU.Host/Views/WebViewHost.xaml(.cs)`：WebView2 宿主控件，异步且幂等初始化，注册 `AddHostObjectToScript("wfu", ...)`，开启 DevTools。
  - `MainWindow`：右侧（Column 4）挂载 WebViewHost；`Loaded` 时初始化并加载嵌入的 `test-bridge.html`；工具栏新增 `Bridge Test` 自检按钮，View 菜单新增 `Test C# → JS`。
  - 新增嵌入资源 `Resources/test-bridge.html`。
  - 新增 `docs/Bridge.md`：记载桥接机制、竞态防护、遇到的坑与已知问题。
  - 偏差：移除 `EnableComHosting`（会导致 `NETSDK1088`，且 `AddHostObjectToScript` 不需要）；`IsInitialized` 改名 `IsWebViewInitialized`（避免 CS0108）。
  - 验证：`dotnet build` 0 警告 0 错误；JS→C#（同步/返回值/异步）与 C#→JS 全通，F12 可开 DevTools。
- **WFU.Host：实现 M2 主窗口 UI（2026-09-10）**
  - `ViewModels/MainViewModel.cs`：MVVM 视图模型（6 个 `[ObservableProperty]` + New/Open/Save/Exit 命令 + Edit/View/About 占位命令）。
  - `MainWindow.xaml`：5 行 Grid 布局（菜单/工具栏/三栏内容/分隔条/状态栏），集成 AvalonEdit（行号、Consolas 14）。
  - `Behaviors/EditorTextBinding.cs`：AvalonEdit 文本双向绑定附加属性桥接（其 `Text` 非依赖属性，无法直接绑定）。
  - `App.xaml.cs`：全局异常捕获（`AppDomain.UnhandledException` + `DispatcherUnhandledException`，后者置 `Handled=true` 防闪退）。
  - `WFU.Host.csproj`：新增 CommunityToolkit.Mvvm 8.2.0 / AvalonEdit 6.3.0.90 / Microsoft.Web.WebView2 包。
  - 新增 `docs/UI.md`：详细记载布局、MVVM 绑定、AvalonEdit 集成、异常捕获与已知技术债。
  - 验证：`dotnet build` 0 警告 0 错误；`dotnet run` 界面正常弹出，Open 测试（打开 .html → 内容入编辑器）通过。
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

### Changed
- 两个插件类改为同时实现 `IPlugin` + 能力接口（供 `PluginLoader` 识别）。
- `MainWindow.xaml` 给 15 个菜单项/工具栏按钮添加 `x:Name`。
- `MainWindow.xaml(.cs)` 增加窗口位置/大小持久化与 `Closing` 事件。

### Known Issues（计划 M5 处理）
- `IThemeProvider` 只有 4 属性，缺少 `EditorBackground`/`EditorForeground`（当前为接口外扩展）。
- `ILanguagePack` / `IThemeProvider` 未继承 `IPlugin`（当前靠多实现兼容）。
- `EnglishPack` 缺失 8 个 key（Edit 子项 5、Toggle Panel 2、Bridge Test 1）。
- 状态栏文本暂未接入语言包。

### Removed
- 移除各项目模板残留的空类 `Class1.cs`（`WFU.Core` / `WFU.PluginSDK` / `WFU.Bridge`，均无任何引用）。
- 清理仓库根目录的运行时残留 `bridge-test.txt`。

### Notes
- 命名空间统一为 `WFU.PluginSDK`，目标框架 `net8.0`。
- 本阶段仅定义契约，尚未在 `WFU.Core` 中实现插件加载器（见 Roadmap M2）。
