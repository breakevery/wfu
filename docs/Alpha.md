# WFU Alpha 验收报告（v0.1.0-alpha）

> 本文档归档 WFU 首次端到端 Alpha 验收的结果、发现的问题与已知限制。
> 版本号 **v0.1.0-alpha** 为**内部版本，不对外发布**。

- **验收日期**：2026-09-10
- **代码基线**：`d92e7ad`（M1 Core + M2 UI + M3 WebView2 Bridge）
- **环境**：Windows / .NET 8.0.425 / WebView2 Runtime 152.0.4191.66
- **结论**：**8/8 通过**（功能链路全线打通，但内部限制见第 3 节）

---

## 1. 验收结果摘要（8/8 PASS）

| # | 验收项 | 结果 | 关键证据 |
|---|--------|------|----------|
| 1 | 冷启动 | ✅ PASS | 控制台 `[WebViewHost] WebView2 初始化完成。`；标题 `WFU — WebView For Unity`；右侧加载 test-bridge.html |
| 2 | 文件打开 | ✅ PASS | 状态栏 `已打开: ...alpha-test.html`；编辑器内容与文件一致 |
| 3 | 编辑 | ✅ PASS | 状态栏出现 `● 已修改`（IsModified=true） |
| 4 | 保存 | ✅ PASS | 磁盘内容变为 `<h1>ALPHA_EDITED_MARKER</h1>`；状态栏回到 `已保存` |
| 5 | 预览刷新 | ✅ PASS | WebView2 文档 = `ALPHA_EDITED_MARKER`（渲染后内容，非缓存）；状态栏 `预览已刷新` |
| 6 | 桥接回归 | ✅ PASS | `[JS→C#] Hello from JS!`、时间戳返回值、`bridge-test.txt` 生成 |
| 7 | 菜单回归 | ✅ PASS | New 清空编辑器且还原 `新建文件 / 已保存`；Exit 菜单项存在且 `enabled=True` |
| 8 | 异常捕获 | ✅ PASS | 注入 `alpha-test` 异常后弹出「WFU · 未处理异常」，主窗口仍在（未闪退） |

---

## 2. 已修复的隐藏 Bug

### 2.1 AvalonEdit 文本双向绑定缺陷（M2 遗留）

**现象**：编辑过文字后，点击 **New / Open 无法刷新编辑器**（状态栏已复位，但编辑器文本不变）。

**根因**：M2 的 `Behaviors/EditorTextBinding.cs` 采用**双向绑定 + 回写**。回写在附加依赖属性上写入“本地值”，
**覆盖掉了绑定本身**，导致此后「视图模型 → 编辑器」的推送全部失效。

**修复方案**：附加属性改为**仅负责「视图模型 → 编辑器」的单向桥接**；
反向的「编辑器 → 视图模型」改由 `MainWindow` 的 `TextChanged` 事件转发（带一致性判断，避免误标已修改）。
只有保持绑定不被本地写入覆盖，`New` / `Open` 才能可靠刷新编辑器。

> 该问题是本次 Alpha 验收才暴露的隐藏缺陷（M2 当时“看似通过”）。

### 2.2 为满足验收补齐的功能

| 功能 | 说明 |
|------|------|
| Preview 预览刷新 | 工具栏 `Preview` 原为占位，现实现为把 `EditorContent` 渲染到 WebView2 |
| IsModified 视觉提示 | 状态栏新增 `已保存 / ● 已修改` 指示器 |

---

## 3. 已知限制清单

1. 无语言切换（界面文本硬编码，`ILanguagePack` 尚未接入）。
2. 无主题切换（固定深色，`IThemeProvider` 尚未接入）。
3. 无一键导出 .zip（Toolbar 的 `Export` 为占位）。
4. 无插件加载（`PluginLoader` 已就绪，但尚无插件 DLL 产出）。
5. 预览需手动点 `Preview`；且**预览会覆盖桥接测试页**，无法在应用内返回该页（需重启）。
6. 左侧文件树、View 面板切换、Edit（撤销/重做/剪贴板）均为占位。

---

## 4. 版本号

- **v0.1.0-alpha**（内部版本，不对外发布，不打包、不打 tag）。

---

## 5. 下一步计划

- **M4 插件装配**：落地 `plugins/EnglishPack`（`ILanguagePack`）与 `plugins/BasicTheme`（`IThemeProvider`），
  由 `WFU.Host` 启动时从 `Plugins/` 加载并应用；**加载失败时优雅降级**（回退硬编码文本 / 默认主题，不崩溃）。
- 后续：插件目录扫描配置、导出功能、文件树与编辑命令实装。

---

*本文件随验收进展持续更新。*
