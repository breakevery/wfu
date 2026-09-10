# WFU Bridge 说明（M3 — WebView2 集成与 C#↔JS 双向桥接）

> 本文档详细记载 WFU 预览层（M3）的 WebView2 集成方式、C#↔JS 双向桥接机制、
> 异步初始化竞态的处理、以及遇到的坑与已知问题。

- **桥接程序集**：`WFU.Bridge.dll`（目标框架 `net8.0`，命名空间 `WFU.Bridge`）
- **宿主控件**：`WFU.Host/Views/WebViewHost.xaml(.cs)`
- **WebView2 包**：Microsoft.Web.WebView2 `1.0.4191.47`
- **WebView2 Runtime**：本机已安装 `152.0.4191.66`

---

## 1. 结构

```
JS (页面)  ⇄  window.chrome.webview.hostObjects.wfu   ⇄  WfuBridgeObject (C#)
JS (页面)  ⇄  WebViewHost.ExecuteScriptAsync(script)   ⇄  C# 主动调用
```

| 项目 | 角色 |
|------|------|
| `WFU.Bridge` | 只放「暴露给 JS 的对象」，不依赖 WPF |
| `WFU.Host` | 通过 `WebViewHost` 控件承载 WebView2，负责初始化与桥接注册 |

`WFU.Host.csproj` 通过 `ProjectReference` 引用 `WFU.Bridge`；`WFU.Bridge` 目标框架为 `net8.0`
（`WFU.Host` 为 `net8.0-windows`，可正常引用 `net8.0` 类库）。

---

## 2. `WfuBridgeObject`（暴露给 JS 的对象）

命名空间 `WFU.Bridge`，类标记 `[ComVisible(true)]`。JS 调用语法：

```js
await window.chrome.webview.hostObjects.wfu.方法名(参数...)
```

| 方法 | 签名 | 说明 |
|------|------|------|
| `ShowMessage` | `void ShowMessage(string message)` | JS→C# 同步（fire-and-forget）演示 |
| `GetTimestamp` | `string GetTimestamp()` | JS→C# 带返回值演示 |
| `SaveFileAsync` | `Task<bool> SaveFileAsync(string path, string content)` | JS→C# 异步演示，可被 JS `await` |
| `Ping` | `string Ping()` | 返回 `wfu-bridge-ok`，用于确认对象已挂载 |

> 关键：异步方法必须返回 `Task` / `Task<T>`，**不能是 `async void`**，否则 JS 侧无法 `await` 到结果。

---

## 3. `WebViewHost`（WebView2 宿主控件）

### 3.1 异步初始化（竞态防护）

WebView2 的 `CoreWebView2` 是**异步准备**的，`EnsureCoreWebView2Async()` **只能调用一次**，
且在其完成前访问 `CoreWebView2` 会 `NullReferenceException`。`WebViewHost` 的处理：

- `InitializeAsync()`：先检查 `_initialized`（`lock` 保护）——已初始化直接返回（幂等）；
- `await PreviewWebView.EnsureCoreWebView2Async();` 完成后才注册桥接对象与设置；
- 设置完成后才把 `_initialized = true`；
- 任何异常都会包成带友好提示的 `InvalidOperationException`（提示检查 WebView2 Runtime 与目标框架）；
- 所有 `CoreWebView2` 访问前先经 `EnsureInitialized()` 校验，未初始化时抛带说明的异常。

### 3.2 暴露接口

| 方法 | 说明 |
|------|------|
| `Task InitializeAsync()` | 异步初始化（幂等） |
| `void LoadHtml(string html)` | `NavigateToString` |
| `void LoadUrl(string url)` | `Navigate` |
| `Task<string> ExecuteScriptAsync(string script)` | C# → JS 执行脚本 |
| `bool IsWebViewInitialized` | 初始化状态（**注意**：不能叫 `IsInitialized`，会与 `FrameworkElement.IsInitialized` 冲突产生 CS0108） |

### 3.3 初始化时设置的项

```csharp
core.AddHostObjectToScript("wfu", _bridgeObject);   // 暴露桥接对象
core.Settings.AreDevToolsEnabled = true;            // 开发期允许 F12
core.Settings.AreDefaultContextMenusEnabled = true; // 允许右键菜单
```

---

## 4. 最小闭环验证页

`WFU.Host/Resources/test-bridge.html` 作为**嵌入资源**打包，`MainWindow` 在 `Loaded` 时读取并载入：

```xml
<EmbeddedResource Include="Resources\test-bridge.html" />
```

页面提供三个按钮，分别调用 `ShowMessage` / `GetTimestamp` / `SaveFileAsync`，并在 `#log` 区显示结果。

---

## 5. 遇到的坑与处理（重要）

1. **`EnableComHosting=true` 会导致编译失败**
   报 `NETSDK1088: COMVisible 类"..."必须具有 GuidAttribute`。
   原因：`EnableComHosting` 是给**原生 COM 客户端**用的（生成 `.comhost.dll`），而
   `AddHostObjectToScript` **不需要**它。**处理：移除 `EnableComHosting`**，仅保留 `[ComVisible(true)]`。

2. **`TextEditor.Text` 不可绑定**（M2 已解决）：AvalonEdit 的 `Text` 非依赖属性，用附加属性桥接。

3. **`IsInitialized` 命名冲突**：`UserControl` 继承自 `FrameworkElement`，已有 `IsInitialized`。
   自定义同名属性会产生 CS0108 警告。**处理：改名为 `IsWebViewInitialized`**。

4. **`hostObjects` 不能漏**：JS 正确写法是 `window.chrome.webview.hostObjects.wfu.Xxx(...)`；
   写成 `window.chrome.webview.wfu.Xxx(...)` 会失败。

5. **布局列号**：M2 的三列内容区实际是 5 个 `ColumnDefinition`（两个竖向 `GridSplitter` 各占一列），
   右侧预览区是 **Column 4**（不是 `Grid.Column="2"`）。

---

## 6. 验证结果（2026-09-10）

| # | 项目 | 结果 |
|---|------|------|
| 1 | `dotnet build` | ✅ 0 警告、0 错误 |
| 2 | 启动 + 右侧加载 test-bridge.html | ✅ WebView2 初始化完成，测试页已加载 |
| 3 | JS→C# 同步 `ShowMessage` | ✅ 控制台输出 `[JS→C#] Hello from JS!` |
| 4 | JS→C# 返回值 `GetTimestamp` | ✅ 页面显示 `✅ 时间戳: 2026-09-10 11:21:25` |
| 5 | JS→C# 异步 `SaveFileAsync` | ✅ 生成 `bridge-test.txt`，内容 `Written by JS` |
| 6 | C#→JS `ExecuteScriptAsync` | ✅ 页面 `#log` 变为“C# 主动调用 JS 成功” |
| 7 | F12 打开 DevTools | ✅ 出现 `DevTools - about:blank` 窗口 |

---

## 7. 已知技术债 / 后续

1. **预览尚未与编辑器联动**：右侧固定加载测试页；后续应把 `EditorContent` 实时渲染到 WebView。
2. **`SaveFileAsync` 使用相对路径**：会落在进程工作目录；后续应由 C# 侧统一管理路径与安全校验。
3. **未处理导航/新窗口事件**：后续需接管 `NavigationStarting`、`NewWindowRequested` 等。
4. **`WfuBridgeObject` 尚无宿主回调**：`ShowMessage` 目前只写日志，后续应回调到 WPF 层（需注意跨线程）。
5. **CWD 污染**：桥接自检会在工作目录生成 `bridge-test.txt`（已加入 `.gitignore`）。

---

## 8. 变更记录

| 日期 | 变更 |
|------|------|
| 2026-09-10 | 新增 `WFU.Bridge`、`WebViewHost`、桥接测试页，打通 C#↔JS 双向通信。 |

---

*本文件随桥接机制演进持续更新。修改桥接接口或 WebView 初始化逻辑时，请同步更新本文件与 `CHANGELOG.md`。*
