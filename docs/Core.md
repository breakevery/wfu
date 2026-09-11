# WFU.Core — 核心模块文档（M1 Bare Core）

> 本文档详细记载 `WFU.Core` 在 M1（Bare Core）阶段实现的核心模块、契约、行为与设计取舍。

- **程序集**：`WFU.Core.dll`
- **目标框架**：`net8.0`
- **依赖**：`WFU.PluginSDK`
- **命名空间**：`WFU.Core`、`WFU.Core.Services`

---

## 1. 定位

`WFU.Core` 是 WFU 的**原生微内核**，遵循「核心是规则，插件是选择」。核心只做四件事：
**读写、编辑、调试、设置**；其余一切（语言包、主题、工具栏、文件树等）皆为插件。

M1 阶段落地其中的三块：

| 模块 | 文件 | 职责 |
|------|------|------|
| 文件读写 | `Services/IFileService.cs`、`Services/FileService.cs` | 异步读写工程文件，自动建目录 |
| 设置存储 | `Services/ISettingsStore.cs`、`Services/SettingsStore.cs` | 基于 `System.Text.Json` 持久化 `settings.json` |
| 插件加载 | `PluginLoader.cs` | 扫描目录、`Assembly.LoadFrom` 加载 `IPlugin` 实现 |

> 「编辑（EditorHost / AvalonEdit）」与「调试（DebugPipe / WebSocket）」属于后续里程碑（M3 等）。

---

## 2. `IFileService` / `FileService`

文件读写契约与其默认实现，基于 `System.IO.File` 的异步 API，**不做任何解析**。

### 成员

| 成员 | 签名 | 说明 |
|------|------|------|
| `ReadFileAsync` | `Task<string> ReadFileAsync(string path)` | 异步读取全文；文件不存在抛 `FileNotFoundException` |
| `WriteFileAsync` | `Task WriteFileAsync(string path, string content)` | 异步写入；目录不存在时用 `Directory.CreateDirectory` 自动创建 |
| `FileExists` | `bool FileExists(string path)` | 判断文件是否存在 |
| `DirectoryExists` | `bool DirectoryExists(string path)` | 判断目录是否存在（M6-1a 新增） |
| `CreateDirectory` | `void CreateDirectory(string path)` | 创建目录（已存在时静默返回，M6-1a 新增） |
| `EnumerateFiles` | `IEnumerable<string> EnumerateFiles(string path, string searchPattern)` | 非递归枚举目录下文件；目录不存在返回空序列（M6-1a 新增） |

### 行为约定

- `path` 为 `null` 时抛 `ArgumentNullException`。
- `content` 为 `null` 时按空字符串写入。
- 全程使用 `async/await` + `ConfigureAwait(false)`，不使用 `.Result` / `.Wait()`，避免死锁。

### 示例

```csharp
using WFU.Core.Services;

IFileService files = new FileService();
await files.WriteFileAsync(@"C:\proj\index.html", "<h1>Hello</h1>");
var html = await files.ReadFileAsync(@"C:\proj\index.html");
```

---

## 3. `ISettingsStore` / `SettingsStore`

设置存储，以键值对形式保存字体、主题、最近项目等，并持久化到本地 `settings.json`。

### 成员

| 成员 | 签名 | 说明 |
|------|------|------|
| `Get<T>` | `T Get<T>(string key, T defaultValue)` | 读取设置；键不存在或转换失败时返回默认值 |
| `Set<T>` | `void Set<T>(string key, T value)` | 写内存（需 `Save()` 才落盘） |
| `Save` | `void Save()` | 序列化并写入 `settings.json` |
| `Load` | `void Load()` | 从磁盘加载（文件不存在时静默跳过） |

### 实现要点

- 构造函数接收 `string settingsFilePath`。
- 内部使用 `Dictionary<string, object>`（键名不区分大小写）存储。
- `Save()`：目录不存在时自动创建；使用 `JsonSerializerOptions { WriteIndented = true }` 输出可读 JSON。
- `Load()`：反序列化为 `Dictionary<string, JsonElement>` 后缓存；`Get<T>()` 会自动把 `JsonElement` 反序列化为请求类型。
- 属性 `SettingsFilePath` 暴露当前设置文件路径。

### 示例

```csharp
using WFU.Core.Services;

var settings = new SettingsStore(Path.Combine(AppContext.BaseDirectory, "settings.json"));
settings.Load();
var fontSize = settings.Get("fontSize", 14.0);
settings.Set("theme", "Dark");
settings.Save();
```

---

## 4. `PluginLoader`

插件动态加载器。

### 成员

| 成员 | 签名 | 说明 |
|------|------|------|
| `LoadPlugins` | `void LoadPlugins(string directory)` | 扫描目录下 `*.dll`，加载并初始化插件 |
| `UnloadAll` | `void UnloadAll()` | 逆序调用所有插件的 `Dispose()` 并清空列表 |
| `Plugins` | `IReadOnlyList<IPlugin> Plugins` | 当前已加载的插件实例 |

### 加载流程

```
扫描目录 *.dll
  → Assembly.LoadFrom(dll)
  → 反射枚举类型
  → 筛选：实现 IPlugin、非接口、非抽象
  → Activator.CreateInstance(type)
  → plugin.Initialize()
  → 加入 Plugins
```

### 容错策略

- **每个 DLL、每个类型**单独 `try/catch`，异常通过 `Console.WriteLine` 输出，**单个插件失败不会中断整体加载**。
- `GetTypes()` 采用容错版本：遇到 `ReflectionTypeLoadException` 时跳过无法加载的类型。
- `UnloadAll()` 同样逐个容错，确保一个插件的 `Dispose()` 抛错不影响其它插件。

---

## 5. 依赖关系

```
WFU.PluginSDK  ←  WFU.Core  ←  WFU.Host
   (契约)          (微内核)      (WPF 主程序)
```

`WFU.Core.csproj` 通过 `ProjectReference` 引用 `WFU.PluginSDK`，从而识别 `IPlugin` 契约。

---

## 6. 编码规范

- 所有 `public` 成员均带 XML 文档注释。
- 命名空间：`WFU.Core` / `WFU.Core.Services`。
- 使用 `async/await`，禁止 `.Result` / `.Wait()`。
- 禁止使用 `System.Windows.Forms`（核心保持 UI 无关）。

---

## 7. 变更记录

| 日期 | 变更 |
|------|------|
| 2026-09-10 | 实现 `IFileService` / `FileService`、`ISettingsStore` / `SettingsStore`、`PluginLoader`（M1）。 |

---

*本文件随核心实现演进持续更新。修改核心模块时请同步更新本文件与 `CHANGELOG.md`。*
