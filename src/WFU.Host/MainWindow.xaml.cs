using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WFU.Core;
using WFU.Core.Services;
using WFU.Host.ViewModels;

namespace WFU.Host;

/// <summary>
/// Interaction logic for MainWindow.xaml。
/// 本文件只做界面事件转发，业务逻辑位于 <see cref="MainViewModel"/>。
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    /// <summary>初始化主窗口：装配视图模型并接线编辑器事件。</summary>
    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel(
            new FileService(),
            new SettingsStore(Path.Combine(AppContext.BaseDirectory, "settings.json")));
        DataContext = _viewModel;

        // 光标位置变化 -> 转发给视图模型更新行列号
        Editor.TextArea.Caret.PositionChanged += OnCaretPositionChanged;

        // ==== 临时验证：M1 核心模块回归测试 ====
        // 默认不调用；需要回归时取消下面一行的注释。
        // RunCoreModuleTests();
    }

    /// <summary>窗口加载完成：初始化 WebView2 并加载桥接测试页。</summary>
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await RightWebView.InitializeAsync();
            RightWebView.LoadHtml(LoadEmbeddedResource("test-bridge.html"));
            _viewModel.StatusText = "WebView2 已就绪";
            Console.WriteLine("[MainWindow] WebView2 初始化完成，测试页已加载。");
        }
        catch (Exception ex)
        {
            _viewModel.StatusText = $"WebView2 初始化失败: {ex.Message}";
            Console.WriteLine($"[MainWindow] WebView2 初始化失败: {ex}");
            MessageBox.Show(ex.Message, "WebView2 初始化失败", MessageBoxButton.OK, MessageBoxImage.Error);
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

    /// <summary>编辑器文本变化的事件转发（XAML 中注册）。</summary>
    private void Editor_TextChanged(object sender, EventArgs e)
    {
        _viewModel.NotifyEditorTextChanged();
    }

    /// <summary>光标位置变化的事件转发。</summary>
    private void OnCaretPositionChanged(object? sender, EventArgs e)
    {
        _viewModel.NotifyCaretPositionChanged(
            Editor.TextArea.Caret.Line,
            Editor.TextArea.Caret.Column);
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
