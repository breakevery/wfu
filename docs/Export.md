# WFU Project Export / Import / WFU 项目导出与导入

本文档说明 WFU 的项目导出与导入机制。

## 一、项目概念

WFU 的“项目”是一个**标准目录**，包含若干 HTML / CSS / JS 文件。

一个最小项目包含：

```
MyProject/
├── index.html       # 入口页面
├── app.js           # 业务逻辑
├── style.css        # 样式
└── unity-bridge.js  # Unity WebView 桥接适配层
```

新建项目时，WFU 会自动从 `Templates/` 复制这 4 个文件。

## 二、新建项目

**入口**：`File → New Project...` 或工具栏 `New Project`

**流程**：
1. 弹出保存对话框，选择项目存放位置（保存文件名为 `index.html`）
2. WFU 取该文件所在的目录作为项目根
3. 自动从 `Templates/` 复制 4 个模板文件
4. 自动设为当前项目，左侧文件树显示所有文件
5. 自动打开 `index.html`

## 三、导出为 ZIP

**入口**：`File → Export as ZIP...`（未打开项目时禁用）

**输出结构**（zip 内部）：

```
{项目名}/
├── index.html
├── app.js
├── style.css
└── unity-bridge.js
```

**关键设计**：外层套一层 `{项目名}/` 目录，便于：
- 用户解压后得到独立文件夹
- Unity 项目 `StreamingAssets/MiniApps/{项目名}/` 直接对应
- 避免解压时散落文件

**技术实现**：使用 `System.IO.Compression.ZipArchive` 逐文件添加，`CompressionLevel.Optimal`。

## 四、导入项目

**入口**：`File → Import Project...` 或工具栏 `Import`

**流程**：
1. 选择要导入的 zip 包
2. WFU 解压到 zip 同级同名目录（如 `D:\myexp.zip` → `D:\myexp\`）
3. 自动识别 zip 内的项目根（若 zip 只有一层 `{项目名}/` 目录，自动进入）
4. 设为当前项目，打开 `index.html`

**目录已存在时**：弹窗确认“是否合并并继续”。点“否”则取消操作。

**安全防护**：解压时检查每个条目路径，防止 zip slip 攻击（`../` 逃逸）。

## 五、Unity 侧对接

**建议**：将 WFU 导出的 zip 解压后，整个 `{项目名}/` 目录放进：

```
Assets/StreamingAssets/MiniApps/{项目名}/
```

Unity 侧加载路径（通过 WebView 插件）：

```csharp
var path = Path.Combine(Application.streamingAssetsPath, "MiniApps", "MyProject", "index.html");
// Android: 需特殊处理 file:///android_asset/
// iOS: 需特殊处理 WKWebView 的 loadFileURL
```

## 六、模板文件说明

| 文件 | 作用 |
|------|------|
| `index.html` | 页面骨架，引入 CSS/JS |
| `app.js` | 业务逻辑，演示 `UnityBridge.call()` 用法 |
| `style.css` | 默认深色样式 |
| `unity-bridge.js` | 桥接适配层：preview 模式提供 mock，Unity 宿主覆盖真实实现 |

**自定义模板**：编辑 `src/WFU.Host/Templates/` 下的文件即可。发布时自动带上。

## 七、相关技术细节

- **Templates 目录**：`src/WFU.Host/Templates/`，通过 csproj `<Content>` 复制到输出和 publish
- **IFileService 目录 API**：M6-1a 扩展了 `DirectoryExists` / `CreateDirectory` / `EnumerateFiles`
- **CanExecute**：Export 命令绑定 `HasProject`，未打开项目时按钮禁用

## 八、已知限制

- 暂不支持导入包含子目录的 zip（仅处理扁平结构 + 一层 `{项目名}/`）
- 暂不支持导出时排除特定文件（如 `.git`、`node_modules`）
- 暂不支持导出为单 HTML 文件（多文件结构是当前设计）
