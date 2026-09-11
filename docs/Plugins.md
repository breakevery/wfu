# WFU 插件系统文档（M4）

> 本文档详细记载 WFU 插件系统的架构、加载机制、开发方式、降级策略与已知问题。

- **契约程序集**：`WFU.PluginSDK`（`net8.0`）
- **加载器**：`WFU.Core.PluginLoader`
- **插件目录**：`<程序目录>/Plugins/`
- **官方插件**：`plugins/EnglishPack`、`plugins/BasicTheme`

---

## 1. 插件系统概述

WFU 采用**微内核（microkernel）**架构：

- `WFU.Core` 只提供 4 项基础能力：**读写、编辑、调试、设置**，并额外提供 `PluginLoader` 用于动态加载。
- 其余一切（语言包、主题、文件树、工具栏……）都由**插件**实现。
- 插件实现 `WFU.PluginSDK` 定义的接口，内核在启动时动态发现并加载它们。

当前支持的插件能力接口：

| 接口 | 用途 |
|------|------|
| `ILanguagePack` | 提供界面文本（`GetString(key)`），支撑「零硬编码文本」 |
| `IThemeProvider` | 提供配色与字体（背景色/前景色/字体/字号） |

> 内核通过 `IPlugin` 识别「这是一个插件」，能力接口则声明它「能做什么」。

---

## 2. 插件加载机制

### 扫描与加载

- **扫描目录**：`Path.Combine(AppContext.BaseDirectory, "Plugins")`，即程序输出目录下的 `Plugins/`。
- **扫描策略**：**根目录 + 一层子目录**（因为插件 DLL 按规范位于 `Plugins/<插件名>/`，而
  `WFU.Core.PluginLoader` 只做**单层**扫描，因此 `WFU.Host` 在「根目录」与「每个一级子目录」上分别调用 `LoadPlugins`）。
- **加载方式**：对每个 `*.dll` 执行 `Assembly.LoadFrom`，反射枚举类型。
- **识别条件**：类型**实现了 `IPlugin`**、非接口、非抽象、具有公共无参构造函数。
- **初始化**：`Activator.CreateInstance(type)` 后调用 `plugin.Initialize()`，并加入 `PluginLoader.Plugins`。

### 容错

- 每个 DLL、每个类型**单独 try/catch**：单个插件加载失败**不会中断**整体加载。
- `GetTypes()` 采用容错版本（遇到 `ReflectionTypeLoadException` 时跳过无法加载的类型）。
- 错误信息通过 `Console.WriteLine` 输出（形如 `[PluginLoader] 加载失败: ...`）。

### 宿主侧接线（`WFU.Host`）

```
MainWindow_Loaded
  → LoadPlugins()        // 扫描根目录 + 一级子目录，取 CurrentLanguagePack / CurrentTheme
  → ApplyTheme()         // 应用主题（方案 A：复用 BackgroundColor）
  → ApplyLanguage()      // 方案 B：代码后置把语言包文本写入菜单/工具栏
  → RestoreWindowBounds()// 从 settings.json 恢复窗口位置/大小
```

---

## 3. 如何编写新插件

### 3.1 最小模板

**目录结构**

```
plugins/
└── MyPlugin/
    ├── MyPlugin.csproj
    └── MyPlugin.cs
```

**`MyPlugin.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <!-- 输出到宿主的 Plugins/<插件名>/ -->
    <OutputPath>..\..\src\WFU.Host\bin\$(Configuration)\net8.0-windows\Plugins\MyPlugin\</OutputPath>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <CopyLocalLockFileAssemblies>false</CopyLocalLockFileAssemblies>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\WFU.PluginSDK\WFU.PluginSDK.csproj">
      <Private>false</Private>
    </ProjectReference>
  </ItemGroup>
</Project>
```

**`MyPlugin.cs`**

```csharp
using WFU.PluginSDK;

namespace MyPlugin;

[Plugin("MyPlugin", "1.0.0", "My plugin description")]
public class MyPlugin : IPlugin
{
    public void Initialize() { /* 加载时 */ }
    public void Execute()    { /* 触发时 */ }
    public void Dispose()    { /* 卸载时 */ }
}
```

### 3.2 关键注意点

1. **必须实现 `IPlugin`**（即使 3 个方法留空）——否则 `PluginLoader` 会把它过滤掉（表现为「已加载 0 个插件」）。
2. **必须用 `[Plugin(...)]` 标注**元数据（名称 / 版本 / 描述）。
3. 引用 `WFU.PluginSDK` 时用 **`<Private>false</Private>`** + **`CopyLocalLockFileAssemblies=false`**，
   **避免 `WFU.PluginSDK.dll` 被复制进插件目录**（否则可能导致类型加载冲突）。
4. `OutputPath` 必须指向 `Plugins/<你的插件名>/`。
5. 一个插件 DLL 里**可以同时实现多个能力接口**（例如既做语言包又做主题）。

### 3.3 完整示例：EnglishPack

```csharp
using WFU.PluginSDK;

namespace EnglishPack;

[Plugin("EnglishPack", "1.0.0", "English language pack")]
public class EnglishLanguagePack : IPlugin, ILanguagePack
{
    private static readonly Dictionary<string, string> Strings = new()
    {
        { "menu_file", "File" },
        // ...其余 key 省略
    };

    public void Initialize() { }
    public void Execute() { }
    public void Dispose() { }

    public string GetString(string key)
        => Strings.TryGetValue(key, out var value) ? value : key;
}
```

---

## 4. 当前官方插件清单

| 插件 | 版本 | 能力 | 说明 |
|------|------|------|------|
| **EnglishPack** | 1.0.0 | `ILanguagePack` | 英文语言包，覆盖菜单/工具栏/状态栏/项目/导入导出共 **52** 条 key |
| **BasicTheme** | 1.0.0 | `IThemeProvider` | 深色主题（背景 `#1E1E1E`、前景 `#D4D4D4`、Consolas 14） |

两者均位于仓库根 `plugins/` 下，已加入解决方案但不参与主程序编译（各自输出到宿主 `Plugins/` 子目录）。

---

## 5. 降级策略

**降级不崩溃是硬性要求。**

| 场景 | 行为 |
|------|------|
| `Plugins/` 目录不存在 | 记录日志，插件数 = 0，UI 回退到**硬编码文本 / 默认样式**，程序正常运行 |
| 无语言包 | `ApplyLanguage()` 直接返回，菜单/工具栏保持原硬编码文本 |
| 无主题 | `ApplyTheme()` 直接返回，使用默认样式 |
| 单个插件加载失败 | 该插件被跳过，**不影响**其他插件；异常写入控制台 |
| `settings.json` 缺失 | 窗口使用默认位置/大小 |

宿主侧所有插件相关调用都包在 try/catch 中，任何异常都会降级并写日志，**不会导致程序退出**。

---

## 6. 已知限制与技术债

> **M5 进展**：① 接口继承重构（`ILanguagePack` / `IThemeProvider` 继承 `IPlugin`，生命周期方法提供默认实现）——✅ M5-1 已完成；
> ② 补全 EnglishPack 8 个缺失 key——✅ M5-2 已完成；③ 状态栏接入语言包——✅ M5-3 已完成；④ 发布自动复制 Plugins——✅ M5-4 已完成。

1. 界面文本替换采用**方案 B（代码后置硬替换）+ ViewModel 注入**，而非 XAML 绑定 Converter。
2. `IThemeProvider` 的 `EditorBackground` / `EditorForeground` 已进入接口（带默认实现），M5-1 已偿还该项技术债。
3. 历史遗留的「接口未继承 `IPlugin`」问题已由 M5-1 解决（插件类不再需要重复实现生命周期方法）。
4. 状态栏的动态文本（状态消息、已保存/已修改、行列标签）已由语言包驱动（M5-3）。

> **M5 剩余计划**：无（4 项子任务全部完成）。

---

## 7. 故障排查

| 现象 | 排查方向 |
|------|----------|
| **「已加载 0 个插件」** | ① DLL 是否确实在 `Plugins/` 或其一级子目录下；② 插件类是否实现了 `IPlugin`；③ 是否有 `[Plugin]` 标注；④ 类型是否有公共无参构造函数 |
| **`FileLoadException` / `FileNotFoundException`** | 检查 `WFU.PluginSDK.dll` 是否被意外复制到插件目录（应使用 `<Private>false</Private>`）；检查插件目标框架与宿主兼容性 |
| **语言包不生效** | ① `CurrentLanguagePack` 是否为 null（看控制台 `[PluginLoader] 语言包:` 行）；② 菜单项的 `x:Name` 是否已加；③ key 是否与插件中定义**完全一致** |
| **主题不生效** | 查看控制台是否输出 `[Theme] 已应用: ...`；检查控件 `x:Name`（编辑器为 `Editor`） |
| **窗口位置异常** | 直接删除 `settings.json` 恢复默认；代码含虚拟屏边界校验，越界会自动回退 |

---

*本文件随插件系统演进持续更新。修改插件契约或加载逻辑时，请同步更新本文件与 `CHANGELOG.md`。*
