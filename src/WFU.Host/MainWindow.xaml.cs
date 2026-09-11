using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WFU.Core;
using WFU.Core.Services;
using WFU.Host.ViewModels;
using WFU.PluginSDK;

namespace WFU.Host;

/// <summary>
/// Interaction logic for MainWindow.xaml。
/// 本文件只做界面事件转发，业务逻辑位于 <see cref="MainViewModel"/>。
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly SettingsStore _settingsStore;

    /// <summary>当前生效的语言包插件；未加载时为 <c>null</c>，界面需回退到硬编码文本。</summary>
    public static ILanguagePack? CurrentLanguagePack { get; private set; }

    /// <summary>当前生效的主题插件；未加载时为 <c>null</c>，界面需回退到默认样式。</summary>
    public static IThemeProvider? CurrentTheme { get; private set; }

    /// <summary>初始化主窗口：装配视图模型并接线编辑器事件。</summary>
    public MainWindow()
    {
        InitializeComponent();

        // 提取为字段，供窗口生命周期（恢复/保存位置）使用
        _settingsStore = new SettingsStore(Path.Combine(AppContext.BaseDirectory, "settings.json"));

        _viewModel = new MainViewModel(new FileService(), _settingsStore);
        DataContext = _viewModel;

        // 光标位置变化 -> 转发给视图模型更新行列号
        Editor.TextArea.Caret.PositionChanged += OnCaretPositionChanged;

        // ==== 临时验证：M1 核心模块回归测试 ====
        // 默认不调用；需要回归时取消下面一行的注释。
        // RunCoreModuleTests();
    }

    /// <summary>窗口加载完成：初始化 WebView2、加载插件并应用主题。</summary>
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 0) 先加载设置（语言包选择需要读 settings 里的 language 字段）
        _settingsStore.Load();

        // 1) 初始化 WebView2 并加载桥接测试页
        try
        {
            await RightWebView.InitializeAsync();
            RightWebView.LoadHtml(LoadEmbeddedResource("test-bridge.html"));
            // 启动就绪状态（立即可见；插件未加载时回退中文，稍后由 ApplyLanguage 本地化刷新）
            _viewModel.StatusText = CurrentLanguagePack?.GetString("status_webview_ready") ?? "WebView2 已就绪";
            Console.WriteLine("[MainWindow] WebView2 初始化完成，测试页已加载。");
        }
        catch (Exception ex)
        {
            _viewModel.StatusText = $"WebView2 初始化失败: {ex.Message}";
            Console.WriteLine($"[MainWindow] WebView2 初始化失败: {ex}");
            MessageBox.Show(ex.Message, "WebView2 初始化失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // 2) 加载插件（含降级保护）、应用主题与语言包
        LoadPlugins();
        ApplyTheme();
        ApplyLanguage();
        RestoreWindowBounds();
    }

    /// <summary>
    /// 扫描 <c>Plugins/</c> 目录并加载所有插件。任何异常都会被捕获并降级，
    /// 不会导致程序崩溃。
    /// </summary>
    private void LoadPlugins()
    {
        try
        {
            var pluginDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
            Console.WriteLine($"[PluginLoader] 扫描目录: {pluginDir}");

            var loader = new PluginLoader();

            // 注意：WFU.Core.PluginLoader 只扫描“单层目录”，而插件 DLL 按规范位于 Plugins\<插件名>\ 子目录下。
            // 内核冻结（不修改 WFU.Core），因此这里在 Host 侧对根目录与各一层子目录分别扫描。
            if (Directory.Exists(pluginDir))
            {
                loader.LoadPlugins(pluginDir);

                foreach (var subDir in Directory.GetDirectories(pluginDir))
                {
                    if (Directory.GetFiles(subDir, "*.dll").Length > 0)
                        loader.LoadPlugins(subDir);
                }
            }
            else
            {
                Console.WriteLine($"[PluginLoader] 插件目录不存在: {pluginDir}");
            }

            // 按 settings 里的 language 字段匹配插件 Name
            var preferredLanguage = _settingsStore.Get<string>("language", "ChinesePack");
            CurrentLanguagePack = loader.Plugins
                .OfType<ILanguagePack>()
                .FirstOrDefault(p => GetPluginName(p) == preferredLanguage);

            if (CurrentLanguagePack != null)
            {
                Console.WriteLine($"[Lang] 按 settings 选择语言包: {preferredLanguage}");
            }
            else
            {
                // 回退：取第一个可用语言包
                CurrentLanguagePack = loader.Plugins.OfType<ILanguagePack>().FirstOrDefault();
                if (CurrentLanguagePack != null)
                {
                    Console.WriteLine($"[Lang] 未找到 {preferredLanguage} 语言包，回退到 {GetPluginName(CurrentLanguagePack)}");
                }
                else
                {
                    Console.WriteLine("[Lang] 无任何语言包可用");
                }
            }
            CurrentTheme = loader.Plugins.OfType<IThemeProvider>().FirstOrDefault();

            // 注入语言包到 ViewModel（供状态栏等动态文本本地化）
            _viewModel.SetLanguagePack(CurrentLanguagePack);

            Console.WriteLine($"[PluginLoader] 已加载 {loader.Plugins.Count} 个插件");
            Console.WriteLine($"[PluginLoader] 语言包: {CurrentLanguagePack?.GetType().Name ?? "未加载（降级到硬编码）"}");
            Console.WriteLine($"[PluginLoader] 主题: {CurrentTheme?.GetType().Name ?? "未加载（降级到默认样式）"}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PluginLoader] 加载异常，已降级: {ex.Message}");
            Console.WriteLine($"[PluginLoader] 堆栈: {ex.StackTrace}");
            // 不抛出，让程序继续以硬编码模式运行
        }
    }

    /// <summary>应用主题插件提供的颜色与字体；未加载或失败时保持默认样式。</summary>
    private void ApplyTheme()
    {
        if (CurrentTheme == null)
        {
            Console.WriteLine("[Theme] 无主题插件，使用默认样式");
            return;
        }

        try
        {
            // 方案 A：编辑器背景复用 BackgroundColor（IThemeProvider 仅 4 个属性）
            var bg = (Color)ColorConverter.ConvertFromString(CurrentTheme.BackgroundColor);
            var fg = (Color)ColorConverter.ConvertFromString(CurrentTheme.ForegroundColor);

            Background = new SolidColorBrush(bg);
            Editor.Background = new SolidColorBrush(bg);
            Editor.Foreground = new SolidColorBrush(fg);
            Editor.FontFamily = new FontFamily(CurrentTheme.FontFamily);
            Editor.FontSize = CurrentTheme.FontSize;

            Console.WriteLine($"[Theme] 已应用: bg={CurrentTheme.BackgroundColor}, font={CurrentTheme.FontFamily} {CurrentTheme.FontSize}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Theme] 应用主题失败，保留默认样式: {ex.Message}");
        }
    }

    /// <summary>
    /// 应用语言包文本到菜单与工具栏（方案 B：代码后置硬替换）。
    /// 无语言包时保持原有硬编码文本（优雅降级）。
    /// </summary>
    private void ApplyLanguage()
    {
        if (CurrentLanguagePack == null)
        {
            Console.WriteLine("[Lang] 无语言包，保持原硬编码文本");
            return;
        }

        try
        {
            var t = CurrentLanguagePack;

            // 菜单栏
            MenuFile.Header = t.GetString("menu_file");
            MenuFileNew.Header = t.GetString("menu_file_new");
            MenuFileOpen.Header = t.GetString("menu_file_open");
            MenuFileSave.Header = t.GetString("menu_file_save");
            MenuFileExit.Header = t.GetString("menu_file_exit");
            MenuEdit.Header = t.GetString("menu_edit");
            MenuView.Header = t.GetString("menu_view");
            MenuViewTestBridge.Header = t.GetString("menu_view_test_bridge");
            MenuHelp.Header = t.GetString("menu_help");
            MenuHelpAbout.Header = t.GetString("menu_help_about");

            // 工具栏
            ToolbarNew.Content = t.GetString("toolbar_new");
            ToolbarOpen.Content = t.GetString("toolbar_open");
            ToolbarSave.Content = t.GetString("toolbar_save");
            ToolbarPreview.Content = t.GetString("toolbar_preview");
            ToolbarExport.Content = t.GetString("toolbar_export");
            ToolbarBridgeTest.Content = t.GetString("toolbar_bridge_test");

            // Edit 菜单项（M5 补全）
            MenuEditUndo.Header = t.GetString("menu_edit_undo");
            MenuEditRedo.Header = t.GetString("menu_edit_redo");
            MenuEditCut.Header = t.GetString("menu_edit_cut");
            MenuEditCopy.Header = t.GetString("menu_edit_copy");
            MenuEditPaste.Header = t.GetString("menu_edit_paste");

            // View 菜单项（M5 补全）
            MenuViewToggleLeft.Header = t.GetString("menu_view_toggle_left");
            MenuViewToggleRight.Header = t.GetString("menu_view_toggle_right");

            // 状态栏静态标签（M5-3）
            StatusBarLineLabel.Text = t.GetString("status_line") + ": ";
            StatusBarColLabel.Text = t.GetString("status_col") + ": ";

            // 启动就绪状态（此时语言包已就绪，覆盖启动时的中文占位）
            _viewModel.StatusText = t.GetString("status_webview_ready");

            // 项目系统（M6-1b-2）
            MenuFileNewProject.Header = t.GetString("menu_file_new_project");
            ToolbarNewProject.Content = t.GetString("toolbar_new_project");

            // Export（M6-2）
            MenuFileExport.Header = t.GetString("menu_file_export");

            // Import（M6-3）
            MenuFileImport.Header = t.GetString("menu_file_import");
            ToolbarImport.Content = t.GetString("toolbar_import");

            Console.WriteLine("[Lang] 已应用语言包文本");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Lang] 应用失败，保留原硬编码: {ex.Message}");
        }
    }

    /// <summary>从插件类型上读 <c>[Plugin]</c> 特性的 Name（找不到时回退为类型名）。</summary>
    /// <param name="plugin">插件实例。</param>
    /// <returns>插件 Name。</returns>
    private static string GetPluginName(WFU.PluginSDK.IPlugin plugin)
    {
        var attr = plugin.GetType()
            .GetCustomAttributes(typeof(WFU.PluginSDK.PluginAttribute), false)
            .FirstOrDefault() as WFU.PluginSDK.PluginAttribute;
        return attr?.Name ?? plugin.GetType().Name;
    }

    /// <summary>从 settings.json 恢复窗口位置与大小（带虚拟屏边界校验）。</summary>
    private void RestoreWindowBounds()
    {
        try
        {
            var left = _settingsStore.Get<double>("window.left", double.NaN);
            var top = _settingsStore.Get<double>("window.top", double.NaN);
            var width = _settingsStore.Get<double>("window.width", 1200);
            var height = _settingsStore.Get<double>("window.height", 800);

            if (!double.IsNaN(left) && !double.IsNaN(top))
            {
                var vLeft = SystemParameters.VirtualScreenLeft;
                var vTop = SystemParameters.VirtualScreenTop;
                var vRight = vLeft + SystemParameters.VirtualScreenWidth;
                var vBottom = vTop + SystemParameters.VirtualScreenHeight;

                if (left >= vLeft - 50 && left <= vRight - 100
                    && top >= vTop - 50 && top <= vBottom - 100)
                {
                    WindowStartupLocation = WindowStartupLocation.Manual;
                    Left = left;
                    Top = top;
                }
                else
                {
                    Console.WriteLine("[Settings] 保存的位置超出屏幕，回退默认");
                }
            }

            if (width > 400 && height > 300)
            {
                Width = width;
                Height = height;
            }

            Console.WriteLine($"[Settings] 已恢复窗口: {Left},{Top} {Width}x{Height}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Settings] 恢复窗口失败，使用默认: {ex.Message}");
        }
    }

    /// <summary>关闭时保存窗口位置与大小到 settings.json。</summary>
    private void OnWindowClosing(object sender, CancelEventArgs e)
    {
        try
        {
            var bounds = WindowState == WindowState.Normal
                ? new Rect(Left, Top, Width, Height)
                : RestoreBounds;

            _settingsStore.Set("window.left", bounds.Left);
            _settingsStore.Set("window.top", bounds.Top);
            _settingsStore.Set("window.width", bounds.Width);
            _settingsStore.Set("window.height", bounds.Height);
            _settingsStore.Save();

            Console.WriteLine($"[Settings] 已保存窗口位置: {bounds}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Settings] 保存窗口失败: {ex.Message}");
        }
    }

    /// <summary>预览刷新：把编辑器当前内容渲染到右侧 WebView2。</summary>
    private void Preview_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var html = _viewModel.EditorContent;
            if (string.IsNullOrWhiteSpace(html))
            {
                _viewModel.StatusText = "预览：内容为空";
                return;
            }

            RightWebView.LoadHtml(html);
            _viewModel.StatusText = "预览已刷新";
            Console.WriteLine($"[PREVIEW] 已刷新预览，长度 {html.Length}。");
        }
        catch (Exception ex)
        {
            _viewModel.StatusText = $"预览失败: {ex.Message}";
            Console.WriteLine($"[PREVIEW] 失败: {ex.Message}");
        }
    }

    /// <summary>C# → JS 反向通信演示：修改预览页 log 区域文本。</summary>
    private async void TestCSharpToJs_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await RightWebView.ExecuteScriptAsync(
                "document.getElementById('log').innerText = 'C# 主动调用 JS 成功';");
            Console.WriteLine("[BRIDGE TEST] C#→JS 调用完成。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BRIDGE TEST] C#→JS 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 桥接自检：依次验证 C# → JS、JS → C#（void / 返回值 / 异步），结果输出到控制台。
    /// </summary>
    private async void BridgeTest_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Console.WriteLine("[BRIDGE TEST] ===== 开始 =====");

            // 1) C# → JS
            await RightWebView.ExecuteScriptAsync(
                "document.getElementById('log').innerText = 'C# 主动调用 JS 成功';");
            await Task.Delay(300);
            var log1 = await RightWebView.ExecuteScriptAsync("document.getElementById('log').innerText");
            Console.WriteLine($"[BRIDGE TEST] C#→JS 后 log = {log1}");

            // 2) JS → C# 同步 void
            await RightWebView.ExecuteScriptAsync("testShowMessage()");
            await Task.Delay(500);

            // 3) JS → C# 带返回值
            await RightWebView.ExecuteScriptAsync("testGetTimestamp()");
            await Task.Delay(500);
            var log2 = await RightWebView.ExecuteScriptAsync("document.getElementById('log').innerText");
            Console.WriteLine($"[BRIDGE TEST] 时间戳 log = {log2}");

            // 4) JS → C# 异步
            await RightWebView.ExecuteScriptAsync("testSaveFile()");
            await Task.Delay(800);
            var log3 = await RightWebView.ExecuteScriptAsync("document.getElementById('log').innerText");
            Console.WriteLine($"[BRIDGE TEST] 保存 log = {log3}");

            Console.WriteLine("[BRIDGE TEST] ===== 完成 =====");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BRIDGE TEST] 失败: {ex}");
        }
    }

    /// <summary>编辑器文本变化的事件转发（XAML 中注册）：把文本同步回视图模型并标记已修改。</summary>
    private void Editor_TextChanged(object sender, EventArgs e)
    {
        // 与视图模型一致时说明是「视图模型 → 编辑器」的同步引起的，忽略，避免误标已修改
        if (Editor.Text == _viewModel.EditorContent)
            return;

        _viewModel.EditorContent = Editor.Text;
        _viewModel.NotifyEditorTextChanged();
    }

    /// <summary>光标位置变化的事件转发。</summary>
    private void OnCaretPositionChanged(object? sender, EventArgs e)
    {
        _viewModel.NotifyCaretPositionChanged(
            Editor.TextArea.Caret.Line,
            Editor.TextArea.Caret.Column);
    }

    /// <summary>文件树双击：打开选中的项目文件（M6-1b-2）。</summary>
    private async void ProjectTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TreeView tree) return;
        if (tree.SelectedItem is not string fileName) return;
        await _viewModel.OpenProjectFileAsync(fileName);
    }

    /// <summary>读取嵌入资源文本（按文件名后缀匹配）。</summary>
    private static string LoadEmbeddedResource(string fileNameSuffix)
    {
        var assembly = typeof(MainWindow).Assembly;
        var resourceName = Array.Find(
            assembly.GetManifestResourceNames(),
            n => n.EndsWith(fileNameSuffix, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"未找到嵌入资源: {fileNameSuffix}");

        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException($"无法读取嵌入资源: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// 临时验证代码：依次测试 <see cref="FileService"/>、<see cref="SettingsStore"/>、<see cref="PluginLoader"/>。
    /// 每项用 try-catch 包裹，结果同时输出到 <see cref="Console"/> 与 <see cref="MessageBox"/>（双通道）。
    /// </summary>
    /// <remarks>
    /// 说明：构造函数内无法使用 async/await，这里对 FileService 的异步方法采用
    /// <c>GetAwaiter().GetResult()</c>（FileService 内部使用 ConfigureAwait(false)，不会死锁）。
    /// </remarks>
    private static void RunCoreModuleTests()
    {
        var results = new List<string>();

        // 1) FileService：写入 -> 读回 -> 删除
        try
        {
            var files = new FileService();
            var testFile = Path.Combine(AppContext.BaseDirectory, "test.txt");

            files.WriteFileAsync(testFile, "Hello WFU").GetAwaiter().GetResult();
            var content = files.ReadFileAsync(testFile).GetAwaiter().GetResult();

            var exists = files.FileExists(testFile);
            if (exists)
                File.Delete(testFile);

            results.Add(content == "Hello WFU" && exists
                ? "[TEST PASS] FileService: 写入/读取/删除均正常"
                : $"[TEST FAIL] FileService: 读回内容不符 -> \"{content}\"，exists={exists}");
        }
        catch (Exception ex)
        {
            results.Add($"[TEST FAIL] FileService: {ex.Message}");
        }

        // 2) SettingsStore：Set -> Save -> 新实例 Load -> Get
        try
        {
            var settingsPath = Path.Combine(AppContext.BaseDirectory, "test-settings.json");

            var store = new SettingsStore(settingsPath);
            store.Set("fontSize", 14);
            store.Save();

            var reloaded = new SettingsStore(settingsPath);
            reloaded.Load();
            var fontSize = reloaded.Get("fontSize", 0);

            results.Add(fontSize == 14
                ? "[TEST PASS] SettingsStore: 读取到 fontSize = 14"
                : $"[TEST FAIL] SettingsStore: 读取到 fontSize = {fontSize}（预期 14）");
        }
        catch (Exception ex)
        {
            results.Add($"[TEST FAIL] SettingsStore: {ex.Message}");
        }

        // 3) PluginLoader：创建 Plugins 目录 -> 加载 -> 统计数量
        try
        {
            var pluginsDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
            Directory.CreateDirectory(pluginsDir);

            var loader = new PluginLoader();
            loader.LoadPlugins(pluginsDir);
            var count = loader.Plugins.Count;

            results.Add("[TEST PASS] PluginLoader: 加载到 " + count + " 个插件"
                + (count == 0 ? "（Plugins 目录为空）" : string.Empty));
        }
        catch (Exception ex)
        {
            results.Add($"[TEST FAIL] PluginLoader: {ex.Message}");
        }

        // 双通道输出
        foreach (var line in results)
            Console.WriteLine(line);

        MessageBox.Show(
            string.Join(Environment.NewLine, results),
            "WFU · M1 核心模块验证",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
