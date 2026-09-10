# WFU.PluginSDK — 插件契约文档

> 本文件详细记载 `WFU.PluginSDK` 程序集对外暴露的全部契约。
> 任何 WFU 插件都必须只依赖本程序集（`WFU.PluginSDK`），不应依赖核心 `WFU.Core`。

- **程序集**：`WFU.PluginSDK.dll`
- **命名空间**：`WFU.PluginSDK`
- **目标框架**：`net8.0`
- **协议**：Apache-2.0（另有商业授权，见仓库 README）

---

## 1. 概述

WFU 采用「微内核 + 插件」架构：核心（`WFU.Core`）只负责读写、编辑、调试、设置四件事，
其余一切（语言包、主题、工具栏、文件树……）均以插件形式提供。核心通过扫描 `Plugins/`
目录、用 `Assembly.Load` + 反射加载插件 DLL。

本程序集定义插件与核心之间的**稳定契约**。契约一旦发布应保持向后兼容。

| 类型 | 用途 |
|------|------|
| `IPlugin` | 插件基础契约（生命周期三方法） |
| `PluginAttribute` | 插件元数据标注（名称 / 版本 / 描述） |
| `ILanguagePack` | 语言包契约（零硬编码文本支持） |
| `IThemeProvider` | 主题契约（配色与字体） |

---

## 2. `IPlugin`

插件的基础接口。核心只在发现实现了 `IPlugin` 的公开类型时才会将其视为插件。

### 成员

| 成员 | 说明 |
|------|------|
| `void Initialize()` | 插件被加载并加入插件集时调用，用于初始化资源。 |
| `void Execute()` | 用户触发插件时调用，用于完成插件的主要工作。 |
| `void Dispose()` | 插件被卸载或宿主退出时调用，用于释放资源。 |

> **M5 变更**：三个生命周期方法均已提供**默认空实现**，实现 `IPlugin`（或其派生接口）的类无需重复编写。

### 生命周期顺序

```
发现插件类 → 实例化 → Initialize() → （可多次）Execute() → Dispose()
```

### 示例

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

> 约定：`Dispose()` 应可被安全重复调用（幂等）；`Execute()` 不应抛出未处理异常。

---

## 3. `PluginAttribute`

标注在插件类上的元数据特性。核心通过反射读取它，从而在**实例化之前**即可获得插件的
名称、版本与描述，用于列表展示、排序与兼容性判断。

### 构造函数

```csharp
public PluginAttribute(string name, string version, string description = "")
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `name` | `string` | 插件名称（显示名）。 |
| `version` | `string` | 插件版本号，建议语义化版本（如 `1.0.0`）。 |
| `description` | `string` | 用途的一句话描述，可为空。 |

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Name` | `string` | 插件名称（显示名）。 |
| `Version` | `string` | 插件版本号。 |
| `Description` | `string` | 插件用途的一句话描述。 |

### 元数据约束

- 目标：仅可用于类（`AttributeTargets.Class`）。
- `Inherited = false`：不随派生类继承。
- `AllowMultiple = false`：一个类只能标注一次。

---

## 4. `ILanguagePack`

语言包契约，用于落实「零硬编码文本」原则：界面上的所有文字都必须来自语言包。

> **M5 变更**：`ILanguagePack : IPlugin`（生命周期方法使用默认实现），实现者只需提供 `GetString`。

### 成员

```csharp
string GetString(string key);
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `key` | `string` | 文本键，例如 `"menu_file"`、`"menu_edit"`。 |
| 返回 | `string` | 对应文本；键不存在时应**原样返回 `key`**。 |

### 约定键（核心内置）

| 键 | 含义 |
|----|------|
| `menu_file` | 菜单：文件 |
| `menu_edit` | 菜单：编辑 |
| `menu_view` | 菜单：视图 |
| `menu_help` | 菜单：帮助 |

### 示例

```csharp
using WFU.PluginSDK;

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

---

## 5. `IThemeProvider`

主题契约，向核心提供编辑器与界面的配色和字体。

> **M5 变更**：`IThemeProvider : IPlugin`（生命周期方法使用默认实现）；新增 `EditorBackground` /
> `EditorForeground` 两个**带默认实现**的属性（默认复用 `BackgroundColor` / `ForegroundColor`）。

### 成员

| 成员 | 类型 | 说明 |
|------|------|------|
| `BackgroundColor` | `string` | 背景色，十六进制，如 `"#1E1E1E"`。 |
| `ForegroundColor` | `string` | 前景（文字）色，十六进制，如 `"#D4D4D4"`。 |
| `FontFamily` | `string` | 字体名称，如 `"Consolas"`。 |
| `FontSize` | `double` | 字号（磅）。 |
| `EditorBackground` | `string` | 编辑器背景色（默认复用 `BackgroundColor`）。 |
| `EditorForeground` | `string` | 编辑器前景色（默认复用 `ForegroundColor`）。 |

### 示例

```csharp
using WFU.PluginSDK;

public class DarkTheme : IThemeProvider
{
    public string BackgroundColor => "#1E1E1E";
    public string ForegroundColor => "#D4D4D4";
    public string FontFamily => "Consolas";
    public double FontSize => 14.0;
}
```

---

## 6. 插件如何被发现与加载

1. 核心在启动时扫描 `WFU_HOME/Plugins/` 目录下的 `*.dll`。
2. 对每个 DLL 执行 `Assembly.Load`，并用反射枚举其中的公开类型。
3. 挑选「实现了 `IPlugin`、非抽象、且有公共无参构造函数」的类型。
4. 通过 `PluginAttribute` 读取名称 / 版本 / 描述。
5. 实例化并调用 `Initialize()`；在用户触发时调用 `Execute()`；卸载时调用 `Dispose()`。

### 打包与部署

```bash
dotnet build -c Release
# 将生成的 .dll 复制到 WFU_HOME/Plugins/
```

---

## 7. 变更记录

| 日期 | 变更 |
|------|------|
| 2026-09-10 | 首次定义 `IPlugin`、`PluginAttribute`、`ILanguagePack`、`IThemeProvider`。 |

---

*本文件随契约演进持续更新。修改契约时请同步更新本文件与 `CHANGELOG.md`。*
