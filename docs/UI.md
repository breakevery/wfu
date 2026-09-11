# WFU UI 架构说明（M2）

> 本文档详细记载 WFU 主界面（M2 视觉层）的布局结构、MVVM 绑定约定、AvalonEdit 集成方式、
> 全局异常捕获策略与已知技术债。

- **程序集**：`WFU.Host.dll`
- **目标框架**：`net8.0-windows`（WPF）
- **依赖包**：CommunityToolkit.Mvvm `8.2.0`、AvalonEdit `6.3.0.90`、Microsoft.Web.WebView2 `1.0.4191.47`
- **命名空间**：`WFU.Host`、`WFU.Host.ViewModels`、`WFU.Host.Behaviors`

---

## 1. 主窗口布局

`MainWindow.xaml` 采用 **5 行 Grid**：

| 行 | 内容 | 高度 |
|----|------|------|
| Row 0 | 菜单栏（Menu：File / Edit / View / Help） | `Auto` |
| Row 1 | 工具栏（ToolBar：New / Open / Save / 分隔 / Preview / Export） | `Auto` |
| Row 2 | 主内容区（Grid，三列） | `*` |
| Row 3 | 分隔条（GridSplitter） | `Auto` |
| Row 4 | 状态栏（StatusBar） | `Auto` |

主内容区（Row 2）的三列：

| 列 | 宽度 | 内容 |
|----|------|------|
| Col 0 | `250` | 左侧文件树（`TreeView`，暂留空） |
| Col 1 | `*` | AvalonEdit 编辑器 |
| Col 2 | `400` | 右侧预览区（占位，M3 接入 WebView2） |

列之间各有一个竖向 `GridSplitter`（`ResizeBehavior="PreviousAndNext"`），用于拖拽调整宽度。

---

## 2. MVVM 绑定要点

### 2.1 MainViewModel 属性清单

`ViewModels/MainViewModel.cs` 继承 `CommunityToolkit.Mvvm.ComponentModel.ObservableObject`，
使用 `[ObservableProperty]` 标注私有字段，源生成器自动生成同名公开属性：

| 字段 | 生成属性 | 类型 | 默认值 | 用途 |
|------|----------|------|--------|------|
| `_currentFilePath` | `CurrentFilePath` | `string` | `"Untitled"` | 当前文件路径 |
| `_editorContent` | `EditorContent` | `string` | `""` | 编辑器内容 |
| `_statusText` | `StatusText` | `string` | `"Ready"` | 状态栏文本 |
| `_currentLine` | `CurrentLine` | `int` | `1` | 光标行号 |
| `_currentColumn` | `CurrentColumn` | `int` | `1` | 光标列号 |
| `_isModified` | `IsModified` | `bool` | `false` | 是否有未保存修改 |

### 2.2 命令绑定映射表

命令由 `[RelayCommand]` 标注的方法生成（方法 `Foo` → 命令 `FooCommand`）：

| 界面元素 | 绑定命令 |
|----------|----------|
| File ▸ New | `NewFileCommand` |
| File ▸ Open | `OpenFileCommand` |
| File ▸ Save | `SaveFileCommand` |
| File ▸ Exit | `ExitAppCommand` |
| Edit ▸ Undo / Redo / Cut / Copy / Paste | `UndoCommand` / `RedoCommand` / `CutCommand` / `CopyCommand` / `PasteCommand`（占位） |
| View ▸ Toggle Left Panel | `ToggleLeftPanelCommand`（占位） |
| View ▸ Toggle Right Panel | `ToggleRightPanelCommand`（占位） |
| Help ▸ About | `AboutCommand` |
| 工具栏 New / Open / Save | 同 File 菜单对应命令 |
| 工具栏 Preview / Export | 暂未绑定（占位） |

### 2.3 状态栏实时更新机制

- 行/列号：代码后置订阅 AvalonEdit 的 `Editor.TextArea.Caret.PositionChanged`，
  转发调用 `MainViewModel.NotifyCaretPositionChanged(line, column)`，更新 `CurrentLine` / `CurrentColumn`。
- 内容修改标记：AvalonEdit 的 `TextChanged` 事件转发到 `NotifyEditorTextChanged()`，将 `IsModified` 置为 `true`。
- 状态栏通过 `StatusBar` 中的 `TextBlock` 绑定 `StatusText` / `CurrentLine` / `CurrentColumn`。
- **M5-3 变更**：状态栏文本改为**语言包驱动**——动态文本（状态消息、`已保存`/`● 已修改`、`行:`/`列:`）均来自 `ILanguagePack`；
  `SaveStateText` 属性替代了原 XAML 的 `Style DataTrigger`；`MainWindow.LoadPlugins()` 后调用 `SetLanguagePack()` 注入，
  无语言包时回退中文硬编码。

> 约定：代码后置（`.xaml.cs`）**只做界面事件转发**，不写业务逻辑；业务逻辑全部在视图模型中。

---

## 3. AvalonEdit 集成

### 3.1 为什么不直接写 `Text="{Binding ...}"`

AvalonEdit 的 `TextEditor.Text` 是**普通 CLR 属性，不是依赖属性（DependencyProperty）**。
在 XAML 中对其使用 `Binding` 会抛出：

```
System.Windows.Markup.XamlParseException:
不能在“TextEditor”类型的“Text”属性上设置“Binding”。
只能在 DependencyObject 的 DependencyProperty 上设置“Binding”。
```

（这会导致应用在启动阶段直接崩溃。）

### 3.2 方案：附加属性桥接

`Behaviors/EditorTextBinding.cs` 提供可双向绑定的附加属性 `EditorTextBinding.Text`：

- 注册附加依赖属性 `Text`，`FrameworkPropertyMetadataOptions.BindsTwoWayByDefault`。
- 属性变化（视图模型 → 编辑器）时写入 `editor.Text`；首次绑定时订阅 `editor.TextChanged`，
  把用户输入回写（编辑器 → 视图模型），并用布尔守卫避免循环更新。
- XAML 用法：

```xml
xmlns:behaviors="clr-namespace:WFU.Host.Behaviors"

<avalonedit:TextEditor x:Name="Editor"
    ShowLineNumbers="True" FontFamily="Consolas" FontSize="14"
    behaviors:EditorTextBinding.Text="{Binding EditorContent, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
    TextChanged="Editor_TextChanged"/>
```

XAML 仍然保持声明式绑定，MVVM 未被破坏。

> 命名空间务必为 `http://icsharpcode.net/sharpdevelop/avalonedit`。

---

## 4. 全局异常捕获

`App.xaml.cs` 装配**双重捕获**：

| 捕获点 | 事件 | 作用 |
|--------|------|------|
| 非 UI 线程 | `AppDomain.CurrentDomain.UnhandledException` | 记录/提示无法拦截的致命异常 |
| UI 线程 | `Application.DispatcherUnhandledException` | 弹窗提示，并置 `e.Handled = true` **阻止闪退** |

要点：`DispatcherUnhandledException` 必须设置 `e.Handled = true`，否则异常会继续传播导致进程退出；
`AppDomain.UnhandledException` 只能用于提示/记录，无法阻止崩溃。

> 实测价值：M2 首次运行时正是这套捕获把 AvalonEdit 绑定异常以弹窗形式暴露出来，
> 而不是静默闪退，从而快速定位问题。

---

## 5. 已知技术债

1. **构造函数内同步等待异步**：`MainWindow` 构造函数无法 `async`，临时验证代码与初始化里使用了
   `GetAwaiter().GetResult()`（因 `FileService` 内部 `ConfigureAwait(false)`，暂无死锁）。后续应改为异步初始化。
2. **Edit 菜单为空实现**：Undo / Redo / Cut / Copy / Paste 目前为占位命令，未接入编辑器实际行为。
3. **View 菜单为空实现**：Toggle Left / Right Panel 未实现真实折叠逻辑。
4. **工具栏 Preview / Export 未绑定**：等待 M3（WebView2 预览）与后续导出功能。
5. **硬编码显示文本**：界面文本暂用中文占位，后续切换为 `ILanguagePack`。
6. **右侧预览区为占位**：M3 将挂载 WebView2 并建立 C# ↔ JS 桥接。

---

## 6. 变更记录

| 日期 | 变更 |
|------|------|
| 2026-09-10 | 实现 M2 主窗口骨架、MVVM、AvalonEdit 集成与全局异常捕获。 |

---

*本文件随 UI 演进持续更新。修改界面或视图模型时请同步更新本文件与 `CHANGELOG.md`。*
