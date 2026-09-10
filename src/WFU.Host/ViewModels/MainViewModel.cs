using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WFU.Core.Services;

namespace WFU.Host.ViewModels;

/// <summary>
/// 主窗口视图模型。承载编辑器状态与文件操作命令，遵循 MVVM：
/// 界面事件只在代码后置中转发到这里，业务逻辑全部在本类中。
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IFileService _fileService;
    private readonly ISettingsStore _settingsStore;

    /// <summary>当前打开的文件路径；未保存的新文件为 "Untitled"。</summary>
    [ObservableProperty]
    private string _currentFilePath = "Untitled";

    /// <summary>编辑器当前文本内容。</summary>
    [ObservableProperty]
    private string _editorContent = string.Empty;

    /// <summary>状态栏文本。</summary>
    [ObservableProperty]
    private string _statusText = "Ready";

    /// <summary>光标当前行号（从 1 开始）。</summary>
    [ObservableProperty]
    private int _currentLine = 1;

    /// <summary>光标当前列号（从 1 开始）。</summary>
    [ObservableProperty]
    private int _currentColumn = 1;

    /// <summary>当前内容是否有未保存的修改。</summary>
    [ObservableProperty]
    private bool _isModified;

    /// <summary>
    /// 使用文件服务与设置存储初始化 <see cref="MainViewModel"/>。
    /// </summary>
    /// <param name="fileService">文件读写服务。</param>
    /// <param name="settingsStore">设置存储。</param>
    /// <exception cref="ArgumentNullException">任一依赖为 <c>null</c> 时抛出。</exception>
    public MainViewModel(IFileService fileService, ISettingsStore settingsStore)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
    }

    /// <summary>新建文件：清空内容并重置为未命名状态。</summary>
    [RelayCommand]
    private void NewFile()
    {
        EditorContent = string.Empty;
        CurrentFilePath = "Untitled";
        IsModified = false;
        StatusText = "新建文件";
    }

    /// <summary>打开文件：弹出打开对话框，读取所选文件内容到编辑器。</summary>
    [RelayCommand]
    private async Task OpenFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "打开文件",
            Filter = "网页与脚本 (*.html;*.htm;*.js;*.css)|*.html;*.htm;*.js;*.css|所有文件 (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            EditorContent = await _fileService.ReadFileAsync(dialog.FileName);
            CurrentFilePath = dialog.FileName;
            IsModified = false;
            StatusText = $"已打开: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            StatusText = $"打开失败: {ex.Message}";
            MessageBox.Show(ex.Message, "打开失败", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>保存文件：已有路径则直接保存，否则弹出“另存为”对话框。</summary>
    [RelayCommand]
    private async Task SaveFile()
    {
        var path = CurrentFilePath;
        if (string.IsNullOrEmpty(path) || path == "Untitled")
        {
            var dialog = new SaveFileDialog
            {
                Title = "保存文件",
                Filter = "网页与脚本 (*.html;*.htm;*.js;*.css)|*.html;*.htm;*.js;*.css|所有文件 (*.*)|*.*",
                FileName = "index.html"
            };

            if (dialog.ShowDialog() != true)
                return;

            path = dialog.FileName;
        }

        try
        {
            await _fileService.WriteFileAsync(path, EditorContent);
            CurrentFilePath = path;
            IsModified = false;
            StatusText = $"已保存: {path}";
        }
        catch (Exception ex)
        {
            StatusText = $"保存失败: {ex.Message}";
            MessageBox.Show(ex.Message, "保存失败", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>退出应用。</summary>
    [RelayCommand]
    private void ExitApp()
    {
        Application.Current.Shutdown();
    }

    /// <summary>编辑器文本发生变化时由界面事件转发调用，标记内容已修改。</summary>
    public void NotifyEditorTextChanged()
    {
        IsModified = true;
    }

    /// <summary>光标位置变化时由界面事件转发调用，更新行列号。</summary>
    /// <param name="line">行号。</param>
    /// <param name="column">列号。</param>
    public void NotifyCaretPositionChanged(int line, int column)
    {
        CurrentLine = line;
        CurrentColumn = column;
    }

    // ---- 以下为占位命令：Edit / View 菜单，后续接入真实逻辑 ----

    /// <summary>撤销（占位）。</summary>
    [RelayCommand]
    private void Undo()
    {
    }

    /// <summary>重做（占位）。</summary>
    [RelayCommand]
    private void Redo()
    {
    }

    /// <summary>剪切（占位）。</summary>
    [RelayCommand]
    private void Cut()
    {
    }

    /// <summary>复制（占位）。</summary>
    [RelayCommand]
    private void Copy()
    {
    }

    /// <summary>粘贴（占位）。</summary>
    [RelayCommand]
    private void Paste()
    {
    }

    /// <summary>切换左侧面板显示（占位）。</summary>
    [RelayCommand]
    private void ToggleLeftPanel()
    {
    }

    /// <summary>切换右侧面板显示（占位）。</summary>
    [RelayCommand]
    private void ToggleRightPanel()
    {
    }

    /// <summary>显示“关于”信息。</summary>
    [RelayCommand]
    private void About()
    {
        MessageBox.Show("WFU — WebView For Unity\nM2 视觉层骨架", "关于",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
