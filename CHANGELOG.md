# Changelog

本文件记录 WFU 项目的所有重要变更。
格式参考 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，版本号遵循语义化版本。

## [Unreleased]

### Added
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

### Notes
- 命名空间统一为 `WFU.PluginSDK`，目标框架 `net8.0`。
- 本阶段仅定义契约，尚未在 `WFU.Core` 中实现插件加载器（见 Roadmap M2）。
