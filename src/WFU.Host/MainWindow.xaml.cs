using System;
using System.Collections.Generic;
using System.IO;
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
