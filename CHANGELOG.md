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
- **M6 项目导出/导入（2026-09-11）**
  - 新建项目：从 `Templates/` 复制标准骨架（`index.html` / `app.js` / `style.css` / `unity-bridge.js`）。
  - 左侧文件树：显示项目内 `.html/.js/.css`，双击打开。
  - 导出为 ZIP：外层套 `{项目名}/` 目录（符合 Unity `StreamingAssets` 习惯）。
  - 导入项目：解压到 zip 同级目录，自动识别项目根；目录已存在时确认合并。
  - CanExecute 机制：未打开项目时导出按钮禁用（WFU 首个用例）。
  - 新增 `docs/Export.md`：项目导出/导入完整文档。
- **M6-3 导入功能：打开已导出的 zip 项目（2026-09-11）**
  - `MainViewModel` 新增 `ImportCommand` + `ResolveProjectRoot()`（自动识别 zip 内的 `{项目名}/` 层级）。
  - 解压到 zip 同级同名目录；目标已存在时弹窗确认合并（否 → 取消；是 → 合并）。
  - 含 **zip slip 安全防护**（校验解压目标不逃出目标目录）。
  - `MainWindow.xaml`：File 菜单新增 `Import Project...`；工具栏新增 `Import`。
  - `ApplyLanguage()` 补 `MenuFileImport.Header` / `ToolbarImport.Content`。
  - EnglishPack 新增 6 个 key（`menu_file_import` / `dialog_import_title` / `status_import_success` / `status_import_failed` / `confirm_import_overwrite_title` / `confirm_import_overwrite_msg`），现共 52 条。
  - 验证：首次导入 / 重复导入（否→取消、是→合并）/ 降级 全 PASS。
- **M6-2 Export 导出功能（含首个 CanExecute 机制）（2026-09-11）**
  - `MainViewModel` 新增 `ExportCommand`（`CanExecute = HasProject`）+ `CanExport()`；`_currentProjectPath` 加 `[NotifyCanExecuteChangedFor(nameof(ExportCommand))]`（WFU **首个 CanExecute 用例**）。
  - 导出实现：`ZipArchive` 逐文件添加，zip 外层套 `{项目名}/` 目录（`CompressionLevel.Optimal`）。
  - `MainWindow.xaml`：File 菜单新增 `Export as ZIP...`；`ToolbarExport` 绑定 `ExportCommand`。
  - `ApplyLanguage()` 补 `MenuFileExport.Header`。
  - EnglishPack 新增 5 个 key（`menu_file_export` / `dialog_export_title` / `status_export_success` / `status_export_failed` / `status_no_project`），现共 46 条。
  - 验证：未打开项目时按钮禁用；新建项目后启用；导出 zip 结构为 `WFU-Test/{4 文件}` 且可正常解压；降级显示中文、不崩溃。
- **M6-1b-2 项目系统：新建项目 + 文件树（2026-09-11）**
  - `MainViewModel` 新增 `CurrentProjectPath`（`[ObservableProperty]`）、`ProjectFiles`（`ObservableCollection<string>`）、`HasProject`，以及 `NewProjectCommand`。
  - `NewProject`：弹出保存对话框选目录 → `CreateDirectory` → 把 `Templates/` 下 4 个模板写入 → 设为当前项目 → 自动打开 `index.html`。
  - 新增 `RefreshProjectFiles()` / `GetFullPath()` / `OpenProjectFileAsync()`。
  - `MainWindow.xaml`：File 菜单新增 `New Project...`、工具栏新增 `New Project`，`LeftPanel` TreeView 绑定 `ProjectFiles` + 双击打开。
  - `MainWindow.xaml.cs`：新增 `ProjectTree_MouseDoubleClick`；`ApplyLanguage()` 补 2 项。
  - EnglishPack 新增 6 个 key（`menu_file_new_project` / `toolbar_new_project` / `status_project_created` / `status_project_failed` / `status_project_opened` / `dialog_new_project_title`），现共 41 条。
  - 验证：新建项目/双击打开/修改保存/降级 4 项全 PASS；`dotnet build` 0 警告 0 错误。
- **M6-1b-1 新增项目模板文件（2026-09-11）**
  - 新增 `src/WFU.Host/Templates/`：`index.html` / `app.js` / `style.css` / `unity-bridge.js`。
  - `unity-bridge.js` 提供 preview 模式的 mock（Unity 宿主侧会覆盖）。
  - `WFU.Host.csproj` 新增 `Content` + `PreserveNewest`：自动复制模板到输出目录与 publish。
  - 验证：`dotnet build` 0 警告 0 错误；Debug 与 publish 输出均带 4 个模板文件。
- **M6-1a 扩展 IFileService：新增目录级 API（2026-09-11）**
  - `IFileService` / `FileService` 新增 `DirectoryExists` / `CreateDirectory` / `EnumerateFiles`（非递归）。
  - 现有 3 个文件方法签名不变；**临时解冻 `WFU.Core`**（M6-1 完成后重新冻结）。
  - 为 M6-1 项目模板系统（生成项目骨架 + 文件树）提供目录抽象。
  - 验证：`dotnet build` 0 警告 0 错误；程序启动行为无变化。
- **M5-4 publish 自动复制 Plugins + 收尾本地化（2026-09-11）**
  - `WFU.Host.csproj` 新增 `CopyPluginsToPublish` Target（`AfterTargets="Publish"`）：自动把 `bin\<Config>\net8.0-windows\Plugins` 复制到 `publish\Plugins`；源目录缺失时输出 Warning。
  - EnglishPack 新增 `status_webview_ready` key（现共 **35** 条）；`MainWindow` 启动就绪状态改走语言包（在 `ApplyLanguage` 中刷新）。
  - 验证：publish 日志出现 `Copying 6 plugin files ...`；发布版自动加载 2 个插件；状态栏显示 `WebView2 Ready`；降级显示中文硬编码。
- **M5-3 状态栏接入语言包（2026-09-11）**
  - `MainViewModel` 新增 `ILanguagePack?` 字段 + `SetLanguagePack()` 注入 + `T(key, fallback)` + `RefreshLanguage()`。
  - 状态消息本地化：`status_new_file` / `status_opened` / `status_open_failed` / `status_saved` / `status_save_failed`。
  - 新增 `SaveStateText` 属性（替代 XAML 的 Style DataTrigger）。
  - `MainWindow.LoadPlugins()` 后向 ViewModel 注入语言包；`ApplyLanguage()` 处理状态栏静态标签（`status_line` / `status_col`）。
  - EnglishPack 新增 3 个 key（`status_new_file` / `status_open_failed` / `status_save_failed`），现共 34 条。
  - 验证：8 项状态栏验证全 PASS；语言包生效（临时值验证）；降级显示中文硬编码、不崩溃。
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
- `IFileService` 扩展目录级 API（`DirectoryExists` / `CreateDirectory` / `EnumerateFiles`，M6-1a）。
- 工具栏 `Import` 按钮改用独立 key `toolbar_import`（不带省略号，M6-4a）。

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
